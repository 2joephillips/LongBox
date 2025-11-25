using System.Threading.Channels;
using Microsoft.Extensions.Options;

public enum FileEventType { Created, Changed, Deleted, Renamed }

public record FileEvent(FileEventType EventType, string Path, string? OldPath = null, DateTime OccurredUtc = default);


public class FileEventChannel
{
  private readonly Channel<FileEvent> _channel;
  public FileEventChannel(IOptions<WatcherOptions> options)
  {
    var capacity = options.Value.ChannelCapacity;
    if (capacity <= 0)
    {
      throw new ArgumentException($"Channel capacity must be greater than 0, but was {capacity}", nameof(options));
    }
    var channelOptions = new BoundedChannelOptions(capacity)
    {
      SingleReader = false,
      SingleWriter = false,
      FullMode = BoundedChannelFullMode.Wait
    };
    _channel = Channel.CreateBounded<FileEvent>(channelOptions);
  }
  public ValueTask WriteAsync(FileEvent item, CancellationToken ct) => _channel.Writer.WriteAsync(item, ct);
  public bool TryWrite(FileEvent item) => _channel.Writer.TryWrite(item);
  public ChannelReader<FileEvent> Reader => _channel.Reader;
}