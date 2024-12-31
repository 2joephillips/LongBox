using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ComicBin.ViewModels;

namespace ComicBin;

public partial class ReaderWindow : ReactiveWindow<ReaderViewModel>
{
  public ReaderWindow()
  {
    InitializeComponent();
  }
}