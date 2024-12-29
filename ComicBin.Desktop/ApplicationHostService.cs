using ComicBin.Core;
using ComicBin.Data;
using ComicBin.Desktop.Views;
using Microsoft.Extensions.Hosting;
using System.Windows.Navigation;

namespace ComicBin.Desktop
{
  internal class ApplicationHostService : IHostedService
  {
    private readonly IServiceProvider _serviceProvider;
    private readonly DatabaseIntializer _database;
    private readonly INavigationService _navigationService;

    public ApplicationHostService(IServiceProvider serviceProvider, DatabaseIntializer databaseIntializer, INavigationService _navigation)
    {
      _serviceProvider = serviceProvider;
      _database = databaseIntializer;
      _navigationService = _navigation;
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

      //var _navigationWindow = (
      //        _serviceProvider.GetService(typeof(INavigationWindow)) as INavigationWindow
      //    )!;

      //_navigationWindow!.ShowWindow();
      _navigationService.NavigateAsync(typeof(Home));
      var mainWindow = _serviceProvider.GetService(typeof(MainWindow)) as MainWindow;
      mainWindow.Show();

      //if (ApplicationSettings.IsSetUpComplete)
      //  _navigationWindow.Navigate(typeof(DashboardPage));
      //else
      //  _navigationWindow.Navigate(typeof(StartUpPage));
      await Task.CompletedTask;
    }
  }
}