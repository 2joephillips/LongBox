using System.Xml.Linq;

namespace ComicBin.Services;

public static class XDocumenExtensions
{
  public static XElement? GetElement(this XDocument document, string elementName)
  {
    return document.Root?.Element(elementName) ?? null;
  }

  public static string GetRootElement(this XDocument document, string elementName)
  {
    return document.Root?.Element(elementName)?.Value ?? "";
  }

  public static int? GetRootElementAsInt(this XDocument document, string elementName)
  {
    var value = document.GetRootElement(elementName);
    return int.TryParse(value, out var year) ? year : null;
  }
}