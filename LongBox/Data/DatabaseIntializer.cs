using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LongBox.Core.Models;

namespace LongBox.Data;

public class DatabaseIntializer
{
  private readonly LongBoxContext _context;

  public DatabaseIntializer(LongBoxContext context)
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
    if (!anySettings)
    {
      _context.Settings.AddRange(
          new SettingEntity { Key = ApplicationSettingKey.SetupComplete, Value = "false" }
          );
      await _context.SaveChangesAsync();
    }
    return _context.Settings.ToList();
  }
}
