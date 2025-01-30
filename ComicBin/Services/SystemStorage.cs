using System;
using System.IO;
using ComicBin.Core.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ComicBin.Services;


public interface ISystemStorage
{
  (string ThumbnailPath, string HighResPath) CreateComicCoverImages(Image<Rgba32>? coverImage);
}

public class SystemStorage : ISystemStorage
{
  public SystemStorage()
  {
  }

  public (string ThumbnailPath, string HighResPath) CreateComicCoverImages(Image<Rgba32>? coverImage)
  {
    if (coverImage == null)
      return (ApplicationSettings.DefaultThumbNailImageLocation,ApplicationSettings.DefaultHighResImageLocation);

    // Generate a unique file name
    var comicImagePath = Guid.NewGuid().ToString();
    var highResFileName = Guid.NewGuid().ToString() + "_highres.jpg";

    var thumbnailFilePath = Path.Combine(ApplicationSettings.AppDataPath, comicImagePath + "_thumbnail.jpg");
    var highResFilePath = Path.Combine(ApplicationSettings.AppDataPath, highResFileName);
    
    ImageHandler.CreatePlaceholderImages(coverImage, highResFilePath, thumbnailFilePath);

    return (thumbnailFilePath, highResFilePath);
  }





}
