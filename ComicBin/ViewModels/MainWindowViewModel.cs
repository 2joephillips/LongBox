using ComicBin.Core;
using ComicBin.ViewModels.Pages;
using DynamicData;
using ReactiveUI;
using System.Windows.Input;

namespace ComicBin.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
  public Interaction<MainWindowViewModel, ReaderViewModel?> ShowDialog { get; }
  public ICommand OpenReaderCommand { get; }

  private readonly PageViewModelBase[] Pages;
  private PageViewModelBase _CurrentPage;

  public MainWindowViewModel(HomePageViewModel homePageViewModel,
                             SettingsPageViewModel settingsPageViewModel,
                             AboutPageViewModel aboutPageViewModel)
  {
    var isSetUpComplete = ApplicationSettings.IsSetUpComplete;

    Pages =
    [
            homePageViewModel,
                settingsPageViewModel,
                aboutPageViewModel
    ];

    _CurrentPage = Pages[0];

    var canNavNext = this.WhenAnyValue(x => x.CurrentPage.CanNavigateNext);
    var canNavPrev = this.WhenAnyValue(x => x.CurrentPage.CanNavigatePrevious);

    NavigateNextCommand = ReactiveCommand.Create(NavigateNext, canNavNext);
    NavigatePreviousCommand = ReactiveCommand.Create(NavigatePrevious, canNavPrev);

    OpenReaderCommand = ReactiveCommand.CreateFromTask(async () =>
    {
      var window = new ReaderWindow() { DataContext = new ReaderViewModel() };
      window.Show();
    });
  }

  public PageViewModelBase CurrentPage
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

