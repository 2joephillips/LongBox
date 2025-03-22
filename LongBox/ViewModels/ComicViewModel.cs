using LongBox.Domain;

namespace LongBox.ViewModels
{
  public class ComicViewModel
  {
    private Comic? comic;

    public ComicViewModel()
    {
      this.comic = new Comic();
    }

    public ComicViewModel(Comic? comic)
    {
      this.comic = comic;
    }
  }
}