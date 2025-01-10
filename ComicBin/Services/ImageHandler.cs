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
  public static string DefaultThumbNailImageLocation { get; internal set; }
  public static string DefaultMediumResImageLocation { get; internal set; }
  public static string DefaultHighResImageLocation { get; internal set; }

  public static Avalonia.Media.Imaging.Bitmap DefaultHighResImage => new Avalonia.Media.Imaging.Bitmap(DefaultHighResImageLocation);

  public static Image<Rgba32> ResizeImage(Image<Rgba32> image, int maxWidth = 300, int maxHeight = 300)
  {
    if (image == null)
      throw new ArgumentNullException(nameof(image));

    var options = new ResizeOptions
    {
      Mode = ResizeMode.Max,
      Size = new SixLabors.ImageSharp.Size(maxWidth, maxHeight)
    };

    image.Mutate(x => x.Resize(options));
    return image;
  }


  public static void SaveResizedImage(Image<Rgba32> image, string filePath, int maxWidth, int maxHeight, int quality)
  {
    // Calculate new dimensions while maintaining aspect ratio
    var resizedImage = ResizeImage(image, maxWidth, maxHeight);

    // Save the resized image with the specified quality
    var encoder = new JpegEncoder { Quality = quality };
    resizedImage.Save(filePath, encoder);
  }

  public static void CreateDefaultImages(string appDataPath)
  {
    // Define sizes for each image
    var thumbnailSize = (Width: 150, Height: 150);
    var mediumSize = (Width: 300, Height: 300);
    var highResSize = (Width: 1024, Height: 1024);

    // Define file names
    DefaultThumbNailImageLocation = Path.Combine(appDataPath, "default_thumbnail.jpg");
    DefaultMediumResImageLocation = Path.Combine(appDataPath, "default_medium.jpg");
    DefaultHighResImageLocation = Path.Combine(appDataPath, "default_highres.jpg");

    // Create and save the default images
    CreatePlaceholderImage(thumbnailSize.Width, thumbnailSize.Height, SixLabors.ImageSharp.Color.Grey, "Thumbnail", DefaultThumbNailImageLocation);
    CreatePlaceholderImage(mediumSize.Width, mediumSize.Height, SixLabors.ImageSharp.Color.LightGray, "Medium", DefaultMediumResImageLocation);
    CreatePlaceholderImage(highResSize.Width, highResSize.Height, SixLabors.ImageSharp.Color.DarkGray, "High Res", DefaultHighResImageLocation);
  }
  private static void CreatePlaceholderImage(int width, int height, SixLabors.ImageSharp.Color backgroundColor, string text, string filePath)
  {
    using var image = new Image<Rgba32>(width, height);
    image.Mutate(ctx =>
    {
      ctx.BackgroundColor(backgroundColor);
      var font = SystemFonts.CreateFont("Arial", 16, SixLabors.Fonts.FontStyle.Bold);
      Vector2 center = new Vector2(image.Width / 2, 10); //center horizontally, 10px down 
      ctx.DrawText(text, font, SixLabors.ImageSharp.Color.Black, new PointF(width / 2, height / 2));

   
    });

    var encoder = new JpegEncoder { Quality = 90 };
    image.Save(filePath, encoder);
  }

  public static Image<Rgba32> GetImageFromZipArchiveEntry(ZipArchiveEntry entry)
  {
    using var entryStream = entry.Open();
    return Image.Load<Rgba32>(entryStream); // Load the image into memory
  }
}
