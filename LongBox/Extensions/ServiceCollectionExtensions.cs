using LongBox.Services;
using LongBox.Data;
using LongBox.ViewModels;
using LongBox.ViewModels.Pages;
using LongBox.Views;
using LongBox.Views.Pages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LongBox.Extensions;

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
    return services;
  }
}