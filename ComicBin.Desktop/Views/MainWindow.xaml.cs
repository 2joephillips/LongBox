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
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow(MainWindowViewModel viewModel)
    {
      InitializeComponent();
      //_navigation = navigation;
    }
    private void HomePage_Click(object sender, RoutedEventArgs e)
    {
      //_navigation.NavigateAsync(typeof(Home));
    }

    private void SettingsPage_Click(object sender, RoutedEventArgs e)
    {
      //_navigation.NavigateAsync(typeof(Settings));
    }

    private void AboutPage_Click(object sender, RoutedEventArgs e)
    {
      //_navigation.NavigateAsync(typeof(About));
    }
  }
}