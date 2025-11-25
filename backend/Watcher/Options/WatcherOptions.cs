public class WatcherOptions
{
  public bool IncludeSubdirectories { get; set; } = true;
  public int DebounceMilliseconds { get; set; } = 500;
  public int ChannelCapacity { get; set; } = 100;
  public int ProcessorCount { get; set; } = Environment.ProcessorCount;
  public FileAccessRetryOptions FileAccessRetryOptions { get; set; } = new FileAccessRetryOptions();

}
