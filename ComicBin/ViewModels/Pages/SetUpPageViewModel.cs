using ComicBin.Core.Models;
using ComicBin.Core.Services;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using Avalonia.Media.Imaging;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using ComicBin.Services;

namespace ComicBin.ViewModels.Pages;



public class SetUpPageViewModel : ViewModelBase
{
  private readonly IComicMetadataExtractor _extractor;
  private readonly IApiKeyHandler _apiKeyHandler;
  private readonly IFolderHandler _folderHandler;
  private ObservableCollection<Comic> _comicCollection = new();
  public ObservableCollection<Comic> ComicCollection
  {
    get => _comicCollection;
    set => this.RaiseAndSetIfChanged(ref _comicCollection, value);
  }

  private string _rootFolder = string.Empty;
  public string RootFolder
  {
    get => _rootFolder;
    private set => this.RaiseAndSetIfChanged(ref _rootFolder, value);
  }

  private string _comicVineApiKey = string.Empty;
  public string ComicVineApiKey
  {
    get => _comicVineApiKey;
    set => this.RaiseAndSetIfChanged(ref _comicVineApiKey, value);
  }

  private ApiKeyValidationResult _apiKeyStatus;
  public ApiKeyValidationResult ApiKeyStatus
  {
    get => _apiKeyStatus;
    set => this.RaiseAndSetIfChanged(ref _apiKeyStatus, value);
  }

  private Bitmap _currentImagePath = ApplicationSettings.DefaultHighResImage;
  public Bitmap CurrentImagePath
  {
    get => _currentImagePath;
    set => this.RaiseAndSetIfChanged(ref _currentImagePath, value);
  }

  private FolderScanningProgress _scanningProgress = new(false, 0, 0, string.Empty);
  public FolderScanningProgress ScanningProgress
  {
    get => _scanningProgress;
    set => this.RaiseAndSetIfChanged(ref _scanningProgress, value);
  }

  private bool _scanningInProgress;
  public bool ScanningInProgress
  {
    get => _scanningInProgress;
    set => this.RaiseAndSetIfChanged(ref _scanningInProgress, value);
  }



  public ICommand SelectFolderCommand { get; }
  public ICommand OpenComicVineSiteCommand { get; }
  public ICommand VerifyApiKeyCommand { get; }
  public ICommand ScanFolderCommand { get; }

  public SetUpPageViewModel()
  {
    var folderHandler = new FolderHandler();
    RootFolder = ApplicationSettings.RootFolder ?? folderHandler.FolderNotSelected;
    ApiKeyStatus = new ApiKeyValidationResult(false, "Not Validated");
    ComicVineApiKey = "fake-example-key-b355-0748f2b71e68";
    ScanningProgress = new FolderScanningProgress(true, 2, 10, "scanning");
    var comic = new Comic()
    {
      CoverImagePaths = (
        ThumbnailPath: "C:\\Users\\Josep\\AppData\\Local\\ComicRack\\dbd3c688-af7b-481d-a116-630d2a396c1b_thumbnail.jpg",
        HighResPath: "C:\\Users\\Josep\\AppData\\Local\\ComicRack\\55bfabb8-8557-4c4e-b459-35f10bbcdd9a_highres.jpg"
        ),
      FileName = "fake-file-name",
    };
    ComicCollection = [comic];
  }

  public SetUpPageViewModel(IComicMetadataExtractor comicMetadataExtractor, IApiKeyHandler apiKeyHandler, IFolderHandler folderHandler)
  {
    _extractor = comicMetadataExtractor;
    _apiKeyHandler = apiKeyHandler;
    _folderHandler = folderHandler;
    ComicCollection = [];
    RootFolder = ApplicationSettings.RootFolder ?? _folderHandler.FolderNotSelected;
    ApiKeyStatus = new ApiKeyValidationResult(false, "Not Validated");

    SelectFolderCommand = ReactiveCommand.CreateFromTask(SelectRootFolder);

    var canVerifyApiKey = this.WhenAnyValue(x => x.ComicVineApiKey).Select(apiKey => !string.IsNullOrEmpty(apiKey));
    VerifyApiKeyCommand = ReactiveCommand.CreateFromTask(VerifyApiKey, canVerifyApiKey);

    ScanFolderCommand = ReactiveCommand.CreateFromTask(ScanFolder);
    OpenComicVineSiteCommand = ReactiveCommand.Create(_apiKeyHandler.OpenComicVineSite);


    if (RootFolder != _folderHandler.FolderNotSelected)
      Dispatcher.UIThread.InvokeAsync(async () => { ScanningProgress = await _folderHandler.ScanFolderResults(RootFolder).ConfigureAwait(false); });

  }

  private async Task ScanFolder()
  {
    try
    {
      ComicCollection = new ObservableCollection<Comic>();
      var files = await _folderHandler.ScanFolder(RootFolder).ConfigureAwait(false);
      if (!files.Any()) return;

      var comics = files.Select(file => new Comic(file, _extractor)).ToList();
      // Show message box on the UI thread
      var result = await Dispatcher.UIThread.InvokeAsync(async () =>
      {
        var box = MessageBoxManager
                    .GetMessageBoxStandard("Comics Found", "Found " + comics.Count + " comics. Do you want to start scanning?",
                        ButtonEnum.YesNo);
        return await box.ShowAsync();
      });

      if (result == ButtonResult.No)
      {
        ScanningProgress = new FolderScanningProgress(false, 0, 0, string.Empty);
        ComicCollection = [];
        return;
      }

      var index = 0;
      ScanningProgress = new FolderScanningProgress(true, index, comics.Count, ProgressText(index, comics));
      foreach (var comic in comics)
      {
        await Task.Run(async () =>
        {
          comic.LoadMetaData();
          await Dispatcher.UIThread.InvokeAsync(async () =>
          {
            // Fetch metadata for the comic on a background thread
            ScanningProgress = new FolderScanningProgress(true, index, comics.Count, ProgressText(index, comics));
            if (comic.CoverImagePaths.HighResPath != null)
              CurrentImagePath = new Bitmap(comic.CoverImagePaths.HighResPath);

            ComicCollection.Add(comic);
          });

        }).ConfigureAwait(false);
        index++;
      }
    }
    catch (Exception)
    {
      // Handle exception
    }
  }

  public async Task SelectRootFolder()
  {

    RootFolder = await _folderHandler.SelectRootFolder();
    if (RootFolder != string.Empty)
      ScanningProgress = await _folderHandler.ScanFolderResults(RootFolder).ConfigureAwait(false);
  }

  private async Task UpdateProgressWhenRootFolderIsKnown()
  {
    // Determine number of files needing to be scanned.

  }

  private async Task VerifyApiKey()
  {
    await Dispatcher.UIThread.InvokeAsync(() =>
     {
       ApiKeyStatus = new(false, "Validating API Key...");
     });

    ApiKeyStatus = await _apiKeyHandler.VerifyApiKeyAsync(ComicVineApiKey);

  }

  private string ProgressText(int index, List<Comic> comics)
  {
    if (index < comics.Count() - 1 )
      return $" | Scanning: {comics[index].FileName}";
    else
      return $" | Unable To open: {comics.Count(s => s.UnableToOpen)} Needs Metadata : {comics.Count(s => s.NeedsMetaData)} ";
  }

}