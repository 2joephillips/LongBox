using ComicBin.Desktop.WPF.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace ComicBin.Desktop.WPF.Views.Pages
{
  public partial class SettingsPage : INavigableView<SettingsViewModel>
  {
    public SettingsViewModel ViewModel { get; }

    public SettingsPage(SettingsViewModel viewModel)
    {
      ViewModel = viewModel;
      DataContext = this;

      InitializeComponent();
    }
  }
}
