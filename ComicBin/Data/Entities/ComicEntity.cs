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


  [ForeignKey("CBZFileId")]
  public CBZFileEntity CBZFile { get; set; }

  // These properties are for metadata but should not be persisted
  [NotMapped]
  public (string ThumbnailPath, string HighResPath) CoverImagePaths { get; set; }

  [NotMapped]
  public MetaData? MetaData { get; set; }
}
