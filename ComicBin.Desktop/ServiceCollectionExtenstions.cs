using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ComicBin.Data;
using ComicBin.Core;
using ComicBin.Core.Services;
using System.Configuration;
using ComicBin.Desktop.Views;
using System.Windows.Navigation;
using System.Windows.Controls;
using ComicBin.Desktop.Navigation;
using ComicBin.Desktop.ViewModels;

namespace ComicBin.Desktop;

public interface INavigationService
{
  Task NavigateAsync(Type pageType);
  Task NavigateAsync(Type pageType, object parameter);
  Task GoBackAsync();
  bool CanGoBack { get; }
}

public class NavigationService : INavigationService
{
  private readonly Frame _frame;

  public NavigationService(Frame frame)
  {
    _frame = frame ?? throw new ArgumentNullException(nameof(frame));
  }

  public bool CanGoBack => _frame.CanGoBack;

  public async Task NavigateAsync(Type pageType)
  {
    if (pageType == null)
      throw new ArgumentNullException(nameof(pageType));

    var pageInstance = Activator.CreateInstance(pageType);
    _frame.Navigate(pageInstance);
  }

  public async Task NavigateAsync(Type pageType, object parameter)
  {
    if (pageType == null)
      throw new ArgumentNullException(nameof(pageType));

    var pageInstance = Activator.CreateInstance(pageType);
    _frame.Navigate(pageInstance, parameter);
  }

  public async Task GoBackAsync()
  {
    if (CanGoBack)
    {
      _frame.GoBack();
    }
  }
}

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
    // Register views

    services.AddSingleton<MainWindow>();
    services.AddSingleton<NavigationStore>();
    services.AddTransient<HomePageViewModel>();
    services.AddTransient<HomePage>();
    services.AddTransient<SettingsPageViewModel>();
    services.AddTransient<SettingsPage>();
    return services;
  }

  public static IServiceCollection AddRepositories(this IServiceCollection services)
  {
    services.AddSingleton<ISettingsRepository, SettingsRepository>();
    return services;
  }
}

