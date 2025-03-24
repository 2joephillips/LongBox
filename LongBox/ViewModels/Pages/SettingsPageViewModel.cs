using LongBox.Extensions;
using ReactiveUI;
using System;
using System.ComponentModel.DataAnnotations;

namespace LongBox.ViewModels.Pages;

/// <summary>
///  This is our ViewModel for the second page
/// </summary>
public class SettingsPageViewModel : PageViewModel
{
  public SettingsPageViewModel(): base(ApplicationPageNames.Settings)
  {
    // Listen to changes of MailAddress and Password and update CanNavigateNext accordingly
    //this.WhenAnyValue(x => x.MailAddress, x => x.Password)
    //    .Subscribe(_ => UpdateCanNavigateNext());
  }


  private string? _MailAddress;

  /// <summary>
  /// The E-Mail of the user. This field is required
  /// </summary>
  [Required]
  [EmailAddress]
  public string? MailAddress
  {
    get { return _MailAddress; }
    set { this.RaiseAndSetIfChanged(ref _MailAddress, value); }
  }


  private string? _Password;

  /// <summary>
  /// The password of the user. This field is required.
  /// </summary>
  [Required]
  public string? Password
  {
    get { return _Password; }
    set { this.RaiseAndSetIfChanged(ref _Password, value); }
  }

}
