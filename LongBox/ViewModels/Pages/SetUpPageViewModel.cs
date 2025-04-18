﻿using Avalonia.Media.Imaging;
using Avalonia.Threading;
using LongBox.Services;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System;
using MsBox.Avalonia;
using System.Linq;
using System.Reactive.Linq;
using LongBox.Domain;
using LongBox.Extensions;
using static LongBox.Extensions.ServiceCollectionExtensions;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace LongBox.ViewModels.Pages;

public class SetUpPageViewModel : PageViewModel
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

  public Comic _selectedComic;
  public Comic SelectedComic
  {
    get => _selectedComic;
    set => this.RaiseAndSetIfChanged(ref _selectedComic, value);
  }

  public ICommand SelectFolderCommand { get; }
  public ICommand OpenComicVineSiteCommand { get; }
  public ICommand VerifyApiKeyCommand { get; }
  public ICommand ScanFolderCommand { get; }
  public ICommand ShowDetailsCommand { get; }
  public ICommand UpdateCurrentImageCommand { get; }
  public ICommand OpenReaderCommand { get; }

  public Interaction<ReaderViewModel, ComicViewModel?> ShowReaderDialog { get; }

  public SetUpPageViewModel() : base(ApplicationPageNames.SetUp)
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
    ComicCollection = [comic, comic];

  }

  public SetUpPageViewModel(IComicMetadataExtractor comicMetadataExtractor, IApiKeyHandler apiKeyHandler, IFolderHandler folderHandler, PageFactory pageFactory) : base(ApplicationPageNames.SetUp)
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
    ShowDetailsCommand = ReactiveCommand.CreateFromTask<Comic>(ShowDetails);
    UpdateCurrentImageCommand = ReactiveCommand.CreateFromTask<Comic>(UpdateCurrentImage);
    ShowReaderDialog = new Interaction<ReaderViewModel, ComicViewModel?>();
    OpenReaderCommand = ReactiveCommand.CreateFromTask(async () =>
      {
        var toplevel = TopLevel.GetTopLevel(new ReaderWindow());
        var pickedComic = await toplevel?.StorageProvider?.OpenFilePickerAsync(new FilePickerOpenOptions())!;
        var comicPath = pickedComic.FirstOrDefault()?.TryGetLocalPath() ?? string.Empty;
        var comic = new Comic(new CBZFile(comicPath), comicMetadataExtractor);
        var readerViewModel = new ReaderViewModel(comicMetadataExtractor, comic);
        var comicViewModel = await ShowReaderDialog.Handle(readerViewModel);
      });


    if (RootFolder != _folderHandler.FolderNotSelected)
      Dispatcher.UIThread.InvokeAsync(async () => { ScanningProgress = await _folderHandler.ScanFolderResults(RootFolder).ConfigureAwait(false); });

    this.WhenAnyValue(X => X.SelectedComic).InvokeCommand(UpdateCurrentImageCommand);
  }

  private Task ShowDetails(Comic comic)
  {

    Console.WriteLine("Show Details");
    return Task.CompletedTask;

  }

  private Task UpdateCurrentImage(Comic comic)
  {
    if (comic == null) return Task.CompletedTask;
    CurrentImagePath = new Bitmap(comic.GetHighResImagePath);
    return Task.CompletedTask;
  }

  private async Task ScanFolder()
  {
    try
    {
      ComicCollection = new ObservableCollection<Comic>();
      var files = await _folderHandler.ScanFolder(RootFolder).ConfigureAwait(false);
      if (!files.Any()) return;

      var tasks = files.Select(file => Task.Run(() => new CBZFile(file)));
      var zipFiles = await Task.WhenAll(tasks);

      // Show message box on the UI thread
      var result = await Dispatcher.UIThread.InvokeAsync(async () =>
      {
        var box = MessageBoxManager
                    .GetMessageBoxStandard("Comics Found", "Found " + zipFiles.Length + " comics. Do you want to start scanning?",
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
      ScanningProgress = new FolderScanningProgress(true, index, zipFiles.Length, ProgressText(index, zipFiles));
      foreach (var zip in zipFiles)
      {
        await Task.Run(async () =>
        {
          var comic = new Comic(zip, _extractor);
          comic.LoadMetaData();
          await Dispatcher.UIThread.InvokeAsync(async () =>
          {
            // Fetch metadata for the comic on a background thread
            ScanningProgress = new FolderScanningProgress(true, index, zipFiles.Length, ProgressText(index, zipFiles));
            if (comic.GetHighResImagePath != null)
              await UpdateCurrentImage(comic);

            ComicCollection.Add(comic);
          });

        }).ConfigureAwait(false);
        index++;
      }
      await Dispatcher.UIThread.InvokeAsync(async () =>
      {
        // Fetch metadata for the comic on a background thread
        ScanningProgress = new FolderScanningProgress(true, index, zipFiles.Length, ProgressText(index, zipFiles));
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

  private string ProgressText(int index, CBZFile[] zipFiles)
  {
    if (index <= zipFiles.Count() - 1)
      return $" Scanning: {zipFiles[index].FileName}";
    else
      return $" Total scanned: {zipFiles.Count()} | Unable To open: {zipFiles.Count(s => s.UnableToOpen)} Needs Metadata : {zipFiles.Count(s => s.NeedsMetaData)} ";
  }



}