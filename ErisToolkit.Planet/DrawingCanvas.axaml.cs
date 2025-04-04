
using Avalonia;
using Avalonia.Controls;
using Avalonia.Dialogs.Internal;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Drawing;
using Point = Avalonia.Point;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace ErisToolkit.Planet;

public class CanvasWindowViewModel : ReactiveObject, IRoutableViewModel
{
    public int BrushSize { get; set; } = 10;
    public IScreen HostScreen { get; }
    public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);

    public CanvasWindowViewModel(IScreen screen)
    {
        HostScreen = screen;
    }

    public CanvasWindowViewModel()
    {

    }

    public void StartDrawing(Point position)
    {
        DrawAt(position);
    }

    public void DrawAt(Point position)
    {
    }
}

public partial class CanvasWindow : ReactiveUserControl<CanvasWindowViewModel>
{
    private CanvasWindowViewModel _viewModel;
    public CanvasWindow()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
        InitializeComponent();

        _viewModel = new CanvasWindowViewModel();
        DrawingCanvas.Source = Common.GetImage();
    }
}
