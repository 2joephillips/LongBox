﻿using LongBox.Extensions;
using System;

namespace LongBox.ViewModels.Pages;

/// <summary>
///  This is our ViewModel for the first page
/// </summary>
public class HomePageViewModel : PageViewModel
{
  protected HomePageViewModel() : base(ApplicationPageNames.Home)
  {
  }

  /// <summary>
  /// The Title of this page
  /// </summary>
  public string Title => "Welcome to our Wizard-Sample.";

  /// <summary>
  /// The content of this page
  /// </summary>
  public string Message => "Press \"Next\" to register yourself.";
}
