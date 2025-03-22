using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using ReactiveUI;
using System.IO;
using System.Linq;
using System.Windows.Input;
using LongBox.Services;
using System.Reactive;
using LongBox.Domain;

namespace LongBox.ViewModels
{
  public class ReaderViewModel : ViewModelBase
  {

    private Bitmap _sourceImage = ApplicationSettings.DefaultHighResImage;
    public Bitmap SourceImage
    {
      get => _sourceImage;
      set => this.RaiseAndSetIfChanged(ref _sourceImage, value);
    }

    private Comic? _comic;

    public ICommand SelectFolderCommand { get; }
    public ReactiveCommand<Unit, ComicViewModel?> CloseComicCommand { get; }
    public ComicViewModel? Comic { get; set; }

    public ReaderViewModel() { }

    public ReaderViewModel(IComicMetadataExtractor extractor, Comic? comic = null)
    {
      if (comic != null)
        _comic = comic;

      SelectFolderCommand = ReactiveCommand.CreateFromTask(async () =>
      {
        var toplevel = TopLevel.GetTopLevel(new ReaderWindow());
        var pickedComic = await toplevel?.StorageProvider?.OpenFilePickerAsync(new FilePickerOpenOptions())!;
        var comicPath = pickedComic.FirstOrDefault()?.TryGetLocalPath() ?? string.Empty;
        _comic = new Comic(new CBZFile(comicPath), extractor);
        var bitmap = CreateImage(comic.CoverImagePaths.HighResPath);
        SourceImage = bitmap;
      });

      CloseComicCommand = ReactiveCommand.Create(() => new ComicViewModel(comic));
    }


    public Bitmap CreateImage(string FilePath)
    {
      using var imageStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
      return Bitmap.DecodeToWidth(imageStream, 400);
    }
  }
}
