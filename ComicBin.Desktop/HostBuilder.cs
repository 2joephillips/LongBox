using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Reflection;

namespace ComicBin.Desktop;

internal class HostBuilder
{
  internal static IHost Build()
  {
    return Host
       .CreateDefaultBuilder()
       .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)); })
       .ConfigureServices((context, services) =>
       {
         // Register DbContext
         services
              .AddDatabase()
              .AddServices()
              .AddUIComponents()
              .AddRepositories();
       })
       .Build();
  }
}
