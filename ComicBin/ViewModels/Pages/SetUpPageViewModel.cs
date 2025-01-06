using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ComicBin.Core;
using ComicBin.Data;
using ComicBin.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace ComicBin.ViewModels.Pages
{
  public class SetUpPageViewModel : PageViewModelBase
  {
    private const string FOLDER_NOT_SELECTED = "Folder Not Selected";

    public ICommand SelectFolderCommand { get; }
    public ICommand SaveRootFolder { get; }

    private string _rootFolder = string.Empty;
    public string RootFolder
  {
      get => _rootFolder;
      set => this.RaiseAndSetIfChanged(ref _rootFolder, value);
    }

    public SetUpPageViewModel(ISettingsRepository settingsRepository)
    {
      RootFolder = ApplicationSettings.RootFolder ?? FOLDER_NOT_SELECTED;
      SelectFolderCommand = ReactiveCommand.CreateFromTask(async () =>
      {
        var toplevel = TopLevel.GetTopLevel(new MainWindow());
        IReadOnlyList<IStorageFolder> pickedFolder = await toplevel?.StorageProvider?.OpenFolderPickerAsync(new FolderPickerOpenOptions() { AllowMultiple = false});
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
}
