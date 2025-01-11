using System;
using System.IO;
using System.IO.Compression;
using ComicBin.Core.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ComicBin.Services;


public interface ISystemStorage
{
  (string ThumbnailPath, string HighResPath) CreateComicCoverImages(ZipArchiveEntry? coverImage);
}

public class SystemStorage : ISystemStorage
{
  public SystemStorage()
  {
  }

  public (string ThumbnailPath, string HighResPath) CreateComicCoverImages(ZipArchiveEntry? coverImage)
  {
    if (coverImage == null)
      return (ApplicationSettings.DefaultThumbNailImageLocation,ApplicationSettings.DefaultHighResImageLocation);

    // Generate a unique file name
    var comicImagePath = Guid.NewGuid().ToString();
    var highResFileName = Guid.NewGuid().ToString() + "_highres.jpg";

    var thumbnailFilePath = Path.Combine(ApplicationSettings.AppDataPath, comicImagePath + "_thumbnail.jpg");
    var highResFilePath = Path.Combine(ApplicationSettings.AppDataPath, highResFileName);
    using var entryStream = coverImage.Open();
    using var image = Image.Load<Rgba32>(entryStream); // Load the image into memory
    ImageHandler.CreatePlaceholderImages(image, highResFilePath, thumbnailFilePath);

    return (thumbnailFilePath, highResFilePath);
  }





}
