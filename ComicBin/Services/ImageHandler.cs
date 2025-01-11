using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.IO.Compression;
using System.Numerics;


namespace ComicBin.Core.Services;

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
    // Define sizes for each image
    var thumbnailSize = (Width: 150, Height: 150);
    var highResSize = (Width: 1024, Height: 1024);

    // Create and save the default images
    CreatePlaceholderImage(thumbnailSize.Width, thumbnailSize.Height, Color.Grey, "Thumbnail", ApplicationSettings.DefaultThumbNailImageLocation);
    CreatePlaceholderImage(highResSize.Width, highResSize.Height, Color.DarkGray, "High Res", ApplicationSettings.DefaultHighResImageLocation);
  }
  private static void CreatePlaceholderImage(int width, int height, Color backgroundColor, string text, string filePath)
  {
    FontFamily fontFamily;
    if (!SystemFonts.TryGet("Arial", out fontFamily))
      throw new Exception($"Couldn't find font {"Ariel"}");

    var font = fontFamily.CreateFont(16f, FontStyle.Regular);

    var options = new TextOptions(font)
    {
      Dpi = 72,
      KerningMode = KerningMode.Standard
    };

    var rect = TextMeasurer.MeasureSize(text, options);
    using var image = new Image<Rgba32>(width, height);
    image.Mutate(x => x.DrawText(
     text,
     font,
     new Color(Rgba32.ParseHex("#FFFFFFEE")),
     new PointF((image.Width - rect.Width) / 2,
        (image.Height - rect.Height) / 2)
     ));

    var encoder = new JpegEncoder { Quality = 100 };
    image.Save(filePath, encoder);
  }

  public static Image<Rgba32> GetImageFromZipArchiveEntry(ZipArchiveEntry entry)
  {
    using var entryStream = entry.Open();
    return Image.Load<Rgba32>(entryStream); // Load the image into memory
  }
}
