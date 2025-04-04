using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using ErisToolkit.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using ReactiveUI;
using System.Reactive;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Mutagen.Bethesda.Starfield;
using ErisToolkit.Common.GameData;

namespace ErisToolkit.Planet;

public class MainWindowViewModel : ReactiveObject, IScreen
{
    public void OpenCanvas()
    {
        var canvasViewModel = new CanvasWindowViewModel(this);
        Router.Navigate.Execute(canvasViewModel);
    }

    public RoutingState Router { get; } = new RoutingState();

    public ReactiveCommand<Unit, IRoutableViewModel> OpenStarsystemScreen { get; }

    public MainWindowViewModel()
    {
        OpenStarsystemScreen = ReactiveCommand.CreateFromObservable(() =>
        {
            if (Common.mod == null)
            {
                var mainWindow = new MainWindow();
                var topLevel = TopLevel.GetTopLevel(mainWindow);
                var files = topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Open Plugin File",
                    AllowMultiple = false,
                    FileTypeFilter = new[] { Utils.PluginFilePicker }
                });

                if (files.Result.Count > 0 && files.Result[0] != null)
                {
                    string filePath = Uri.UnescapeDataString(files.Result[0].Path.AbsolutePath);
                    Common.mod = Utils.LoadMod(filePath);
                }
            }

            if (Router.NavigationStack.Count > 0)
            {
                return Router.NavigateBack.Execute();
            }
            else
            {
                return Router.Navigate.Execute(new StarsystemWindowViewModel(this));
            }
        });
    }
}

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    List<Avalonia.Controls.Image> images;
    public MainWindow()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
        InitializeComponent();

        images = [imgBiomGridN, imgBiomGridS, imgResGridN, imgResGridS];
    }

    public async void ImageOptionsClickHandler(object sender, RoutedEventArgs args)
    {

    }

    public async void ClickHandler(object sender, RoutedEventArgs args)
    {
        var topLevel = GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open .Biom File",
            AllowMultiple = false,
            FileTypeFilter = new[] { Utils.BiomFilePicker }
        });

        if (files.Count >= 1)
        {
            string filePath = Uri.UnescapeDataString(files[0].Path.AbsolutePath);
            string filename = Uri.UnescapeDataString(files[0].Name);

            Common.biom = new Biom(filePath);

            fileName.Text = $"Loaded {filename}";

            numBiomes.Text = Common.biom.biomStruct.NumBiomes.ToString();

            var picBiomGridN = Common.GetImage(0);
            var picBiomGridS = Common.GetImage(1);
            var picResGridS = Common.GetImage(2);
            var picResGridN = Common.GetImage(3);

            for (int i = 0; i < 4; i++)
            {
                var img = images[i];

                img.Source = Common.GetImage(i);

                // WIP: Implement canvas at first.
                /*
                var menu = new ContextMenu();

                menu.ItemsSource = new MenuItem[]
                {
                    new MenuItem
                    {
                        Header = $"View in Canvas...",
                        Command = ReactiveCommand.Create(() =>
                        {
                            if (DataContext is MainWindowViewModel vm)
                            {
                                Common.currentEditableIndex = i; // TODO: Not working properly
                                vm.OpenCanvas();
                            }
                        })
                    }
                };
                img.ContextMenu = menu;*/
            }

            textBiomGridN.IsVisible = true;
            textBiomGridS.IsVisible = true;
            textResGridN.IsVisible = true;
            textResGridS.IsVisible = true;
        }
    }
}
