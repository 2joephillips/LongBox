using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.IO.Compression;


namespace LongBox.Services;

public static class ImageHandler
{

  public static void CreateThumbnail(Image image, string thumbnailPath, int size, int quality)
  {

    image.Mutate(x => x.Resize(new ResizeOptions
    {
      Size = new Size(size, size), // Create a square thumbnail
      Mode = ResizeMode.Max, // Crop the image to match the size
      Sampler = KnownResamplers.Lanczos3 // Use high-quality resampling
    }));

    image.Save(thumbnailPath, new JpegEncoder
    {
      Quality = quality // Compression quality
    });
  }

  public static void CreatePlaceholderImages(Image image, string highResPath, string thumbnailPath)
  {
    image.SaveAsJpegAsync(highResPath, new JpegEncoder { Quality = 100 });
    CreateThumbnail(image, thumbnailPath, 150, 90);
  }

  public static void SaveImages(Image<Rgba32> image, string filePath, int width, int height, int quality)
  {
    if (image == null)
      throw new ArgumentNullException(nameof(image));

    var resizeOptions = new ResizeOptions
    {
      Mode = ResizeMode.BoxPad, // Prevent distortion while resizing
      Size = new Size(width, height),
      Sampler = KnownResamplers.Lanczos3 // High-quality resampling algorithm
    };

    image.Mutate(x => x.Resize(resizeOptions));

    image.Save(filePath, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
    {
      Quality = quality // Compression level, 100 is maximum quality
    });
  }

  public static void CreateDefaultImages(string appDataPath)
  {
    var highRes = new Bitmap(AssetLoader.Open(new Uri("avares://LongBox/Assets/default_highres.jpg")));
    highRes.Save(ApplicationSettings.DefaultHighResImageLocation);
    highRes.Dispose();

    var thumbnail = new Bitmap(AssetLoader.Open(new Uri("avares://LongBox/Assets/default_thumbnail.jpg")));
    thumbnail.Save(ApplicationSettings.DefaultThumbNailImageLocation);
    thumbnail.Dispose();

  }
  public static Bitmap GetBitmapFromZipArchiveEntry(ZipArchiveEntry entry)
  {
    using var memoryStream = new MemoryStream();
    using var entryStream = entry.Open();
    var image = Image.Load<Rgba32>(entryStream); // Load the image into memory
    image.SaveAsBmp(memoryStream);
    memoryStream.Seek(0, SeekOrigin.Begin);
    return new Bitmap(memoryStream);
  }
}

