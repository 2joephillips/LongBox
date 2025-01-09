using ComicBin.Core.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ComicBin.Data.Entities;

[Table("Comics")]
public class ComicEntity
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

  // These properties are for metadata but should not be persisted
  [NotMapped]
  public (string ThumbnailPath, string MediumPath, string HighResPath) CoverImagePaths { get; set; }

  [NotMapped]
  public MetaData MetaData { get; set; }
}
