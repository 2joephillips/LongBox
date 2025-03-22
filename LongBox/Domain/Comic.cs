using Avalonia.Media.Imaging;
using LongBox.Data.Entities;
using System;
using LongBox.Services;

namespace LongBox.Domain;

public class Comic : ComicEntity
{
  private readonly IComicMetadataExtractor _metadataExtractor;

  public CBZFile CBZFile { get; set; }
  // Not persisted in DB
  public string GetHighResImagePath => CoverImagePaths.HighResPath;
  public string Publisher => MetaData?.Publisher ?? "Unknown";
  public string Title => string.IsNullOrEmpty(MetaData?.Title) ? "Unknown" : MetaData.Title;
  public Bitmap ThumbNailImage => string.IsNullOrEmpty(CoverImagePaths.ThumbnailPath) ? ApplicationSettings.DefaultThumbNailImage  : new Bitmap(CoverImagePaths.ThumbnailPath);

  public Comic()
  {
    _metadataExtractor = null!;
  }

  public Comic(CBZFile zipFile, IComicMetadataExtractor metadataExtractor)
  {
    CBZFile = zipFile;
    _metadataExtractor = metadataExtractor ?? throw new ArgumentNullException(nameof(metadataExtractor));
  }

  public Comic(ComicEntity entity, IComicMetadataExtractor metadataExtractor)
  {
    Id = entity.Id;
    Guid = entity.Guid;
    CBZFile = new CBZFile(entity.CBZFile);
    CoverImagePaths = entity.CoverImagePaths;
    MetaData = entity.MetaData;
    _metadataExtractor = metadataExtractor ?? throw new ArgumentNullException(nameof(metadataExtractor));
  }

  public void LoadMetaData()
  {
    try
    {
      (MetaData? metaData, (string ThumbnailPath, string HighResPath) coverPaths) = _metadataExtractor.ExtractMetadata(CBZFile);
      if(metaData != null)
        MetaData = metaData;
      CoverImagePaths = coverPaths;
    }
    catch (Exception)
    {
      CBZFile.UnableToOpen = true;
    }
  }
}
