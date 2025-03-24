using Avalonia.Controls;
using Avalonia.ReactiveUI;
using LongBox.ViewModels;
using LongBox.ViewModels.Pages;
using ReactiveUI;
using System.Threading.Tasks;

namespace LongBox.Views.Pages;

public partial class SetUpPageView : ReactiveUserControl<SetUpPageViewModel>
{
  public SetUpPageView()
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
    var mainWindow = TopLevel.GetTopLevel(this) as MainWindow; // Use 'this' to get the current window
    var result = await dialog.ShowDialog<ComicViewModel?>(mainWindow);
    interaction.SetOutput(result);
  }
}