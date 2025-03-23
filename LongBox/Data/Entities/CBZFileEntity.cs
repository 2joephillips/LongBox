using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LongBox.Data.Entities
{
  [Table("CBZFiles")]
  public class CBZFileEntity
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public bool UnableToOpen { get; internal set; }

    [Required]
    public bool NeedsMetaData { get; internal set; }

    [Required]
    public bool HasSubdirectories { get; set; }

    [Required]
    public int PageCount { get; set; }
  }
}
