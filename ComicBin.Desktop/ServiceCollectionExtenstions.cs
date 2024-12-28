using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ComicBin.Data;
using ComicBin.Core;
using ComicBin.Core.Services;

namespace ComicBin.Desktop;

public static class ServiceCollectionExtenstions
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

    services.AddHostedService<ApplicationHostService>();
    //services.AddSingleton<IPageService, PageService>();
    //services.AddSingleton<IThemeService, ThemeService>();
    //services.AddSingleton<ITaskBarService, TaskBarService>();
    //services.AddSingleton<ISnackbarService, SnackbarService>();
    //services.AddSingleton<INavigationService, NavigationService>();
    services.AddTransient<IComicMetadataExtractor, ComicMetadataExtractor>();
    services.AddTransient<ISystemStorage, SystemStorage>();
    return services;
  }

  public static IServiceCollection AddUIComponents(this IServiceCollection services)
  {
    //services.AddSingleton<INavigationWindow, MainWindow>();
    //services.AddSingleton<MainWindowViewModel>();
    //services.AddTransient<Reader>();
    //services.AddSingleton<ReaderViewModel>();

    //services.AddSingleton<StartUpPage>();
    //services.AddSingleton<StartUpViewModel>();
    return services;
  }

  public static IServiceCollection AddRepositories(this IServiceCollection services)
  {
    services.AddSingleton<ISettingsRepository, SettingsRepository>();
    return services;
  }
}

