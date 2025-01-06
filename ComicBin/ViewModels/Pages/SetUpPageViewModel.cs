using Avalonia.Controls;
using Avalonia.Platform.Storage;
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
    public ICommand SelectFolderCommand { get; }


    private string _rootFolder = "No Root Folder Selected";
    public string RootFolder
  {
      get => _rootFolder;
      set => this.RaiseAndSetIfChanged(ref _rootFolder, value);
    }

    public SetUpPageViewModel()
    {
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
