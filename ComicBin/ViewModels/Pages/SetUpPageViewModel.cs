using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ComicBin.Core;
using ComicBin.Core.Models;
using ComicBin.Core.Services;
using ComicBin.Data;
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
using Avalonia;
using Avalonia.Threading;
using Avalonia.Media.Imaging;
using System.Collections.ObjectModel;

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
  private const string FOLDER_NOT_SELECTED = "Folder Not Selected";

  public ObservableCollection<Comic> ComicCollection { get; }

  public ICommand SelectFolderCommand { get; }
  public ICommand ScanFolderCommand { get; }
  public ICommand SaveRootFolder { get; }


  private readonly IComicMetadataExtractor _extractor;

  private string _rootFolder = string.Empty;
  public string RootFolder
  {
    get => _rootFolder;
    set => this.RaiseAndSetIfChanged(ref _rootFolder, value);
  }

  private Bitmap _currentImagePath = ImageHandler.DefaultHighResImageLocation == null ? null : new Bitmap(ImageHandler.DefaultHighResImageLocation);
  public Bitmap CurrentImagePath
  {
    get => _currentImagePath;
    set => this.RaiseAndSetIfChanged(ref _currentImagePath, value);
  }

  private string _scanningProgress = string.Empty;
  public string ScanningProgress
  {
    get => _scanningProgress;
    set => this.RaiseAndSetIfChanged(ref _scanningProgress, value);
  }

  public SetUpPageViewModel(ISettingsRepository settingsRepository, IComicMetadataExtractor comicMetadataExtractor)
  {
    _extractor = comicMetadataExtractor;
    ComicCollection = new ObservableCollection<Comic>();
    RootFolder = ApplicationSettings.RootFolder ?? FOLDER_NOT_SELECTED;
    SelectFolderCommand = ReactiveCommand.CreateFromTask(async () =>
    {
      var toplevel = TopLevel.GetTopLevel(new MainWindow());
      IReadOnlyList<IStorageFolder> pickedFolder = await toplevel?.StorageProvider?.OpenFolderPickerAsync(new FolderPickerOpenOptions() { AllowMultiple = false });
      var folderPath = pickedFolder.FirstOrDefault()?.TryGetLocalPath();
      RootFolder = folderPath;
      //var comicPath = pickedComic.FirstOrDefault()?.TryGetLocalPath();
      //var comic = new Comic(comicPath, new ComicMetadataExtractor(new SystemStorage()));
      //var bitmap = CreateImage(comic.CoverImagePaths.HighResPath);
      //SourceImage = bitmap;
    });

    SaveRootFolder = ReactiveCommand.CreateFromTask(async () =>
    {
      settingsRepository.InsertOrUpdateSetting(ApplicationSettingKey.RootFolder, RootFolder);
      ApplicationSettings.UpdateRootFolder(RootFolder);
    });

    ScanFolderCommand = ReactiveCommand.CreateFromTask(async () =>
    {
      try
      {
        var files = await FolderHandler.ScanFolder(RootFolder).ConfigureAwait(false);
        if (files == null || !files.Any()) return;

        var comics = files.Select(file => new Comic(file, _extractor)).ToList();

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
      }
      catch (Exception ex)
      {
        // Handle exception
      }
    });
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
