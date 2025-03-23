using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LongBox.Data;

namespace LongBox.Core.Models;

[Table("Settings")]
public class SettingEntity
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
