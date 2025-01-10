using ComicBin.Core.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ComicBin.Data;

public interface ISettingsRepository
{
  string? GetSetting(ApplicationSettingKey key);
  Task InsertOrUpdateSetting(ApplicationSettingKey key, string value);
}
public class SettingsRepository : ISettingsRepository
{
  private readonly ComicBinContext _context;

  public SettingsRepository(ComicBinContext context)
  {
    _context = context;
  }

  public string? GetSetting(ApplicationSettingKey key)
  {
    var setting = _context.Settings.FirstOrDefault(s => s.Key == key);
    return setting?.Value;
  }

  public async Task InsertOrUpdateSetting(ApplicationSettingKey key, string value)
  {
    var setting = _context.Settings.FirstOrDefault(s => s.Key == key);
    if (setting != null)
    {
      setting.Value = value;
      _context.Settings.Update(setting);
    }
    else
    {
      _context.Settings.Add(new SettingEntity
      {
        Key = key,
        Value = value
      });
    }
    await _context.SaveChangesAsync();
  }
}
