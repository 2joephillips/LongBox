
using ReactiveUI;

using System.Windows.Input;
using LongBox.Services;
using static LongBox.Extensions.ServiceCollectionExtensions;
using LongBox.Extensions;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using LongBox.Domain;
using System.Linq;

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


  public MainWindowViewModel(PageFactory pageFactory,
      IComicMetadataExtractor extractor)
  {
    var isSetUpComplete = ApplicationSettings.IsSetUpComplete;


    _CurrentPage = isSetUpComplete ? pageFactory.GetPageViewModel(ApplicationPageNames.Home) : pageFactory.GetPageViewModel(ApplicationPageNames.SetUp);

    ShowReaderDialog = new Interaction<ReaderViewModel, ComicViewModel?>();
    OpenReaderCommand = ReactiveCommand.CreateFromTask(async () =>
    {
      var toplevel = TopLevel.GetTopLevel(new ReaderWindow());
      var pickedComic = await toplevel?.StorageProvider?.OpenFilePickerAsync(new FilePickerOpenOptions())!;
      var comicPath = pickedComic.FirstOrDefault()?.TryGetLocalPath() ?? string.Empty;
      var  comic = new Comic(new CBZFile(comicPath), extractor);
      var readerViewModel = new ReaderViewModel(extractor, comic);
      var comicViewModel = await ShowReaderDialog.Handle(readerViewModel);
    });
  }


}

