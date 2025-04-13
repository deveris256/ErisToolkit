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
using System.IO;
using System.Collections.ObjectModel;
using Reloaded.Memory.Pointers;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using Avalonia.Styling;

namespace ErisToolkit.Planet;

public partial class MainWindowViewModel : ReactiveObject, IScreen
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
    List<Avalonia.Controls.Button> buttons;
    MainWindowViewModel viewModel;

    public MainWindow()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
        InitializeComponent();

        Biom.LoadPalette();

        DataContext = viewModel;

        images = [imgBiomGridN, imgBiomGridS, imgResGridN, imgResGridS];
        buttons = [BiomGridN, BiomGridS, ResGridN, ResGridS];

        BiomesList.ItemsSource = Common.biomesList;
        ResourcesList.ItemsSource = Common.resourcesList;
    }

    public async void LoadEsmClickHandler()
    {
        var topLevel = GetTopLevel(this);
        var files = topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Plugin File",
            AllowMultiple = false,
            FileTypeFilter = new[] { Utils.PluginFilePicker }
        });

        if (files.Result.Count > 0 && files.Result[0] != null)
        {
            string filePath = Uri.UnescapeDataString(files.Result[0].Path.AbsolutePath);

            Common.AddModToLoadOrder(Utils.LoadMod(filePath));

            if (Common.mod != null) esmName.Text = $"Loaded {Common.mod.ModKey.FileName}";
        }
    }

    public void CopyColor(object sender, RoutedEventArgs args)
    {
        Clipboard.SetTextAsync($"#{((Button)sender).Background.ToString().Remove(0,3)}");
    }

    public async void SelectImageClickHandler(object sender, RoutedEventArgs args)
    {
        foreach (var button in buttons)
        {
            button.Background = null;
            button.IsEnabled = true;
        }

        ((Button)sender).Background = Avalonia.Media.Brushes.Aquamarine;
        ((Button)sender).IsEnabled = false;
    }

    public async void SaveBiomFile()
    {
        if (Common.biom == null) return;

        var topLevel = GetTopLevel(this);

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save .Biom File",
            DefaultExtension = "biom"
        });

        if (file != null)
        {
            await using var stream = await file.OpenWriteAsync();
            using var writer = new BinaryWriter(stream);

            Common.biom.biomStruct.Write(writer);
        }
    }

    public async void LoadBiomFile()
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
            }

            textBiomGridN.IsVisible = true;
            textBiomGridS.IsVisible = true;
            textResGridN.IsVisible = true;
            textResGridS.IsVisible = true;
        }
    }

    public async void SlotSaveImage()
    {
        if (Common.biom == null) { return; }

        var topLevel = GetTopLevel(this);
        Button selButton = null;

        foreach (var button in buttons)
        {
            if (button.IsEnabled == false)
            {
                selButton = button;
                break;
            }
        }

        if (selButton == null)
        {
            return;
        }

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Image To Disk",
            DefaultExtension = "png",
            SuggestedFileName = $"{selButton.Name}.png"
        });

        int index = Common.StringToIndex(selButton.Name);
        if (index == -1) return;

        Common.currentEditableIndex = index;

        if (file != null)
        {
            var img = Common.GetBitmap();

            if (img != null) { img.Save(file.Path.AbsolutePath); }
        }
    }

    public async void SlotReplaceImage()
    {
        if (Common.biom == null) { return; }

        var topLevel = GetTopLevel(this);
        Button selButton = null;

        foreach (var button in buttons)
        {
            if (button.IsEnabled == false)
            {
                selButton = button;
                break;
            }
        }

        if (selButton == null) { return; }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open .png File",
            AllowMultiple = false,
            FileTypeFilter = new[] { Utils.PngFilePicker }
        });

        int index = Common.StringToIndex(selButton.Name);
        if (index == -1) return;

        Common.currentEditableIndex = index;

        if (files.Count >= 1)
        {
            var img = new System.Drawing.Bitmap(files[0].Path.AbsolutePath);

            switch (index)
            {
                case 0:
                    Common.biom.LoadBiomeImage(img, 0);
                    imgBiomGridN.Source = Utils.ConvertToAvaloniaBitmap(Common.biom.GetBiomeImage(Common.biom.biomStruct.BiomeGridN));
                    break;
                case 1:
                    Common.biom.LoadBiomeImage(img, 1);
                    imgBiomGridS.Source = Utils.ConvertToAvaloniaBitmap(Common.biom.GetBiomeImage(Common.biom.biomStruct.BiomeGridS));
                    break;

                case 2:
                    Common.biom.LoadResourceImage(img, 0);
                    imgResGridN.Source = Utils.ConvertToAvaloniaBitmap(Common.biom.GetResourceImage(Common.biom.biomStruct.ResrcGridN));
                    break;
                case 3:
                    Common.biom.LoadResourceImage(img, 1);
                    imgResGridS.Source = Utils.ConvertToAvaloniaBitmap(Common.biom.GetResourceImage(Common.biom.biomStruct.ResrcGridS));
                    break;
            }
        }
    }
}
