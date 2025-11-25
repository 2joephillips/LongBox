using Serilog;

public interface IFileProcessor
{
  Task ProcessAsync(FileEvent ev, CancellationToken ct);
}


public class ComicFileProcessor : IFileProcessor
{
  private readonly ILogger<ComicFileProcessor> _log;

  public ComicFileProcessor(ILogger<ComicFileProcessor> log)
  {
    _log = log;
  }

  public async Task ProcessAsync(FileEvent ev, CancellationToken ct)
  {
    _log.LogInformation("Processing {Type} {Path}", ev.EventType, ev.Path);

    // Example: if file was added or changed -> index it
    if (ev.EventType == FileEventType.Created || ev.EventType == FileEventType.Changed)
    {
      Log.Information("Indexing comic file: {Path}", ev.Path);
      await Task.Delay(50, ct); // placeholder for real work
    }
    else if (ev.EventType == FileEventType.Deleted)
    {
      // remove from index
      Log.Information("Removing comic file from index: {Path}", ev.Path);
      await Task.Delay(10, ct);
    }
    else if (ev.EventType == FileEventType.Renamed)
    {
      // handle rename (ev.OldPath available)
      Log.Information("Renaming comic file in index: {OldPath} -> {NewPath}", ev.OldPath, ev.Path);
      await Task.Delay(10, ct);
    }
  }
}