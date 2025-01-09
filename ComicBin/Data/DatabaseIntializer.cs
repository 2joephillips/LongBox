using ComicBin.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ComicBin.Data;

public class DatabaseIntializer
{
  private readonly ComicBinContext _context;

  public DatabaseIntializer(ComicBinContext context)
  {
    _context = context;
  }


  public async void EnsureCreated()
  {
    try
    {
      // Apply migrations
      await _context.Database.MigrateAsync();
      Console.WriteLine("Database migrated and up-to-date.");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error during migration: {ex.Message}");
      throw;
    }
  }

  public async Task<List<SettingEntity>> InitializeSettings()
  {
    var anySettings = _context.Settings.Any();
    if (anySettings)
    {
      return _context.Settings.ToList();
    };
    _context.Settings.AddRange(
        new SettingEntity { Key = ApplicationSettingKey.SetupComplete, Value = "false" }
        //new Setting { Key = ApplicationSettingKey.ThemeColor, Value = ApplicationTheme.Dark.ToString() }
        );
    await _context.SaveChangesAsync();
    return _context.Settings.ToList();
  }
}
