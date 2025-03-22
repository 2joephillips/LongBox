using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using LongBox.Views;
using static System.Net.WebRequestMethods;

namespace LongBox.Services;

public class FolderScanningProgress
{
  public bool InProgress { get; set; }
  public int Current { get; set; }
  public int Total { get; set; }
  public string Message { get; set; }

  public FolderScanningProgress(bool inProgress, int current, int total, string message)
  {
    InProgress = inProgress;
    Current = current;
    Total = total;
    Message = message;
  }
}

public interface IFolderHandler
{
  string FolderNotSelected { get; }
  Task<FolderScanningProgress> ScanFolderResults(string folderName);
  Task<List<string>> ScanFolder(string folderName);
  Task<string> SelectRootFolder();
}


public class FolderHandler : IFolderHandler
{
  public string FolderNotSelected => "Folder Not Selected";
  public Task<List<string>> ScanFolder(string folderName)
  {
    if (folderName == FolderNotSelected) return Task.FromResult(new List<string>());
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
  public Task<FolderScanningProgress> ScanFolderResults(string folderName)
  {

    var filePaths = ScanFolder(folderName).Result;
    if (filePaths.Count() == 0)
      return Task.FromResult(new FolderScanningProgress(false, 0, 0, $"No comics found."));

    return Task.FromResult(new FolderScanningProgress(false, 0, 0, $"Found {filePaths.Count()} comics files."));
  }

  public async Task<string> SelectRootFolder()
  {
    var toplevel = TopLevel.GetTopLevel(new MainWindow());
    var pickedFolder = await toplevel?.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions() { AllowMultiple = false })!;
    var folderPath = pickedFolder.FirstOrDefault()?.TryGetLocalPath();
    var rootFolder = folderPath ?? string.Empty;
    return rootFolder;
  }



}
