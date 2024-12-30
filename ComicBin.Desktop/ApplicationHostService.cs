using ComicBin.Core;
using ComicBin.Data;
using ComicBin.Desktop.Navigation;
using ComicBin.Desktop.ViewModels;
using ComicBin.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows.Navigation;

namespace ComicBin.Desktop
{
  internal class ApplicationHostService : IHostedService
  {
    private readonly IServiceProvider _serviceProvider;
    private readonly DatabaseIntializer _database;
    private readonly NavigationStore _navigationStore;

    public ApplicationHostService(IServiceProvider serviceProvider, DatabaseIntializer databaseIntializer, NavigationStore navigationStore)
    {
      _serviceProvider = serviceProvider;
      _database = databaseIntializer;
      _navigationStore = navigationStore;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      await HandleActivationAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task HandleActivationAsync()
    {
      ApplicationSettings.EnsureAppDataFolderExists();

      _database.EnsureCreated();

      var settings = await _database.InitializeSettings();
      ApplicationSettings.Apply(settings);

      //ApplicationThemeManager.Apply(ApplicationSettings.CurrentTheme);

      var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
      mainWindow.Show();
      _navigationStore.CurrentViewModel = _serviceProvider.GetRequiredService<HomePageViewModel>();
      //if (ApplicationSettings.IsSetUpComplete)
      //  _navigationWindow.Navigate(typeof(DashboardPage));
      //else
      //  _navigationWindow.Navigate(typeof(StartUpPage));
      await Task.CompletedTask;
    }
  }
}