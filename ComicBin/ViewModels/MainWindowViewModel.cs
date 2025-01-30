using ComicBin.Core.Services;
using ComicBin.ViewModels.Pages;
using DynamicData;
using DynamicData.Kernel;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ComicBin.ViewModels;

public class MainWindowViewModel : ViewModelBase
{

  public ICommand OpenReaderCommand { get; }

  private ViewModelBase? _CurrentPage;
  public ViewModelBase? CurrentPage
  {
    get { return _CurrentPage; }
    private set { this.RaiseAndSetIfChanged(ref _CurrentPage, value); }
  }

  private Dictionary<string, ViewModelBase>? ViewDirectory;

  public MainWindowViewModel()
  {
  }

  public MainWindowViewModel(HomePageViewModel homePageViewModel,
                             SettingsPageViewModel settingsPageViewModel,
                             AboutPageViewModel aboutPageViewModel,
                              SetUpPageViewModel setUpPageViewModel)
  {
    var isSetUpComplete = ApplicationSettings.IsSetUpComplete;

    ViewDirectory = new Dictionary<string, ViewModelBase>
    {
      { "home", homePageViewModel },
      { "settings", settingsPageViewModel },
      { "about", aboutPageViewModel },
      { "setup", setUpPageViewModel }
    };

    _CurrentPage = isSetUpComplete ? ViewDirectory["home"] : ViewDirectory["setup"];

    OpenReaderCommand = ReactiveCommand.CreateFromTask(async () =>
    {
      await Task.Run(() =>
      {
        var window = new ReaderWindow() { DataContext = new ReaderViewModel() };
        window.Show();
      });
    });
  }


}

