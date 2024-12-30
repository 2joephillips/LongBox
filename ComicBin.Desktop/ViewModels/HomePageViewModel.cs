using ComicBin.Desktop.Navigation;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ComicBin.Desktop.ViewModels
{
  internal class HomePageViewModel
  {
    private readonly NavigationStore _navigationStore;

    public RelayCommand NavigateToSettingsCommand { get; }

    public HomePageViewModel(NavigationStore navigationStore, IServiceProvider serviceProvider)
    {
      _navigationStore = navigationStore;
      NavigateToSettingsCommand = new RelayCommand(() =>
      {
        _navigationStore.CurrentViewModel = serviceProvider.GetRequiredService<SettingsPageViewModel>();
      });
    }
  }
}
