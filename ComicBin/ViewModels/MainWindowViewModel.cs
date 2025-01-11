using ComicBin.Core.Services;
using ComicBin.ViewModels.Pages;
using DynamicData;
using DynamicData.Kernel;
using ReactiveUI;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ComicBin.ViewModels;

public class MainWindowViewModel : ViewModelBase
{

  public ICommand OpenReaderCommand { get; }

  private readonly PageViewModelBase[] Pages;
  private PageViewModelBase? _CurrentPage;

  public MainWindowViewModel()
  {
    
  }

  public MainWindowViewModel(HomePageViewModel homePageViewModel,
                             SettingsPageViewModel settingsPageViewModel,
                             AboutPageViewModel aboutPageViewModel,
                              SetUpPageViewModel setUpPageViewModel)
  {
    var isSetUpComplete = ApplicationSettings.IsSetUpComplete;

    Pages = [homePageViewModel, settingsPageViewModel, aboutPageViewModel, setUpPageViewModel];

    _CurrentPage = isSetUpComplete ? Pages.First(): Pages.LastOrDefault();



    var canNavNext = this.WhenAnyValue(x => x.CurrentPage).Select(currentPage => currentPage is { CanNavigateNext: true });
    var canNavPrev = this.WhenAnyValue(x => x.CurrentPage).Select(currentPage => currentPage is { CanNavigatePrevious: true }); 

    NavigateNextCommand = ReactiveCommand.Create(NavigateNext, canNavNext);
    NavigatePreviousCommand = ReactiveCommand.Create(NavigatePrevious, canNavPrev);

    OpenReaderCommand = ReactiveCommand.CreateFromTask(async () =>
    {
      await Task.Run(() =>
      {
        var window = new ReaderWindow() { DataContext = new ReaderViewModel() };
        window.Show();
      });

    });
  }

  public PageViewModelBase? CurrentPage
  {
    get { return _CurrentPage; }
    private set { this.RaiseAndSetIfChanged(ref _CurrentPage, value); }
  }

  public ICommand NavigateNextCommand { get; }

  private void NavigateNext()
  {
    var index = Pages.IndexOf(CurrentPage) + 1;
    CurrentPage = Pages[index];
  }

  public ICommand NavigatePreviousCommand { get; }

  private void NavigatePrevious()
  {
    var index = Pages.IndexOf(CurrentPage) - 1;
    CurrentPage = Pages[index];
  }
}

