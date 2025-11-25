using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Xunit;

public class FileWatcherTests
{
  private FileWatcher CreateWatcher(int debounceMs)
  {
    var opts = Options.Create(new WatcherOptions { DebounceMilliseconds = debounceMs });
    var config = new ConfigurationBuilder().AddInMemoryCollection(new[] { new KeyValuePair<string, string>("ComicsPath", ".") }).Build();
    var channel = new FileEventChannel(opts);
    var memory = new MemoryCache(new MemoryCacheOptions());
    var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<FileWatcher>();
    return new FileWatcher(opts, config, channel, logger, memory);
  }

  [Fact]
  public void Debounce_Suppresses_Quick_Successive_Events()
  {
    var watcher = CreateWatcher(200);
    var channel = new FileEventChannel(Options.Create(new WatcherOptions()));
    // Use the same memory cache that the watcher has
    var memory = new MemoryCache(new MemoryCacheOptions());
    var opts = Options.Create(new WatcherOptions { DebounceMilliseconds = 200 });
    var config = new ConfigurationBuilder().AddInMemoryCollection(new[] { new KeyValuePair<string, string>("ComicsPath", ".") }).Build();
    var ch = new FileEventChannel(opts);
    var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<FileWatcher>();
    var w = new FileWatcher(opts, config, ch, logger, memory);

    var ev1 = new FileEvent(FileEventType.Created, "path1", null, DateTime.UtcNow);
    w.EnqueueEvent(ev1);
    Assert.True(ch.Reader.TryRead(out var r1));

    var ev2 = new FileEvent(FileEventType.Changed, "path1", null, DateTime.UtcNow);
    w.EnqueueEvent(ev2);
    // immediate second event should be suppressed by debounce
    Assert.False(ch.Reader.TryRead(out var r2));
  }

  [Fact]
  public void Rename_Carries_Over_Debounce_Recent_OldPath_Suppresses()
  {
    var opts = Options.Create(new WatcherOptions { DebounceMilliseconds = 500 });
    var config = new ConfigurationBuilder().AddInMemoryCollection(new[] { new KeyValuePair<string, string>("ComicsPath", ".") }).Build();
    var ch = new FileEventChannel(opts);
    var memory = new MemoryCache(new MemoryCacheOptions());
    var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<FileWatcher>();
    var w = new FileWatcher(opts, config, ch, logger, memory);

    // simulate recent activity on old path
    memory.Set("old/path", DateTime.UtcNow);

    var rename = new FileEvent(FileEventType.Renamed, "new/path", "old/path", DateTime.UtcNow);
    w.EnqueueEvent(rename);

    // because old path was recent, rename should be suppressed
    Assert.False(ch.Reader.TryRead(out var _));
  }

  [Fact]
  public void Rename_Carries_Over_Debounce_OldPath_Older_Allows()
  {
    var opts = Options.Create(new WatcherOptions { DebounceMilliseconds = 200 });
    var config = new ConfigurationBuilder().AddInMemoryCollection(new[] { new KeyValuePair<string, string>("ComicsPath", ".") }).Build();
    var ch = new FileEventChannel(opts);
    var memory = new MemoryCache(new MemoryCacheOptions());
    var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<FileWatcher>();
    var w = new FileWatcher(opts, config, ch, logger, memory);

    // simulate old path last seen long ago
    memory.Set("old/path", DateTime.UtcNow - TimeSpan.FromSeconds(10));

    var rename = new FileEvent(FileEventType.Renamed, "new/path", "old/path", DateTime.UtcNow);
    w.EnqueueEvent(rename);

    // because old path was old, rename should be queued
    Assert.True(ch.Reader.TryRead(out var ev));
    Assert.Equal(FileEventType.Renamed, ev.EventType);
    Assert.Equal("new/path", ev.Path);
  }
}
