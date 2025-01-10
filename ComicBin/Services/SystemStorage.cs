using System;
using System.IO;
using System.IO.Compression;
using ComicBin.Core.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ComicBin.Services;


public interface ISystemStorage
{
  (string ThumbnailPath, string MediumPath, string HighResPath) CreateComicCoverImages(ZipArchiveEntry? coverImage);
}

public class SystemStorage : ISystemStorage
{
  public SystemStorage()
  {
  }

  public (string ThumbnailPath, string MediumPath, string HighResPath) CreateComicCoverImages(ZipArchiveEntry? coverImage)
  {
    if (coverImage == null)
      return (ApplicationSettings.DefaultThumbNailImageLocation, ApplicationSettings.DefaultMediumResImageLocation, ApplicationSettings.DefaultHighResImageLocation);

    // Generate a unique file name
    var thumbnailFileName = Guid.NewGuid().ToString() + "_thumbnail.jpg";
    var mediumFileName = Guid.NewGuid().ToString() + "_medium.jpg";
    var highResFileName = Guid.NewGuid().ToString() + "_highres.jpg";

    var thumbnailFilePath = Path.Combine(ApplicationSettings.AppDataPath, thumbnailFileName);
    var mediumFilePath = Path.Combine(ApplicationSettings.AppDataPath, mediumFileName);
    var highResFilePath = Path.Combine(ApplicationSettings.AppDataPath, highResFileName);

    using var entryStream = coverImage.Open();
    using var image = Image.Load<Rgba32>(entryStream); // Load the image into memory

    // Generate and save the three sizes
    ImageHandler.SaveResizedImage(image, thumbnailFilePath, 150, 150, 75); // Thumbnail
    ImageHandler.SaveResizedImage(image, mediumFilePath, 300, 300, 85);    // Medium
    ImageHandler.SaveResizedImage(image, highResFilePath, 1024, 1024, 90); // High resolution

    return (thumbnailFilePath, mediumFilePath, highResFilePath);
  }





}
