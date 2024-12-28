using ComicBin.Core;
using ComicBin.Data;
using Microsoft.Extensions.Hosting;
using System.Windows;
using System.Windows.Navigation;

namespace ComicBin.Desktop
{
  internal class ApplicationHostService : IHostedService
  {
    private readonly IServiceProvider _serviceProvider;
    private readonly DatabaseIntializer _database;
    //private readonly INavigationWindow _navigationWindow;

    public ApplicationHostService(IServiceProvider serviceProvider, DatabaseIntializer databaseIntializer)
      //, INavigationWindow navigationWindow)
    {
      _serviceProvider = serviceProvider;
      _database = databaseIntializer;
      //_navigationWindow = navigationWindow;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      await HandleActivationAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task HandleActivationAsync()
    {
      var mainWindow = Application.Current.Windows.OfType<MainWindow>();

      ApplicationSettings.EnsureAppDataFolderExists();

      _database.EnsureCreated();

      var settings = await _database.InitializeSettings();
      ApplicationSettings.Apply(settings);

      //ApplicationThemeManager.Apply(ApplicationSettings.CurrentTheme);

      //var _navigationWindow = (
      //        _serviceProvider.GetService(typeof(INavigationWindow)) as INavigationWindow
      //    )!;

      //_navigationWindow!.ShowWindow();
      mainWindow.First().Show();
      //if (ApplicationSettings.IsSetUpComplete)
      //  _navigationWindow.Navigate(typeof(DashboardPage));
      //else
      //  _navigationWindow.Navigate(typeof(StartUpPage));
      await Task.CompletedTask;
    }
  }
}