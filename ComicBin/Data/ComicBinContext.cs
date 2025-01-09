using ComicBin.Core.Models;
using ComicBin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace ComicBin.Data;

public partial class ComicBinContext : DbContext
{
  public DbSet<ComicEntity> Comics { get; set; }
  public DbSet<SettingEntity> Settings { get; set; }

  public ComicBinContext()
  {
  }

  public ComicBinContext(DbContextOptions<ComicBinContext> options)
      : base(options)
  {
  }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    if (!optionsBuilder.IsConfigured)
    {
      var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ComicBin", "comics.db");
      optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    OnModelCreatingPartial(modelBuilder);
  }

  partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
