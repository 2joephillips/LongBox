﻿using Avalonia.Media.Imaging;
using ComicBin.Core.Services;
using ComicBin.Data.Entities;
using System;
using System.Linq;

namespace ComicBin.Core.Models
{
  public class Comic : ComicEntity
  {
    private readonly IComicMetadataExtractor _metadataExtractor;

    public int Id { get; private set; }
    public Guid Guid { get; private set; }
    public string FilePath { get;  set; }
    public string FileName { get;  set; }
    public bool UnableToOpen { get; private set; }
    public bool NeedsMetaData { get; private set; }
    public int? PageCount { get; private set; }
    public (string ThumbnailPath, string MediumPath, string HighResPath) CoverImagePaths { get; private set; }
    public MetaData MetaData { get; private set; }

    // Not persisted in DB
    public string GetHighResImagePath => CoverImagePaths.HighResPath;
    public string Publisher => MetaData?.Publisher ?? "Unknown";
    public string Title => string.IsNullOrEmpty(MetaData?.Title) ? "Unknown" : MetaData.Title;
    public Bitmap ThumbNailImage => string.IsNullOrEmpty(CoverImagePaths.ThumbnailPath) ? new Bitmap(ImageHandler.DefaultThumbNailImageLocation) : new Bitmap(CoverImagePaths.ThumbnailPath);

    public Comic() { }
    public Comic(string filePath, IComicMetadataExtractor metadataExtractor)
    {
      if (string.IsNullOrEmpty(filePath))
        throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
      FilePath = filePath;
      FileName = filePath.Split('\\').LastOrDefault() ?? string.Empty;
      _metadataExtractor = metadataExtractor ?? throw new ArgumentNullException(nameof(metadataExtractor));
    }

    public Comic(ComicEntity entity, IComicMetadataExtractor metadataExtractor)
    {
      Id = entity.Id;
      Guid = entity.Guid;
      FilePath = entity.FilePath;
      FileName = entity.FileName;
      UnableToOpen = entity.UnableToOpen;
      NeedsMetaData = entity.NeedsMetaData;
      PageCount = entity.PageCount;
      CoverImagePaths = entity.CoverImagePaths;
      MetaData = entity.MetaData;
      _metadataExtractor = metadataExtractor ?? throw new ArgumentNullException(nameof(metadataExtractor));
    }

    public void LoadMetaData()
    {
      try
      {
        (bool needsMetaData, MetaData metaData, int imageCount, (string ThumbnailPath, string MediumPath, string HighResPath) coverPaths) = _metadataExtractor.ExtractMetadata(FilePath);
        NeedsMetaData = needsMetaData;
        MetaData = metaData;
        CoverImagePaths = coverPaths;
        PageCount = imageCount;
        UnableToOpen = false;
      }
      catch (Exception)
      {
        UnableToOpen = true;
      }
    }
  }
}
