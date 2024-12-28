using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Compression;

namespace ComicBin.Core.Services;

public static class ImageHandler
{
  public static string DefaultThumbNailImageLocation { get; internal set; }
  public static string DefaultMediumResImageLocation { get; internal set; }
  public static string DefaultHighResImageLocation { get; internal set; }

  public static ImageCodecInfo GetEncoder(ImageFormat format) => ImageCodecInfo.GetImageDecoders().FirstOrDefault(codec => codec.FormatID == format.Guid);

  public static Image ResizeImage(Image image, int maxWidth = 300, int maxHeight = 300)
  {
    if (image == null)
      throw new ArgumentNullException(nameof(image));

    int newWidth = image.Width;
    int newHeight = image.Height;

    // Maintain aspect ratio
    if (image.Width > maxWidth || image.Height > maxHeight)
    {
      var widthRatio = (double)maxWidth / image.Width;
      var heightRatio = (double)maxHeight / image.Height;
      var scale = Math.Min(widthRatio, heightRatio);

      newWidth = (int)(image.Width * scale);
      newHeight = (int)(image.Height * scale);
    }

    var resizedImage = new Bitmap(newWidth, newHeight);
    using (var graphics = Graphics.FromImage(resizedImage))
    {
      graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
      graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
      graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

      graphics.DrawImage(image, 0, 0, newWidth, newHeight);
    }

    return resizedImage;
  }

  public static void SaveResizedImage(Image image, string filePath, int maxWidth, int maxHeight, long quality)
  {
    // Calculate new dimensions while maintaining aspect ratio
    var resizedImage = ResizeImage(image, maxWidth, maxHeight);
    // Save the resized image with the specified quality
    var jpegEncoder = GetEncoder(ImageFormat.Jpeg);
    var encoderParameters = new EncoderParameters(1)
    {
      Param = { [0] = new EncoderParameter(Encoder.Quality, quality) }
    };

    resizedImage.Save(filePath, jpegEncoder, encoderParameters);
  }

  public static void CreateDefaultImages(string appDataPath)
  {
    // Define sizes for each image
    var thumbnailSize = (Width: 150, Height: 150);
    var mediumSize = (Width: 300, Height: 300);
    var highResSize = (Width: 1024, Height: 1024);

    // Define file names
    var thumbnailPath = Path.Combine(appDataPath, "default_thumbnail.jpg");
    var mediumPath = Path.Combine(appDataPath, "default_medium.jpg");
    var highResPath = Path.Combine(appDataPath, "default_highres.jpg");

    // Create and save the default images
    CreatePlaceholderImage(thumbnailSize.Width, thumbnailSize.Height, Color.Gray, "Thumbnail", thumbnailPath);
    CreatePlaceholderImage(mediumSize.Width, mediumSize.Height, Color.LightGray, "Medium", mediumPath);
    CreatePlaceholderImage(highResSize.Width, highResSize.Height, Color.DarkGray, "High Res", highResPath);

  }


  private static void CreatePlaceholderImage(int width, int height, Color backgroundColor, string text, string filePath)
  {
    using var bitmap = new Bitmap(width, height);
    using var graphics = Graphics.FromImage(bitmap);

    // Fill the background
    using var backgroundBrush = new SolidBrush(backgroundColor);
    graphics.FillRectangle(backgroundBrush, 0, 0, width, height);

    // Add text to the center
    using var font = new Font("Arial", 16, FontStyle.Bold);
    using var textBrush = new SolidBrush(Color.White);
    var textSize = graphics.MeasureString(text, font);
    var textPosition = new PointF((width - textSize.Width) / 2, (height - textSize.Height) / 2);
    graphics.DrawString(text, font, textBrush, textPosition);

    // Save the image as a JPEG
    var jpegEncoder = GetEncoder(ImageFormat.Jpeg);
    var encoderParameters = new EncoderParameters(1)
    {
      Param = { [0] = new EncoderParameter(Encoder.Quality, 90L) } // High quality for default placeholders
    };
    bitmap.Save(filePath, jpegEncoder, encoderParameters);
  }


  public static Image GetImageFromZipArchiveEntry(ZipArchiveEntry entry)
  {
    using var entryStream = entry.Open();
    return Image.FromStream(entryStream); // Load the image into memory
  }
}
