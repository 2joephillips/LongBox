using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using ComicBin.Core.Models;
using ComicBin.Core.Services;
using ReactiveUI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using ComicBin.Services;

namespace ComicBin.ViewModels
{
  public class ReaderViewModel : ViewModelBase
  {

    private Bitmap _sourceImage = ApplicationSettings.DefaultHighResImage;
    public Bitmap SourceImage
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
        var pickedComic = await toplevel?.StorageProvider?.OpenFilePickerAsync(new FilePickerOpenOptions())!;
        var comicPath = pickedComic.FirstOrDefault()?.TryGetLocalPath() ?? string.Empty;
        var comic = new Comic(comicPath, new ComicMetadataExtractor(new SystemStorage()));
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
