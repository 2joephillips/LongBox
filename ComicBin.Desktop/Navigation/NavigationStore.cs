namespace ComicBin.Desktop.Navigation;

public class NavigationStore
{
  public event Action CurrentViewModelChanged;

  private object _currentViewModel;
  public object CurrentViewModel
  {
    get => _currentViewModel;
    set
    {
      _currentViewModel = value;
      CurrentViewModelChanged?.Invoke();
    }
  }
}
