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
using System.IO;
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

namespace ComicBin.ViewModels.Pages;

public static class FolderHandler
{
  public static Task<List<string>> ScanFolder(string folderName)
  {
    return Task.Run(() =>
    {
      var supportedExtensions = new List<string>() { ".jpg", ".png", ".pdf", ".cbz", ".cbr" };

      // Get all file paths in the root directory and its subdirectories
      var filePaths = Directory.GetFiles(folderName, "*.*", SearchOption.AllDirectories)
          .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLower()))
          .ToList();

      return filePaths;
    });
  }
}

public class SetUpPageViewModel : PageViewModelBase
{
  private const string FolderNotSelected = "Folder Not Selected";

  public ICommand SelectFolderCommand { get; }
  public ICommand ScanFolderCommand { get; }
  
  public ICommand OpenComicVineSiteCommand { get; }
  public ICommand VerifyApiKeyCommand { get; }
  

  private ObservableCollection<Comic> _comicCollection = new ObservableCollection<Comic>();
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

  private bool _isValidKey;
  public bool IsValidKey
  {
    get => _isValidKey;
    set => this.RaiseAndSetIfChanged(ref _isValidKey, value);
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

  public SetUpPageViewModel() { }

  public SetUpPageViewModel(IComicMetadataExtractor comicMetadataExtractor)
  {
    var extractor = comicMetadataExtractor;
    ComicCollection = new ObservableCollection<Comic>();
    RootFolder = ApplicationSettings.RootFolder ?? FolderNotSelected;
    SelectFolderCommand = ReactiveCommand.CreateFromTask(SelectRootFolder);
    ApiKeyStatus = "Not Validated";
    // SaveRootFolder = ReactiveCommand.CreateFromTask(async () =>
    // {
    //   await settingsRepository.InsertOrUpdateSetting(ApplicationSettingKey.RootFolder, RootFolder);
    //   ApplicationSettings.UpdateRootFolder(RootFolder);
    // });

    ScanFolderCommand = ReactiveCommand.CreateFromTask(async () =>
    {
      try
      {
        ScanningInProgress = true;
        ComicCollection = new ObservableCollection<Comic>();
        var files = await FolderHandler.ScanFolder(RootFolder).ConfigureAwait(false);
        if (!files.Any()) return;

        var comics = files.Select(file => new Comic(file, extractor)).ToList();

        // Show message box on the UI thread
        var result = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
          var box = MessageBoxManager
                      .GetMessageBoxStandard("Comics Found", "Found " + comics.Count + " comics. Do you want to start scanning?",
                          ButtonEnum.YesNo);

          return await box.ShowAsync();
        });

        if (result == ButtonResult.Yes)
        {
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
        else
        {
          ScanningInProgress = false;
          ComicCollection = new ObservableCollection<Comic>();
        }
      }
      catch (Exception)
      {
        // Handle exception
      }
    });

    var canVerifyApiKey = this.WhenAnyValue(x => x.ComicVineApiKey).Select(apiKey => !string.IsNullOrEmpty(apiKey)); 
    VerifyApiKeyCommand = ReactiveCommand.CreateFromTask(VerifyApiKey, canVerifyApiKey);
    
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

  public async Task SelectRootFolder()
  {
    var toplevel = TopLevel.GetTopLevel(new MainWindow());
    var pickedFolder = await toplevel?.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions() { AllowMultiple = false })!;
    var folderPath = pickedFolder.FirstOrDefault()?.TryGetLocalPath();
    var rootFolder = folderPath ?? string.Empty;
    if (rootFolder != string.Empty)
    {
      await UpdateProgressWhenRootFolderIsKnown().ConfigureAwait(false);
    }


    RootFolder = rootFolder;
  }

  private async Task UpdateProgressWhenRootFolderIsKnown()
  {
    // Determine number of files needing to be scanned.
    var files = await FolderHandler.ScanFolder(RootFolder).ConfigureAwait(false);
    ScanningProgress = $"Found {files.Count()} comics files.";
  }

  private async Task VerifyApiKey()
  {
   await Dispatcher.UIThread.InvokeAsync(() =>
    {
      ApiKeyStatus = "Validating API Key...";
      IsValidKey = false;
    });
   
    var apiUrl = $"https://comicvine.gamespot.com/api/issues/?api_key={ComicVineApiKey}&format=json";

    using HttpClient client = new HttpClient();
    client.DefaultRequestHeaders.Add("User-Agent", "ComicBin");
    try
    {
      HttpResponseMessage response = await client.GetAsync(apiUrl);

      if (response.IsSuccessStatusCode)
      {
        Console.Write("Works");
        ApiKeyStatus = "Validated";
        IsValidKey = true;
      }
      else
      {
        Console.Write("Invalid API Key");
        ApiKeyStatus = "Invalid";
        IsValidKey = false;
      }
    }
    catch (Exception ex)
    {
      Console.Write(ex.Message);
    }
  }
  
  private string ProgressText(int index, List<Comic> comics)
  {
    if (index < comics.Count())
      return $"{index}/{comics.Count()} Scanning: {comics[index - 1].FileName}";
    else
      return $"{index}/{comics.Count()} Unable To open: {comics.Count(s => s.UnableToOpen)} Needs Metadata : {comics.Count(s => s.NeedsMetaData)} ";
  }

  public override bool CanNavigateNext
  {
    get => true;
    protected set => throw new NotSupportedException();
  }
  public override bool CanNavigatePrevious
  {
    get => false;
    protected set => throw new NotSupportedException();
  }
}
