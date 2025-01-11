using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;

namespace ComicBin.Views.Controls;


public class NavigationButtonViewModel : ReactiveObject
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

public partial class NavigationButton : UserControl, IViewFor<NavigationButtonViewModel>
{
  public NavigationButton()
  {
    InitializeComponent();
    ViewModel = new NavigationButtonViewModel();

    this.WhenActivated(disposables =>
    {
      this.Bind(ViewModel, vm => vm.Icon, v => v.IconLabel.Text)
          .DisposeWith(disposables);
      this.Bind(ViewModel, vm => vm.Text, v => v.TextLabel.Text)
          .DisposeWith(disposables);
    });
  }

  public static readonly StyledProperty<NavigationButtonViewModel> ViewModelProperty =
      AvaloniaProperty.Register<NavigationButton, NavigationButtonViewModel>(nameof(ViewModel));

  public NavigationButtonViewModel ViewModel
  {
    get => GetValue(ViewModelProperty);
    set => SetValue(ViewModelProperty, value);
  }

  object IViewFor.ViewModel
  {
    get => ViewModel;
    set => ViewModel = (NavigationButtonViewModel)value;
  }
}