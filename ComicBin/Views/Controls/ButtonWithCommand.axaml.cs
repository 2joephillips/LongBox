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
  
  public static readonly StyledProperty<string> TextProperty =
      AvaloniaProperty.Register<ButtonWithCommand, string>(nameof(Text));

  public string Text
  {
    get { return GetValue(TextProperty); }
    set { SetValue(TextProperty, value); }
  }

  public static readonly StyledProperty<ICommand> ClickCommandProperty =
      AvaloniaProperty.Register<ButtonWithCommand, ICommand>(nameof(ClickCommand));

  public ICommand ClickCommand
  {
    get { return GetValue(ClickCommandProperty); }
    set { SetValue(ClickCommandProperty, value); }
  }


}