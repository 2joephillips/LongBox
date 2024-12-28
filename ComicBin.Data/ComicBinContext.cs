using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ComicBin.Data;

public partial class ComicBinContext : DbContext
{
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
