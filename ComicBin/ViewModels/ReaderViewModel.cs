using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using ComicBin.Domain;
using ReactiveUI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace ComicBin.ViewModels
{
  public class ReaderViewModel : ViewModelBase
  {

    private Avalonia.Media.Imaging.Bitmap _sourceImage;
    public Avalonia.Media.Imaging.Bitmap SourceImage
    {
      get => _sourceImage;
      set => this.RaiseAndSetIfChanged(ref _sourceImage, value);
    }


    public ICommand SelectFolderCommand { get; }
    public ReaderViewModel()
    {
      SelectFolderCommand = ReactiveCommand.CreateFromTask(async() =>
      {
        var toplevel = TopLevel.GetTopLevel(new ReaderWindow());
        IReadOnlyList<IStorageFile> pickedComic = await toplevel?.StorageProvider?.OpenFilePickerAsync(new FilePickerOpenOptions());
        var comicPath = pickedComic.FirstOrDefault()?.TryGetLocalPath();
        var comic = new Comic(comicPath);
        var bitmap = CreateImage(comic.CoverImagePaths.HighResPath);
        SourceImage = bitmap;
      });
    }


    public Bitmap CreateImage(string FilePath)
    {
      using var imageStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
      return Bitmap.DecodeToWidth(imageStream, 400);
    }
  }
}
