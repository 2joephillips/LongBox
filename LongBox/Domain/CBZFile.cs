using System;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using LongBox.Data.Entities;
using System.Xml.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LongBox.Domain;

public class CBZFile: CBZFileEntity
{
  string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

  public CBZFile(CBZFileEntity entity)
  {
    Id = entity.Id;
    FilePath = entity.FilePath;
    FileName = entity.FileName;
    UnableToOpen = entity.UnableToOpen;
    NeedsMetaData = entity.NeedsMetaData;
    HasSubdirectories = entity.HasSubdirectories;
    PageCount = entity.PageCount;
  }

  public CBZFile(string filePath)
  {
    try
    {
      if (string.IsNullOrEmpty(filePath))
        throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
      if (!filePath.EndsWith(".cbz", StringComparison.OrdinalIgnoreCase))
        throw new NotSupportedException("Unsupported file type");
      FilePath = filePath;
      FileName = filePath.Split('\\').LastOrDefault() ?? string.Empty;
      var parsedData = ParseZipFile();
      NeedsMetaData = parsedData.needsMetaData;
      PageCount = parsedData.pageCount;
      HasSubdirectories = parsedData.subdirectories.Count > 0;
    }
    catch (Exception)
    {
      UnableToOpen = true;
    }
  }

  public (bool needsMetaData, List<string> subdirectories, int pageCount) ParseZipFile()
  {
    using FileStream zipToOpen = new FileStream(FilePath, FileMode.Open);
    using var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read);
    var comicXML = archive.Entries.FirstOrDefault(e => e.Name == "ComicInfo.xml");
    var needsMetaData = comicXML == null;
    var subdirectories = archive.Entries
        .Where(entry => entry.FullName.Contains("/") && entry.Length > 0) // Entries with "/" are subdirectories
        .Select(entry => Path.GetDirectoryName(entry.FullName))
        .Distinct()
        .ToList();
    int pageCount = archive.Entries
          .Count(entry => imageExtensions.Any(ext => entry.FullName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));
    return (needsMetaData, subdirectories, pageCount);
  }

  public XDocument GetComicInfoXml()
  {
    using FileStream zipToOpen = new FileStream(FilePath, FileMode.Open);
    using var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read);
    var comicXML = archive.Entries.FirstOrDefault(e => e.Name == "ComicInfo.xml");
    using var stream = comicXML.Open();
    var doc = XDocument.Load(stream);
    return doc;
  }

  public Image<Rgba32> GetFirstImage()
  {
    using FileStream zipToOpen = new FileStream(FilePath, FileMode.Open);
    using var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read);
    var firstImage = archive.Entries.FirstOrDefault(e => imageExtensions.Any(e.Name.Contains));
    using var stream = firstImage.Open();
    return Image.Load<Rgba32>(stream); // Load the image into memory
  }

}
