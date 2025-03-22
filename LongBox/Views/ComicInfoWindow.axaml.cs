using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using LongBox.ViewModels;

namespace LongBox.Views;

public partial class ComicInfoWindow : ReactiveWindow<ComicViewModel>
{
    public ComicInfoWindow()
    {
        InitializeComponent();
    }
}