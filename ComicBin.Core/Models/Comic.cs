using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComicBin.Core.Models;


[Table("Comics")]
public class Comic
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  [Required]
  public Guid Guid { get; set; } = Guid.NewGuid();

  [Required]
  public string FilePath { get; set; }

  [Required]
  public string FileName { get; set; }

  [DefaultValue(false)]
  public bool UnableToOpen { get; set; } = false;

  [DefaultValue(false)]
  public bool NeedsMetaData { get; set; } = false;

  [Required]
  public int? PageCount { get; set; }

  [NotMapped]
  public (string ThumbnailPath, string MediumPath, string HighResPath) CoverImagePaths { get; set; }

  [NotMapped]
  public MetaData MetaData { get; set; }

  [NotMapped]
  public string GetHighResImagePath => CoverImagePaths.HighResPath;

  [NotMapped]
  public string Publisher => MetaData?.Publisher ?? "Unknown";

  [NotMapped]
  public string Title => string.IsNullOrEmpty(MetaData?.Title) ? "Unknown" : MetaData.Title;

  private readonly IComicMetadataExtractor _metadataExtractor;

  public Comic() : this(string.Empty, new ComicMetadataExtractor(new SystemStorage()))
  { }

  public Comic(string filePath, IComicMetadataExtractor metadataExtractor)
  {
    if (string.IsNullOrEmpty(filePath))
      throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
    FilePath = filePath;
    FileName = filePath.Split('\\').LastOrDefault() ?? string.Empty;
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
