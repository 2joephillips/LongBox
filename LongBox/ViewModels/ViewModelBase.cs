using LongBox.Extensions;
using ReactiveUI;

namespace LongBox.ViewModels;

  public class ViewModelBase : ReactiveObject
  {
  }

public class PageViewModel: ViewModelBase
{
  private ApplicationPageNames _pageName;
  public ApplicationPageNames PageName
  {
    get => _pageName;
    private set => this.RaiseAndSetIfChanged(ref _pageName, value);
  }

  protected PageViewModel(ApplicationPageNames pageName)
  {
    PageName = pageName;

    if(Avalonia.Controls.Design.IsDesignMode)
      OnDesignTimeConstructor();
  }

  protected virtual void OnDesignTimeConstructor(){ }
}
