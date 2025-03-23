using System.Xml.Linq;

namespace LongBox.Core.Models;

public class PageMetaData
{
  public int Image { get; set; }
  public int ImageWidth { get; set; }
  public int ImageHeight { get; set; }
  public string Type { get; set; }

  public PageMetaData(XElement page)
  {
    Image = int.Parse(page.Attribute("Image")?.Value ?? "0");
    ImageWidth = int.Parse(page.Attribute("ImageWidth")?.Value ?? "0");
    ImageHeight = int.Parse(page.Attribute("ImageHeight")?.Value ?? "0");
    Type = page.Attribute("Type")?.Value ?? string.Empty;
  }

}