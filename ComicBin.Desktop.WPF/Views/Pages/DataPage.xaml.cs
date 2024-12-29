using ComicBin.Desktop.WPF.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace ComicBin.Desktop.WPF.Views.Pages
{
  public partial class DataPage : INavigableView<DataViewModel>
  {
    public DataViewModel ViewModel { get; }

    public DataPage(DataViewModel viewModel)
    {
      ViewModel = viewModel;
      DataContext = this;

      InitializeComponent();
    }
  }
}
