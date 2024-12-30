using ComicBin.Desktop.Navigation;
using ComicBin.Desktop.ViewModels;
using ComicBin.Desktop.Views;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ComicBin.Desktop
{
  public partial class MainWindow : Window
  {
    public MainWindow(NavigationStore navigationStore)
    {
      InitializeComponent();
      navigationStore.CurrentViewModelChanged += () =>
      {
        DataContext = navigationStore.CurrentViewModel;
      };
    }
  }
}