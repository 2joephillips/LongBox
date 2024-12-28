using System.Drawing;
using System.IO;
using System.IO.Compression;

namespace ComicBin.Core.Services;


public interface ISystemStorage
{
  (string ThumbnailPath, string MediumPath, string HighResPath) CreateComicCoverImages(ZipArchiveEntry? coverImage);
}

public class SystemStorage : ISystemStorage
{
  public SystemStorage()
  {
  }

  public (string ThumbnailPath, string MediumPath, string HighResPath) CreateComicCoverImages(ZipArchiveEntry coverImage)
  {
    if (coverImage == null)
      return (ImageHandler.DefaultThumbNailImageLocation, ImageHandler.DefaultMediumResImageLocation, ImageHandler.DefaultHighResImageLocation);

    // Generate a unique file name
    var thumbnailFileName = Guid.NewGuid().ToString() + "_thumbnail.jpg";
    var mediumFileName = Guid.NewGuid().ToString() + "_medium.jpg";
    var highResFileName = Guid.NewGuid().ToString() + "_highres.jpg";

    var thumbnailFilePath = Path.Combine(ApplicationSettings.AppDataPath, thumbnailFileName);
    var mediumFilePath = Path.Combine(ApplicationSettings.AppDataPath, mediumFileName);
    var highResFilePath = Path.Combine(ApplicationSettings.AppDataPath, highResFileName);

    using var entryStream = coverImage.Open();
    using var image = Image.FromStream(entryStream); // Load the image into memory

    // Generate and save the three sizes
    ImageHandler.SaveResizedImage(image, thumbnailFilePath, 150, 150, 75L); // Thumbnail
    ImageHandler.SaveResizedImage(image, mediumFilePath, 300, 300, 85L);    // Medium
    ImageHandler.SaveResizedImage(image, highResFilePath, 1024, 1024, 90L); // High resolution

    return (thumbnailFilePath, mediumFilePath, highResFilePath);
  }





}
