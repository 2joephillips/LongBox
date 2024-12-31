using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.IO.Compression;
using System.IO;
using System.Xml.Linq;
using ComicBin.Domain;

namespace ComicBin.Domain
{
  public class Comic
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public Guid Guid { get; set; }

    [Required]
    public string FilePath { get; set; }

    [Required]
    public string FileName { get; set; }

    public bool UnableToOpen { get; set; }

    public bool NeedsMetaData { get; set; }

    [NotMapped]
    public MetaData MetaData { get; set; }

    [NotMapped]
    public (string ThumbnailPath, string MediumPath, string HighResPath) CoverImagePaths { get; set; }

    [Required]
    public int? PageCount { get; set; }

    [NotMapped]
    public string GetHighResImagePath => CoverImagePaths.HighResPath;

    public Comic(string filePath)
    {
      if (string.IsNullOrEmpty(filePath))
        throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
      FilePath = filePath.ToString();
      FileName = filePath.ToString().Split('\\').LastOrDefault() ?? string.Empty;
      LoadMetaData();
    }

    public void LoadMetaData()
    {
      try
      {
        var extractor = new ComicMetadataExtractor(new SystemStorage());
        (bool needsMetaData, MetaData metaData, int imageCount, (string ThumbnailPath, string MediumPath, string HighResPath) coverPaths) = extractor.ExtractMetadata(FilePath);
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


public interface IComicMetadataExtractor
{
  (bool needsMetaData, MetaData metaData, int pageCount, (string ThumbnailPath, string MediumPath, string HighResPath) coverImagePath) ExtractMetadata(string filePath);
}
public class ComicMetadataExtractor : IComicMetadataExtractor
{
  private readonly ISystemStorage _storage;
  string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

  public ComicMetadataExtractor(ISystemStorage storage)
  {
    _storage = storage ?? throw new ArgumentNullException(nameof(storage));
  }

  public (bool needsMetaData, MetaData metaData, int pageCount, (string ThumbnailPath, string MediumPath, string HighResPath) coverImagePath) ExtractMetadata(string filePath)
  {
    var needsMetaData = false;
    if (!filePath.EndsWith(".cbz", StringComparison.OrdinalIgnoreCase))
      throw new NotSupportedException("Unsupported file type");

    using FileStream zipToOpen = new FileStream(filePath, FileMode.Open);
    using var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read);

    var comicXML = archive.Entries.FirstOrDefault(e => e.Name == "ComicInfo.xml");
    var coverImage = archive.Entries.FirstOrDefault(e => imageExtensions.Any(e.Name.Contains));

    var subdirectories = archive.Entries
        .Where(entry => entry.FullName.Contains("/") && entry.Length > 0) // Entries with "/" are subdirectories
        .Select(entry => Path.GetDirectoryName(entry.FullName))
        .Distinct()
        .ToList();

    int pageCount = archive.Entries
          .Count(entry => imageExtensions.Any(ext => entry.FullName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));

    MetaData metaData = null;

    if (comicXML != null)
    {
      using var stream = comicXML.Open();
      var doc = XDocument.Load(stream);
      metaData = new MetaData(doc);
    }
    else
    {
      needsMetaData = true;
    }
    var coverImagePaths = _storage.CreateComicCoverImages(coverImage);
    return (needsMetaData, metaData, pageCount, coverImagePaths);
  }
}
