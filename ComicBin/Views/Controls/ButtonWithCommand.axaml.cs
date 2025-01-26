using Avalonia;
using Avalonia.Controls.Primitives;
using System.Windows.Input;

namespace ComicBin.Views.Controls;

public class ButtonWithCommand : TemplatedControl
{
  public static readonly StyledProperty<string> IconProperty =
      AvaloniaProperty.Register<ButtonWithCommand, string>(nameof(Icon));

  public string Icon
  {
    get { return GetValue(IconProperty); }
    set { SetValue(IconProperty, value); }
  }

  public static readonly StyledProperty<bool> HasIconProperty =
      AvaloniaProperty.Register<ButtonWithCommand, bool>(nameof(HasIcon));

  public bool HasIcon
  {
    get { return GetValue(HasIconProperty); }
    set { SetValue(HasIconProperty, value); }
  }


  public static readonly StyledProperty<string> TextProperty =
      AvaloniaProperty.Register<ButtonWithCommand, string>(nameof(Text));

  public string Text
  {
    get { return GetValue(TextProperty); }
    set { SetValue(TextProperty, value); }
  }

  public static readonly StyledProperty<ICommand?> CommandProperty =
             AvaloniaProperty.Register<ButtonWithCommand, ICommand?>(nameof(Command), enableDataValidation: true);

  public ICommand? Command
  {
    get => GetValue(CommandProperty);
    set => SetValue(CommandProperty, value);
  }

}