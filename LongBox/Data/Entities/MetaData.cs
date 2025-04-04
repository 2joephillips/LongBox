﻿using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using LongBox.Services;
using LongBox.Core.Models;

namespace LongBox.Data.Entities;

public class MetaData
{
  public string Title { get; set; }
  public string Series { get; set; }
  public string Number { get; set; }
  public string Volume { get; set; }
  public string Notes { get; set; }
  public int? Year { get; set; }
  public int? Month { get; set; }
  public string Penciller { get; set; }
  public string Inker { get; set; }
  public string Publisher { get; set; }
  public string Web { get; set; }
  public int? PageCount { get; set; }
  public string Characters { get; set; }
  public string ScanInformation { get; set; }
  public List<PageMetaData> Pages { get; set; }
  
  public MetaData(XDocument doc)
  {
    Title = doc.GetRootElement("Title");
    Series = doc.GetRootElement("Series");
    Number = doc.GetRootElement("Number");
    Volume = doc.GetRootElement("Volume");
    Notes = doc.GetRootElement("Notes");
    Year = doc.GetRootElementAsInt("Year");
    Month = doc.GetRootElementAsInt("Month");
    Publisher = doc.GetRootElement("Publisher");
    PageCount = doc.GetRootElementAsInt("PageCount");
    Characters = doc.GetRootElement("Characters");
    ScanInformation = doc.GetRootElement("ScanInformation");
    Penciller = doc.GetRootElement("Penciller");
    Inker = doc.GetRootElement("Inker");
    Web = doc.GetRootElement("Web");
    Pages = doc.GetElement("Pages")?.Elements("Page").Select(p => new PageMetaData(p)).ToList() ?? [];

  }

}
