using ComicBin.Core.Models;
using System;
using ComicBin.Data.Entities;
using ComicBin.Services;
using ComicBin.Data;

namespace ComicBin.Core.Services;

public interface IComicMetadataExtractor
{
  (MetaData? metaData, (string ThumbnailPath, string HighResPath) coverImagePath) ExtractMetadata(CBZFile cbzFile);
}

public class ComicMetadataExtractor : IComicMetadataExtractor
{
  private readonly ISystemStorage _storage;
  private readonly ComicBinContext _context;
  string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

  public ComicMetadataExtractor(ISystemStorage storage, ComicBinContext context)
  {
    _storage = storage ?? throw new ArgumentNullException(nameof(storage));
    _context = context ?? throw new ArgumentNullException(nameof(context));
  }

  public (MetaData? metaData, (string ThumbnailPath, string HighResPath) coverImagePath) ExtractMetadata(CBZFile cbzFile)
  {
    MetaData? metaData = null;

    if (!cbzFile.NeedsMetaData)
    {      
      var doc = cbzFile.GetComicInfoXml();
      metaData = new MetaData(doc);
    }

    var coverImagePaths = _storage.CreateComicCoverImages(cbzFile.GetFirstImage());
    return (metaData, coverImagePaths);
  }
}
