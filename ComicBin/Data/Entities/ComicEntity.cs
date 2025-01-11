using ComicBin.Core.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System;
using System.Runtime.CompilerServices;

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
  [MaxLength(255)] // Adjust the length as appropriate
  public string FilePath { get; set; } = string.Empty;

  [Required]
  [MaxLength(150)] // Adjust the length as appropriate
  public string FileName { get; set; } = string.Empty;

  [DefaultValue(false)]
  public bool UnableToOpen { get; set; } 

  [DefaultValue(false)]
  public bool NeedsMetaData { get; set; } 

  [Required]
  public int? PageCount { get; set; }

  // These properties are for metadata but should not be persisted
  [NotMapped]
  public (string ThumbnailPath, string HighResPath) CoverImagePaths { get; set; }

  [NotMapped]
  public MetaData? MetaData { get; set; }
}
