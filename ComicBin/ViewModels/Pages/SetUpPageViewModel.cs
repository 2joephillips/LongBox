using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ComicBin.Core.Models;
using ComicBin.Core.Services;
using ComicBin.Services;
using ComicBin.ViewModels;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System;
using MsBox.Avalonia;
using System.Linq;
using System.Reactive.Linq;

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

    ScanningProgress = new FolderScanningProgress(true, 2, 10, $" Total scanned: 100 | Unable To open: 1 | Needs Metadata : 3 ");
    var comic = new Comic()
    {
      CoverImagePaths = (
        ThumbnailPath: "C:\\Users\\Josep\\AppData\\Local\\ComicRack\\dbd3c688-af7b-481d-a116-630d2a396c1b_thumbnail.jpg",
        HighResPath: "C:\\Users\\Josep\\AppData\\Local\\ComicRack\\55bfabb8-8557-4c4e-b459-35f10bbcdd9a_highres.jpg"
        ),
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

  private async Task ShowDetails()
  {
    Console.WriteLine("Show Details");
  }

  private async Task ScanFolder()
  {
    try
    {
      ComicCollection = new ObservableCollection<Comic>();
      var files = await _folderHandler.ScanFolder(RootFolder).ConfigureAwait(false);
      if (!files.Any()) return;

      var zipFiles = files.Select(file => new CBZFile(file)).ToList();
      // Show message box on the UI thread
      var result = await Dispatcher.UIThread.InvokeAsync(async () =>
      {
        var box = MessageBoxManager
                    .GetMessageBoxStandard("Comics Found", "Found " + zipFiles.Count + " comics. Do you want to start scanning?",
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
      ScanningProgress = new FolderScanningProgress(true, index, zipFiles.Count, ProgressText(index, zipFiles));
      foreach (var zip in zipFiles)
      {
        await Task.Run(async () =>
        {
          var comic = new Comic(zip, _extractor);
          comic.LoadMetaData();
          await Dispatcher.UIThread.InvokeAsync(async () =>
          {
            // Fetch metadata for the comic on a background thread
            ScanningProgress = new FolderScanningProgress(true, index, zipFiles.Count, ProgressText(index, zipFiles));
            if (comic.CoverImagePaths.HighResPath != null)
              CurrentImagePath = new Bitmap(comic.CoverImagePaths.HighResPath);

            ComicCollection.Add(comic);
          });

        }).ConfigureAwait(false);
        index++;
      }
      await Dispatcher.UIThread.InvokeAsync(async () =>
      {
        // Fetch metadata for the comic on a background thread
        ScanningProgress = new FolderScanningProgress(true, index, zipFiles.Count, ProgressText(index, zipFiles));
      });
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


  private async Task VerifyApiKey()
  {
    await Dispatcher.UIThread.InvokeAsync(() =>
    {
      ApiKeyStatus = new(false, "Validating API Key...");
    });

    ApiKeyStatus = await _apiKeyHandler.VerifyApiKeyAsync(ComicVineApiKey);

  }

  private string ProgressText(int index, List<CBZFile> zipFiles)
  {
    if (index <= zipFiles.Count() - 1)
      return $" Scanning: {zipFiles[index].FileName}";
    else
      return $" Total scanned: {zipFiles.Count()} | Unable To open: {zipFiles.Count(s => s.UnableToOpen)} Needs Metadata : {zipFiles.Count(s => s.NeedsMetaData)} ";
  }

}