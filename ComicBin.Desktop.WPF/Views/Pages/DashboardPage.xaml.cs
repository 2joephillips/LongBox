using ComicBin.Desktop.WPF.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace ComicBin.Desktop.WPF.Views.Pages
{
  public partial class DashboardPage : INavigableView<DashboardViewModel>
  {
    public DashboardViewModel ViewModel { get; }

    public DashboardPage(DashboardViewModel viewModel)
    {
      ViewModel = viewModel;
      DataContext = this;

      InitializeComponent();
    }
  }
}
