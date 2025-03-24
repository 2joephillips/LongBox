using Avalonia.Media.Imaging;
using ReactiveUI;
using System.IO;
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


    public ReactiveCommand<Unit, ComicViewModel?> CloseComicCommand { get; }
    public ComicViewModel? Comic { get; set; }

    public ReaderViewModel(IComicMetadataExtractor extractor, Comic comic)
    {
      _comic = comic;
      comic.LoadMetaData();
      var bitmap = CreateImage(_comic.CoverImagePaths.HighResPath);
      SourceImage = bitmap;
      CloseComicCommand = ReactiveCommand.Create(() => new ComicViewModel(comic));

    }

    public Bitmap CreateImage(string FilePath)
    {
      using var imageStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
      return Bitmap.DecodeToWidth(imageStream, 400);
    }
  }
}
