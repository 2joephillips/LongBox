using Avalonia;
using Avalonia.Controls.Primitives;
using System.Windows.Input;

namespace ComicBin.Views.Controls;

public class IconButtonWithCommand : TemplatedControl
{
  public static readonly StyledProperty<string> IconProperty =
    AvaloniaProperty.Register<ButtonWithCommand, string>(nameof(Icon));

  public string Icon
  {
    get { return GetValue(IconProperty); }
    set { SetValue(IconProperty, value); }
  }

  public static readonly StyledProperty<ICommand?> CommandProperty =
             AvaloniaProperty.Register<ButtonWithCommand, ICommand?>(nameof(Command), enableDataValidation: true);

  public ICommand? Command
  {
    get => GetValue(CommandProperty);
    set => SetValue(CommandProperty, value);
  }

}