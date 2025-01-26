using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ComicBin.Core.Models;
using ComicBin.Core.Services;
using ComicBin.Views;
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
using System.Diagnostics;
using System.Net.Http;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using ComicBin.Services;

namespace ComicBin.ViewModels.Pages;

public class SetUpPageViewModel : ViewModelBase
{
  private const string FolderNotSelected = "Folder Not Selected";
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

  private string _apiKeyStatus;
  public string ApiKeyStatus
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

  private bool _scanningInProgress;
  public bool ScanningInProgress
  {
    get => _scanningInProgress;
    set => this.RaiseAndSetIfChanged(ref _scanningInProgress, value);
  }

  private string _scanningProgress = string.Empty;
  public string ScanningProgress
  {
    get => _scanningProgress;
    set => this.RaiseAndSetIfChanged(ref _scanningProgress, value);
  }

  public ICommand SelectFolderCommand { get; }
  public ICommand OpenComicVineSiteCommand { get; }
  public ICommand VerifyApiKeyCommand { get; }
  public ICommand ScanFolderCommand { get; }

  public SetUpPageViewModel()
  {
    RootFolder = ApplicationSettings.RootFolder ?? FolderNotSelected;
    ApiKeyStatus = "Not Validated";
    ComicVineApiKey = "cf1839e4-dcf4-4558-b355-0748f2b71e68";
    ScanningProgress = " 0/100 Scanned";
  }

  public SetUpPageViewModel(IComicMetadataExtractor comicMetadataExtractor, IApiKeyHandler apiKeyHandler, IFolderHandler folderHandler)
  {
    _extractor = comicMetadataExtractor;
    _apiKeyHandler = apiKeyHandler;
    _folderHandler = folderHandler;
    ComicCollection = [];
    RootFolder = ApplicationSettings.RootFolder ?? FolderNotSelected;
    ApiKeyStatus = "Not Validated";

    SelectFolderCommand = ReactiveCommand.CreateFromTask(SelectRootFolder);

    var canVerifyApiKey = this.WhenAnyValue(x => x.ComicVineApiKey).Select(apiKey => !string.IsNullOrEmpty(apiKey));
    VerifyApiKeyCommand = ReactiveCommand.CreateFromTask(VerifyApiKey, canVerifyApiKey);

    ScanFolderCommand = ReactiveCommand.CreateFromTask(ScanFolder);
    OpenComicVineSiteCommand = ReactiveCommand.Create(() =>
    {
      var url = "https://comicvine.gamespot.com/api/";
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        Process.Start(new ProcessStartInfo("cmd", $"/c start {url}"));
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
      {
        Process.Start("xdg-open", url);
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
      {
        Process.Start("open", url);
      }
    });

    if (!string.IsNullOrEmpty(RootFolder))
    {
      Dispatcher.UIThread.InvokeAsync(UpdateProgressWhenRootFolderIsKnown);
    }
  }

  private async Task ScanFolder()
  {
    try
    {
      ScanningInProgress = true;
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
        ScanningInProgress = false;
        ComicCollection = [];
        return;
      }

      var index = 1;
      ScanningProgress = ProgressText(index, comics);
      foreach (var comic in comics)
      {
        ScanningProgress = ProgressText(index++, comics);
        // Fetch metadata for the comic on a background thread
        await Task.Run(() => comic.LoadMetaData()).ConfigureAwait(false);
        if (comic.CoverImagePaths.HighResPath != null)
          CurrentImagePath = new Bitmap(comic.CoverImagePaths.HighResPath);

        ComicCollection.Add(comic);

      }

    }
    catch (Exception)
    {
      // Handle exception
    }
  }

  public async Task SelectRootFolder()
  {
    var toplevel = TopLevel.GetTopLevel(new MainWindow());
    var pickedFolder = await toplevel?.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions() { AllowMultiple = false })!;
    var folderPath = pickedFolder.FirstOrDefault()?.TryGetLocalPath();
    var rootFolder = folderPath ?? string.Empty;
    RootFolder = rootFolder;
    if (rootFolder != string.Empty)
      await UpdateProgressWhenRootFolderIsKnown().ConfigureAwait(false);
  }

  private async Task UpdateProgressWhenRootFolderIsKnown()
  {
    // Determine number of files needing to be scanned.
    var files = await _folderHandler.ScanFolder(RootFolder).ConfigureAwait(false);
    ScanningProgress = $"Found {files.Count()} comics files.";
  }

  private async Task VerifyApiKey()
  {
    await Dispatcher.UIThread.InvokeAsync(() =>
     {
       ApiKeyStatus = "Validating API Key...";
     });

    var validated = await _apiKeyHandler.VerifyApiKeyAsync(ComicVineApiKey);
    if (validated)
    {
      Console.Write("Works");
      ApiKeyStatus = "Validated";
    }
    else
    {
      Console.Write("Invalid API Key");
      ApiKeyStatus = "Invalid";
    }
  }

  private string ProgressText(int index, List<Comic> comics)
  {
    if (index < comics.Count())
      return $"{index}/{comics.Count()} Scanning: {comics[index - 1].FileName}";
    else
      return $"{index}/{comics.Count()} Unable To open: {comics.Count(s => s.UnableToOpen)} Needs Metadata : {comics.Count(s => s.NeedsMetaData)} ";
  }

}