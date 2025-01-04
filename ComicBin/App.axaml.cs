using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ComicBin.Data;
using ComicBin.ViewModels;
using ComicBin.Views;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ComicBin;

public partial class App : Application
{
  public IServiceProvider Services { get; private set; }

  public override void Initialize()
  {
    AvaloniaXamlLoader.Load(this);

    var servicesCollection  = new ServiceCollection();
    ConfigureServices(servicesCollection);
    Services = servicesCollection.BuildServiceProvider();
  }

  public void ConfigureServices(ServiceCollection services)
  {
    // Register the DatabaseHelper with the DI container
    string connectionString = "Data Source=app_data.db";
    services.AddSingleton<IDatabaseHelper>(new DatabaseHelper(connectionString));

    // Register ViewModels
    services.AddTransient<MainWindowViewModel>();
  }

  public override void OnFrameworkInitializationCompleted()
  {
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      desktop.MainWindow = new MainWindow
      {
        DataContext = Services.GetRequiredService<MainWindowViewModel>()
      };
    }

    base.OnFrameworkInitializationCompleted();
  }
}
