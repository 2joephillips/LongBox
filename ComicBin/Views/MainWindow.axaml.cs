using Avalonia.ReactiveUI;
using ComicBin.ViewModels;

namespace ComicBin.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
  
  public MainWindow()
  {
    InitializeComponent();
  }
}
