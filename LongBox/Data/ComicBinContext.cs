using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using LongBox.Core.Models;
using LongBox.Data.Entities;

namespace LongBox.Data;

public partial class LongBoxContext : DbContext
{
  public DbSet<ComicEntity> Comics { get; set; }
  public DbSet<SettingEntity> Settings { get; set; }

  public LongBoxContext()
  {
  }

  public LongBoxContext(DbContextOptions<LongBoxContext> options)
      : base(options)
  {
  }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    if (!optionsBuilder.IsConfigured)
    {
      var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LongBox", "comics.db");
      optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    OnModelCreatingPartial(modelBuilder);
  }

  partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
