using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

public class FileWatcherService : BackgroundService
{
  private readonly FileWatcher _watcher;
  private readonly FileEventChannel _channel;
  private readonly IServiceProvider _providers;
  private readonly WatcherOptions _opts;
  private readonly ILogger<FileWatcherService> _log;
  private readonly IConfiguration _config;
  private readonly IMemoryCache _lockCache;
  private readonly TimeSpan _semaphoreSliding = TimeSpan.FromMinutes(2);

  public FileWatcherService(IConfiguration config, FileWatcher watcher, FileEventChannel channel,
    IOptions<WatcherOptions> opts, IServiceProvider providers, ILogger<FileWatcherService> log, IMemoryCache lockCache)
  {
    _watcher = watcher;
    _channel = channel;
    _opts = opts.Value;
    _providers = providers;
    _log = log;
    _config = config;
    _lockCache = lockCache;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _log.LogInformation("Hosted service starting. ProcessorCount={Count}", _opts.ProcessorCount);

    _watcher.Start();

    var workers = new List<Task>();
    for (int i = 0; i < Math.Max(1, _opts.ProcessorCount); i++)
    {
      workers.Add(WorkerLoop(stoppingToken));
    }

    try
    {
      await Task.WhenAll(workers);
    }
    catch (OperationCanceledException) { }
    finally
    {
      _watcher.Stop();
    }
  }

  private SemaphoreSlim GetSemaphore(string path)
  {
    return _lockCache.GetOrCreate<SemaphoreSlim>(path, entry =>
    {
      entry.SetSlidingExpiration(_semaphoreSliding);
      entry.RegisterPostEvictionCallback((k, v, reason, state) =>
      {
        if (v is SemaphoreSlim s) s.Dispose();
      });
      return new SemaphoreSlim(1, 1);
    });
  }

  private async Task WorkerLoop(CancellationToken ct)
  {
    await foreach (var ev in _channel.Reader.ReadAllAsync(ct))
    {
      var sem = GetSemaphore(ev.Path);
      await sem.WaitAsync(ct);
      try
      {
        using var scope = _providers.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<IFileProcessor>();
        await ProcessWithRetries(processor, ev, ct);
      }
      catch (Exception ex)
      {
        _log.LogError(ex, "Unhandled exception processing {Path}", ev.Path);
      }
      finally
      {
        sem.Release();
      }
    }
  }

  private async Task ProcessWithRetries(IFileProcessor processor, FileEvent ev, CancellationToken ct)
  {
    var retryOpts = _opts.FileAccessRetryOptions;
    var attempt = 0;
    var delay = retryOpts.DelayMilliseconds;

    while (true)
    {
      attempt++;
      try
      {
        // attempt to access file quickly before processing for Created/Changed
        if (ev.EventType == FileEventType.Created || ev.EventType == FileEventType.Changed)
        {
          await EnsureFileAccessible(ev.Path, retryOpts.MaxRetries, retryOpts.DelayMilliseconds, ct);
        }

        await processor.ProcessAsync(ev, ct);
        return;
      }
      catch (OperationCanceledException) when (ct.IsCancellationRequested)
      {
        throw;
      }
      catch (Exception ex) when (attempt < retryOpts.MaxRetries)
      {
        _log.LogWarning(ex, "Attempt {Attempt} failed for {Path}. Retrying in {Delay}ms", attempt, ev.Path, delay);
        await Task.Delay(delay, ct);
        delay = Math.Min(delay * 2, 5000); // exponential backoff, capped at 5 seconds
      }
      catch (Exception ex)
      {
        _log.LogError(ex, "Failed to process {Path} after {Attempts} attempts", ev.Path, attempt);
        return;
      }
    }
  }

  private static async Task EnsureFileAccessible(string path, int maxAttempts, int initialDelayMs, CancellationToken ct)
  {
    var delay = initialDelayMs;

    for (int attempt = 0; attempt < maxAttempts; attempt++)
    {
      try
      {
        // Try open for read with shared read/write
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return; // File is accessible
      }
      catch (FileNotFoundException)
      {
        // File may be moved or deleted between event and attempt
        throw;
      }
      catch (IOException) when (attempt < maxAttempts - 1)
      {
        // Locked by another process — wait and retry
        await Task.Delay(delay, ct);
        delay = Math.Min(delay * 2, 5000); // exponential backoff, capped at 5 seconds
      }
    }

    // Final attempt that will throw if still inaccessible
    using var final = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
  }



}