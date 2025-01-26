using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace ComicBin.Services;

public interface IFolderHandler
{
  Task<List<string>> ScanFolder(string folderName);
}


public class FolderHandler : IFolderHandler
{
  public Task<List<string>> ScanFolder(string folderName)
  {
    return Task.Run(() =>
    {
      var supportedExtensions = new List<string>() { ".jpg", ".png", ".pdf", ".cbz", ".cbr" };

      // Get all file paths in the root directory and its subdirectories
      var filePaths = Directory.GetFiles(folderName, "*.*", SearchOption.AllDirectories)
          .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLower()))
          .ToList();

      return filePaths;
    });
  }
}
