using LongBox.Extensions;
using System;

namespace LongBox.ViewModels.Pages;

/// <summary>
///  This is our ViewModel for the first page
/// </summary>
public class AboutPageViewModel :PageViewModel
{
  protected AboutPageViewModel() : base(ApplicationPageNames.About)
  {
  }

  // The message to display
  public string Message => "Done";

}
