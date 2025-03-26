using Avalonia.Media.Imaging;
using ReactiveUI;
using System.IO;
using LongBox.Services;
using System.Reactive;
using LongBox.Domain;
using System.Windows.Input;
using System.Collections.Generic;
using System.IO.Compression;
using System.Reactive.Disposables;
using Avalonia.Media;
using System.Threading.Tasks;
using System.Linq;


namespace LongBox.ViewModels
{
  public class ReaderViewModel : ViewModelBase
  {

    private Bitmap _currentImage = ApplicationSettings.DefaultHighResImage;
    public Bitmap CurrentImage
    {
      get => _currentImage;
      set => this.RaiseAndSetIfChanged(ref _currentImage, value);
    }

    private Comic? _comic;
    private Dictionary<int, ZipArchiveEntry> _images = new();
    private int _activeImageIndex;
    private ZipArchive _zipArchive;

    public ReactiveCommand<Unit, ComicViewModel?> CloseComicCommand { get; }
    public ComicViewModel? Comic { get; set; }

    public ICommand PreviousPage { get; }
    public ICommand NextPage { get; }

    public ReaderViewModel()
    {
      if (Avalonia.Controls.Design.IsDesignMode)
      {
        CurrentImage = ApplicationSettings.DefaultHighResImage;
      }
    }

    public ReaderViewModel(IComicMetadataExtractor extractor, Comic comic)
    {
      SetUpComicAsync(comic).GetAwaiter().GetResult();
      var bitmap = CreateImage(_comic.CoverImagePaths.HighResPath);
      CurrentImage = bitmap;
      CloseComicCommand = ReactiveCommand.Create(() => new ComicViewModel(comic));

      PreviousPage = ReactiveCommand.CreateFromTask(async () =>
      {
        LoadPage(-1);
      });

      NextPage = ReactiveCommand.CreateFromTask(async () =>
      {
        LoadPage(1);
      });
    }

    public async Task SetUpComicAsync(Comic comic)
    {
      _comic = comic;
      comic.LoadMetaData();
      await ParseImagesAsync();
      var entry = _images[0];
      CurrentImage = ImageHandler.GetBitmapFromZipArchiveEntry(entry);
      //CalculatedBackgroundColor = ColorAnalyzer.GetTopColor(_comic.CoverImagePaths.ThumbnailPath);
      //CalculatedHoverBackgroundColor = new SolidColorBrush(Color.FromArgb(200, CalculatedBackgroundColor.Color.R, CalculatedBackgroundColor.Color.G, CalculatedBackgroundColor.Color.B));
    }
    private void LoadPage(int step)
    {
      _activeImageIndex = (_activeImageIndex + step + _images.Count) % _images.Count;
      var entry = _images[_activeImageIndex];

      CurrentImage = ImageHandler.GetBitmapFromZipArchiveEntry(entry);
    }

    private async Task ParseImagesAsync()
    {
      _images.Clear();

      // Open the ZipArchive for reading and store it in a class-level variable
      _zipArchive = ZipFile.OpenRead(_comic.CBZFile.FilePath);

      var supportedExtensions = new[] { ".jpg", ".png" };

      var filtered = _zipArchive.Entries
          .Where(entry => supportedExtensions.Any(e => entry.Name.ToLower().EndsWith(e)))
          .ToList();

      var indexedItems = filtered.Select((item, index) => new { Index = index, Item = item }).ToList();

      foreach (var item in indexedItems)
      {
        _images.Add(item.Index, item.Item);
      }
    }

    public Bitmap CreateImage(string FilePath)
    {
      using var imageStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
      return Bitmap.DecodeToWidth(imageStream, 400);
    }

    public void Dispose()
    {
      _zipArchive?.Dispose();  // Dispose the ZipArchive only when you're done with it
    }
  }
}
