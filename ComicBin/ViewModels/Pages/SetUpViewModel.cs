using System;

namespace ComicBin.ViewModels.Pages
{
  public class SetUpViewModel : PageViewModelBase
  {
    public override bool CanNavigateNext
    {
      get => true;
      protected set => throw new NotSupportedException();
    }
    public override bool CanNavigatePrevious
    {
      get => false;
      protected set => throw new NotSupportedException();
    }
  }
}
