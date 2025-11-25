using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit;

public class ChannelFallbackTests
{
  [Fact]
  public async Task Enqueue_Schedules_Write_When_Channel_Is_Full()
  {
    var opts = Options.Create(new WatcherOptions { ChannelCapacity = 1, DebounceMilliseconds = 0 });
    var config = new ConfigurationBuilder().AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("ComicsPath", ".") }).Build();
    var ch = new FileEventChannel(opts);
    var memory = new MemoryCache(new MemoryCacheOptions());
    var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<FileWatcher>();
    var w = new FileWatcher(opts, config, ch, logger, memory);

    // Fill the single-slot channel synchronously
    var first = new FileEvent(FileEventType.Created, "p1");
    Assert.True(ch.TryWrite(first));

    // Now call EnqueueEvent which should not block but will schedule an async write
    var second = new FileEvent(FileEventType.Created, "p2");

    var sw = Stopwatch.StartNew();
    w.EnqueueEvent(second);
    sw.Stop();

    // EnqueueEvent is synchronous and should return quickly (not blocked waiting for channel)
    Assert.True(sw.ElapsedMilliseconds < 200, "EnqueueEvent blocked unexpectedly");

    // Read the first item immediately
    Assert.True(ch.Reader.TryRead(out var r1));
    Assert.Equal(first.Path, r1.Path);

    // The scheduled async write should complete once there's space; wait for the second event
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
    var r2 = await ch.Reader.ReadAsync(cts.Token);
    Assert.Equal(second.Path, r2.Path);
  }
}
