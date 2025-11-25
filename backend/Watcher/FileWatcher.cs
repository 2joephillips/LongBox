using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;

public class FileWatcher : IDisposable
{
  private readonly FileSystemWatcher _watcher;
  private readonly FileEventChannel _channel;
  private readonly WatcherOptions _opts;
  private readonly TimeSpan _debounce;
  private readonly IMemoryCache _lastSeenCache;
  private readonly ILogger<FileWatcher> _log;
  private readonly string _path;


  public FileWatcher(IOptions<WatcherOptions> opts, IConfiguration config, FileEventChannel channel, ILogger<FileWatcher> log, IMemoryCache cache)
  {
    _path = config.GetValue<string>("ComicsPath") ?? throw new ArgumentException("ComicsPath configuration value is required.");
    _opts = opts.Value;
    _channel = channel;
    _log = log;
    _debounce = TimeSpan.FromMilliseconds(_opts.DebounceMilliseconds);
    _lastSeenCache = cache;

    _watcher = new FileSystemWatcher(_path)
    {
      IncludeSubdirectories = _opts.IncludeSubdirectories,
      NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite
    };

    _watcher.Created += OnFsEvent;
    _watcher.Changed += OnFsEvent;
    _watcher.Deleted += OnFsEvent;
    _watcher.Renamed += OnRenamed;
  }

  public void Start()
  {
    _log.LogInformation("Starting FileWatcher on {Path}", _path);
    _watcher.EnableRaisingEvents = true;
  }

  public void Stop()
  {
    _log.LogInformation("Stopping FileWatcher");
    _watcher.EnableRaisingEvents = false;
  }

  private void OnFsEvent(object s, FileSystemEventArgs e)
  {
    EnqueueEvent(Map(e.ChangeType, e.FullPath));
  }

  private void OnRenamed(object s, RenamedEventArgs e)
  {
    var ev = new FileEvent(FileEventType.Renamed, e.FullPath, e.OldFullPath, DateTime.UtcNow);
    EnqueueEvent(ev);
  }

  private FileEvent Map(WatcherChangeTypes change, string fullPath)
       => change switch
       {
         WatcherChangeTypes.Created => new FileEvent(FileEventType.Created, fullPath, null, DateTime.UtcNow),
         WatcherChangeTypes.Deleted => new FileEvent(FileEventType.Deleted, fullPath, null, DateTime.UtcNow),
         WatcherChangeTypes.Changed => new FileEvent(FileEventType.Changed, fullPath, null, DateTime.UtcNow),
         _ => new FileEvent(FileEventType.Changed, fullPath, null, DateTime.UtcNow)
       };

  internal void EnqueueEvent(FileEvent ev)
  {
    var now = DateTime.UtcNow;

    // If this is a rename, carry over debounce state from old path (if any)
    if (ev.EventType == FileEventType.Renamed && ev.OldPath != null)
    {
      if (_lastSeenCache.TryGetValue(ev.OldPath, out DateTime oldTime))
      {
        _lastSeenCache.Set(ev.Path, oldTime, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(60)));
      }
      else
      {
        _lastSeenCache.Set(ev.Path, now, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(60)));
      }
    }

    // simple debounce per path (store timestamp in memory cache with sliding expiration)
    if (!_lastSeenCache.TryGetValue(ev.Path, out DateTime last))
    {
      last = DateTime.MinValue;
    }

    if (now - last < _debounce)
    {
      _lastSeenCache.Set(ev.Path, now, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(60)));
      return;
    }

    _lastSeenCache.Set(ev.Path, now, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(60)));

    // Non-blocking attempt: try sync write, otherwise schedule async write
    if (!_channel.TryWrite(ev))
    {
      _ = _channel.WriteAsync(ev, CancellationToken.None).AsTask(); // queue asynchronously
    }
  }

  public void Dispose()
  {
    _watcher.Created -= OnFsEvent;
    _watcher.Changed -= OnFsEvent;
    _watcher.Deleted -= OnFsEvent;
    _watcher.Renamed -= OnRenamed;
    _watcher.Dispose();
  }
}