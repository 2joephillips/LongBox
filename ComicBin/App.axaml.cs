using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ComicBin.Core;
using ComicBin.Core.Services;
using ComicBin.Data;
using ComicBin.Extensions;
using ComicBin.ViewModels;
using ComicBin.ViewModels.Pages;
using ComicBin.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace ComicBin;

public partial class App : Application
{
  public IServiceProvider Services { get; private set; }

  public override void Initialize()
  {
    AvaloniaXamlLoader.Load(this);

    var servicesCollection = new ServiceCollection();
    servicesCollection
      .AddDatabase()
      .AddServices()
      .AddViewModels();

    Services = servicesCollection.BuildServiceProvider();
  }

  public override void OnFrameworkInitializationCompleted()
  {
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      var databaseHandler = Services.GetRequiredService<DatabaseIntializer>();

      databaseHandler.EnsureCreated();
      var settings = databaseHandler.InitializeSettings().Result;
      ApplicationSettings.Apply(settings);

      desktop.MainWindow = new MainWindow
      {
        DataContext = Services.GetRequiredService<MainWindowViewModel>()
      };

    }

    base.OnFrameworkInitializationCompleted();
  }
}
