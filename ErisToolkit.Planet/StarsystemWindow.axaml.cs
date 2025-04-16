using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using ErisToolkit.Planet.ViewModels;
using Point = Avalonia.Point;

namespace ErisToolkit.Planet;

public class StarsystemWindowViewModel : ReactiveObject, IRoutableViewModel
{
    public IScreen HostScreen { get; }
    public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);

    public StarsystemWindowViewModel(IScreen screen)
    {
        HostScreen = screen;
    }

    public StarsystemWindowViewModel() {}
}

public partial class StarsystemWindow : ReactiveUserControl<StarsystemWindowViewModel>
{
    private StarsystemWindowViewModel _viewModel;

    private Point _lastPanPosition;
    private bool _isPanning;
    private ITransform _transformGroup;

    private ScaleTransform _scaleTransform;
    private TranslateTransform _translateTransform;

    private ObservableCollection<StarViewModel> Stars { get; set; }

    public StarsystemWindow()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
        InitializeComponent();

        var transformGroup = (TransformGroup)CanvasContainer.RenderTransform;
        _scaleTransform = (ScaleTransform)transformGroup.Children[0];
        _translateTransform = (TranslateTransform)transformGroup.Children[1];

        _viewModel = new StarsystemWindowViewModel();

        if (Common.currentMod != null) {
            Stars = LoadPlanets();
        }
    }

    public static ObservableCollection<StarViewModel> LoadPlanets()
    {
        var mod = Common.currentMod;
        ObservableCollection<StarViewModel> stars = new ObservableCollection<StarViewModel>();

        foreach (var star in mod.Stars)
        {
            if (star.BNAM == null) { continue; }
            var coords = star.BNAM.Value;

            stars.Add(
                new StarViewModel(star.Name ?? "", coords.X, coords.Z)
            );
        }
        return stars;
    }
}
