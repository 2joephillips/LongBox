
using ReactiveUI;
using System.Collections.Generic;

using System.Reactive.Linq;

using System.Windows.Input;
using LongBox.Services;
using LongBox.ViewModels.Pages;

namespace LongBox.ViewModels;

public class MainWindowViewModel : ViewModelBase
{

  public ICommand OpenReaderCommand { get; }
  public Interaction<ReaderViewModel, ComicViewModel?> ShowReaderDialog { get; }

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
                              SetUpPageViewModel setUpPageViewModel, 
                              IComicMetadataExtractor extractor)
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

    ShowReaderDialog = new Interaction<ReaderViewModel, ComicViewModel?>();
    OpenReaderCommand = ReactiveCommand.CreateFromTask(async () =>
    {
      var readerViewModel = new ReaderViewModel(extractor);
      var comicViewModel = await ShowReaderDialog.Handle(readerViewModel);
    });
  }


}

