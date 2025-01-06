using ComicBin.Core;
using ComicBin.Core.Services;
using ComicBin.Data;
using ComicBin.ViewModels;
using ComicBin.ViewModels.Pages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ComicBin.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddDatabase(this IServiceCollection services)
  {
    services.AddDbContext<ComicBinContext>(options =>
    {
      options.UseSqlite($"Data Source={ApplicationSettings.DatabasePath}");
    }); 
    
    services.AddTransient<DatabaseIntializer>();
    return services;
  }
  public static IServiceCollection AddServices(this IServiceCollection services)
  {
    services.AddTransient<IComicMetadataExtractor, ComicMetadataExtractor>();
    services.AddTransient<ISystemStorage, SystemStorage>();
    services.AddSingleton<ISettingsRepository, SettingsRepository>();
    return services;
  }
  public static IServiceCollection AddViewModels(this IServiceCollection services)
  {

    services.AddSingleton<HomePageViewModel>();
    services.AddSingleton<SettingsPageViewModel>();
    services.AddSingleton<AboutPageViewModel>();
    services.AddSingleton<SetUpPageViewModel>();
    services.AddTransient<MainWindowViewModel>();
    return services;
  }
}