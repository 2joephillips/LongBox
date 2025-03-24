using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Threading.Tasks;
using LongBox.ViewModels;
using Avalonia.Controls;

namespace LongBox.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
  
  public MainWindow()
  {
    InitializeComponent();
    if (Design.IsDesignMode) return;

    this.WhenActivated(action =>
         action(ViewModel!.ShowReaderDialog.RegisterHandler(DoShowReaderDialogAsync)));
  }

  private async Task DoShowReaderDialogAsync(IInteractionContext<ReaderViewModel,
                                        ComicViewModel?> interaction)
  {
     var dialog = new ReaderWindow();
    dialog.DataContext = interaction.Input;

    var result= await dialog.ShowDialog<ComicViewModel?>(this);
    interaction.SetOutput(result);
  }
}
