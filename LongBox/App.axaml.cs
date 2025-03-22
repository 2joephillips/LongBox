using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using LongBox.Services;
using LongBox.Data;
using LongBox.Extensions;
using LongBox.ViewModels;
using LongBox.Views;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: XmlnsDefinition("https://github.com/avaloniaui", "LongBox.Controls")]

namespace LongBox;

public class App : Application
{
  private IServiceProvider Services { get; set; } = null!;

  public override void Initialize()
  {
    AvaloniaXamlLoader.Load(this);

    var servicesCollection = new ServiceCollection();
    servicesCollection
      .AddDatabase()
      .AddServices()
      .AddViews()
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
      ImageHandler.CreateDefaultImages(ApplicationSettings.AppDataPath);
      desktop.MainWindow = new MainWindow
      {
        DataContext = Services.GetRequiredService<MainWindowViewModel>()
      };

    }

    base.OnFrameworkInitializationCompleted();
  }
}
