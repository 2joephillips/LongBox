public class FileAccessRetryOptions
{
  public int MaxRetries { get; set; } = 5;
  public int DelayMilliseconds { get; set; } = 200;
}