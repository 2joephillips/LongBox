using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using LongBox.ViewModels;

namespace LongBox;

public partial class ReaderWindow : ReactiveWindow<ReaderViewModel>
{
  public ReaderWindow()
  {
    InitializeComponent();

    if (Design.IsDesignMode) return;

    this.WhenActivated(action => action(ViewModel!.CloseComicCommand.Subscribe(Close)));
  }
}