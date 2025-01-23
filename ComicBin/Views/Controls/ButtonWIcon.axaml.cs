using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;

namespace ComicBin.Views.Controls;
public class ButtonWIconViewModel : ReactiveObject
{
  private string _icon = "\uE2C2";
  private string _text = "Home";

  public string Icon
  {
    get => _icon;
    set => this.RaiseAndSetIfChanged(ref _icon, value);
  }

  public string Text
  {
    get => _text;
    set => this.RaiseAndSetIfChanged(ref _text, value);
  }
}

public partial class ButtonWIcon :UserControl, IViewFor<ButtonWIconViewModel>
{
  public ButtonWIcon()
    {
        InitializeComponent();
        ViewModel = new ButtonWIconViewModel();

    this.WhenActivated(disposables =>
    {
      this.Bind(ViewModel, vm => vm.Icon, v => v.IconLabel.Content)
          .DisposeWith(disposables);
      this.Bind(ViewModel, vm => vm.Text, v => v.TextLabel.Content)
          .DisposeWith(disposables);
    });
  }
  public static readonly StyledProperty<ButtonWIconViewModel> ViewModelProperty =
     AvaloniaProperty.Register<ButtonWIcon, ButtonWIconViewModel>(nameof(ViewModel));

  public ButtonWIconViewModel ViewModel
  {
    get => GetValue(ViewModelProperty);
    set => SetValue(ViewModelProperty, value);
  }

  object IViewFor.ViewModel
  {
    get => ViewModel;
    set => ViewModel = (ButtonWIconViewModel)value;
  }
}