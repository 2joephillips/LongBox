using LongBox.Services;
using LongBox.Data;
using LongBox.ViewModels;
using LongBox.ViewModels.Pages;
using LongBox.Views;
using LongBox.Views.Pages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LongBox.Extensions;


public enum ApplicationPageNames
{
  Unknown = 0,
  Home,
  Settings,
  About,
  SetUp,
  MainWindow
}

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddDatabase(this IServiceCollection services)
  {
    services.AddDbContext<LongBoxContext>(options =>
    {
      options.UseSqlite($"Data Source={ApplicationSettings.DatabasePath}");
    }); 
    
    services.AddTransient<DatabaseIntializer>();
    return services;
  }
  public static IServiceCollection AddServices(this IServiceCollection services)
  {
    services.AddSingleton<PageFactory>();
    services.AddTransient<IComicMetadataExtractor, ComicMetadataExtractor>();
    services.AddTransient<IFolderHandler, FolderHandler>();
    services.AddTransient<IApiKeyHandler, ApiKeyHandler>();
    services.AddTransient<ISystemStorage, SystemStorage>();
    services.AddSingleton<ISettingsRepository, SettingsRepository>();
    return services;
  }

  public static IServiceCollection AddViews(this IServiceCollection services)
  {
    services.AddTransient<MainWindow>();
    services.AddTransient<ReaderWindow>();
    services.AddTransient<SettingsPageView>();
    services.AddTransient<SetUpPageView>();
    return services;
  }

  public static IServiceCollection AddViewModels(this IServiceCollection services)
  {

    services.AddSingleton<HomePageViewModel>();
    services.AddSingleton<SettingsPageViewModel>();
    services.AddSingleton<AboutPageViewModel>();
    services.AddSingleton<SetUpPageViewModel>();
    services.AddTransient<MainWindowViewModel>();
    services.AddTransient<ReaderViewModel>();

    services.AddSingleton<Func<ApplicationPageNames, PageViewModel>>(services => name => name switch
    {
      ApplicationPageNames.Home => services.GetRequiredService<HomePageViewModel>(),
      ApplicationPageNames.Settings => services.GetRequiredService<SettingsPageViewModel>(),
      ApplicationPageNames.About => services.GetRequiredService<AboutPageViewModel>(),
      ApplicationPageNames.SetUp => services.GetRequiredService<SetUpPageViewModel>(),
      _ => throw new ArgumentException("Invalid page name", nameof(name))
    });
    return services;
  }

  public class PageFactory(Func<ApplicationPageNames, PageViewModel> factory)
  {
    public PageViewModel GetPageViewModel(ApplicationPageNames pageName) => factory.Invoke(pageName);
  }
}