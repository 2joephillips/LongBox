using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComicBin.Core.Models;

[Table("Settings")]
public class Setting
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  [Required]
  [MaxLength(100)]
  public required ApplicationSettingKey Key { get; set; } // Stores the string representation of ApplicationSettingKey

  [Required]
  public required string Value { get; set; }
}
