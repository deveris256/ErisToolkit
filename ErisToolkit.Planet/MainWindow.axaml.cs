using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ErisToolkit.Common;
using System;
using System.Collections.Generic;
using ReactiveUI;
using System.Reactive;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ErisToolkit.Common.GameData;
using System.IO;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Input;
using Noggog;
using DynamicData;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Threading;
using Avalonia.Controls.Templates;
using System.Collections.Specialized;
using ErisToolkit.Planet.ViewModels;
using System.Linq;
using Avalonia.Media.Imaging;
using System.Xml.Linq;
using Mutagen.Bethesda.Starfield;
using static ErisToolkit.Common.GameData.Biom;

namespace ErisToolkit.Planet;

public partial class MainWindowViewModel : ObservableObject, IScreen
{
    [ObservableProperty]
    private string? _modName;

    [ObservableProperty]
    private StarInfo _CurrentStar;

    [ObservableProperty]
    private BiomInfo? _BiomFileInfo;

    public RoutingState Router { get; } = new RoutingState();

    public ReactiveCommand<Unit, IRoutableViewModel> OpenStarsystemScreen { get; }

    public MainWindowViewModel()
    {
        Common.LoadOrder.LoadOrder.CollectionChanged += UpdateModName;
    }

    private void UpdateModName(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ModName = $"{Common.currentMod?.ModKey.FileName}" ?? string.Empty;
    }

    public void LoadStar(int index)
    {
        var starName = Common.starList[index];
        var starView = Common.GetStar(index);

        if (starView == null) { return; }

        CurrentStar = starView;
    }
}

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    MainWindowViewModel viewModel;

    public MainWindow()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
        InitializeComponent();

        DataContext = viewModel;
    }

    /*
     * Adjusting the .biom list
     */
    private void OnBiomDataListKeyDown(object sender, KeyEventArgs e)
    {
        var biom = ((MainWindowViewModel)DataContext).BiomFileInfo;
        if (biom == null) { return; }

        var selData = biom.SelectedBiomDataListItem;
        if (selData == null) { return; }

        else if (biom.BiomesList.Contains(selData))
        {
            int biomDataIndex = biom.BiomesList.IndexOf(selData);
        }
        else if (biom.ResourcesList.Contains(selData))
        {
            int resDataIndex = biom.ResourcesList.IndexOf(selData);
        }
        e.Handled = true;
    }

    /*
     * If Enter key is pressed in the star search,
     * attempt to load the star into UI.
     */
    private void OnStarSearchKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Enter)
        {
            var autoCompleteBox = (AutoCompleteBox)sender;
            string enteredText = autoCompleteBox.Text;

            int matchedIndex = Common.starList.IndexOf(enteredText, StringComparer.OrdinalIgnoreCase);

            if (matchedIndex == -1)
            {
                e.Handled = true;
                return;
            }

            ((MainWindowViewModel)DataContext).LoadStar(matchedIndex);

            e.Handled = true;
        }
    }

    /*
     * Loads (usually) .esm to the software
     * as read-only.
     */
    public void LoadGamePluginReadOnlyClickHandler()
    {
        var biom = ((MainWindowViewModel)DataContext).BiomFileInfo;
        var topLevel = GetTopLevel(this);
        var files = topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Plugin File",
            AllowMultiple = false,
            FileTypeFilter = new[] { Utils.PluginFilePicker }
        });

        if (files.Result.Count == 0 || files.Result[0] == null) { return; }

        string filePath = Uri.UnescapeDataString(files.Result[0].Path.AbsolutePath);
        Common.LoadOrder.AddModToLoadOrder(filePath, topLevel, false);

        if (biom == null) { return; }
        biom.AddBiomeData();
        biom.AddResourceData();
    }

    /*
     * Loads (usually) .esm to the software
     * as editable.
     */
    public void LoadGamePluginEditableClickHandler()
    {
        var topLevel = GetTopLevel(this);
        var files = topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Plugin File for Editing",
            AllowMultiple = false,
            FileTypeFilter = new[] { Utils.PluginFilePicker }
        });

        if (files.Result.Count == 0 || files.Result[0] == null) { return; }

        string filePath = Uri.UnescapeDataString(files.Result[0].Path.AbsolutePath);
        Common.LoadOrder.AddModToLoadOrder(filePath, topLevel, true);

        var biom = ((MainWindowViewModel)DataContext).BiomFileInfo;

        if (biom == null) { return; }
        biom.AddBiomeData();
        biom.AddResourceData();
    }

    /*
     * Used for copying colors of Biome && Resource data
     * in UI
     */
    public void CopyColor(object sender, RoutedEventArgs args)
    {
        Clipboard.SetTextAsync($"#{((Button)sender).Background.ToString().Remove(0,3)}");
    }

    /*
     * Selects an image 'slot' in UI; Used in
     * saving images
     */
    public void SelectBiomeDataImageClickHandler(object sender, RoutedEventArgs args)
    {
        var biom = ((MainWindowViewModel)DataContext).BiomFileInfo;
        if (biom == null) { return; }

        foreach (var button in biom.ImageButtons)
        {
            button.IsSelected = false;
        }

        var name = ((Button)sender).Name;
        var index = biom.ImageButtons
                        .Select(x => x.Name)
                        .IndexOf(name);

        biom.ImageButtons[index].IsSelected = true;
    }

    /*
     * Saves .biom file
     */
    public async void SaveBiomFile()
    {
        var biom = ((MainWindowViewModel)DataContext).BiomFileInfo;
        if (biom == null) { return; }

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

            biom.BiomData.biomStruct.Write(writer);
        }
    }

    /*
     * Loads biom file to UI
     */
    public async void LoadBiomFile()
    {
        var topLevel = GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open .Biom File",
            AllowMultiple = false,
            FileTypeFilter = new[] { Utils.BiomFilePicker }
        });

        if (files.Count == 0) { return; }

        string filePath = Uri.UnescapeDataString(files[0].Path.AbsolutePath);

        ((MainWindowViewModel)DataContext).BiomFileInfo = new(filePath);
    }

    /*
     * Saves image from selected .biom slot to
     * disk.
     */
    public async void SaveBiomDataImageClickHandler()
    {
        var biom = ((MainWindowViewModel)DataContext).BiomFileInfo;

        if (biom == null) { return; }

        var topLevel = GetTopLevel(this);
        var name = "";
        WriteableBitmap? image = null;

        foreach (var button in biom.ImageButtons)
        {
            if (button.IsSelected)
            {
                name = button.Name;
                image = button.Image;
                break;
            }
        }

        if (image == null) { return; }

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Image To Disk",
            DefaultExtension = "png",
            SuggestedFileName = $"{name}.png",
            FileTypeChoices = new[] { Utils.PngFilePicker }
        });

        if (file != null) { image.Save(file.Path.AbsolutePath.Replace("%20", " ")); }
    }

    /*
     * Replaces image of specific UI slot of biome
     * image grids
     */
    public async void ReplaceBiomDataImageClickHandler()
    {
        var biom = ((MainWindowViewModel)DataContext).BiomFileInfo;

        if (biom == null) { return; }

        var topLevel = GetTopLevel(this);
        string? imgType = null;
        BiomDataSide imgSide = BiomDataSide.NULL;

        foreach (var button in biom.ImageButtons)
        {
            if (button.IsSelected)
            {
                imgType = button.DataType;
                imgSide = button.Side;
                break;
            }
        }
        if (imgType == null || imgSide == BiomDataSide.NULL) { return; }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open .png File",
            AllowMultiple = false,
            FileTypeFilter = new[] { Utils.PngFilePicker }
        });

        if (files.Count == 0) { return; }

        var absPath = files[0].Path.AbsolutePath.Replace("%20", " ");
        System.Drawing.Bitmap loadedBitmap = new(absPath);

        if (loadedBitmap == null) { return; }

        switch (imgType)
        {
            case "biome":
                ((MainWindowViewModel)DataContext).BiomFileInfo.GatherBiomeDataFromImage(loadedBitmap, imgSide);
                break;
            case "res":
                ((MainWindowViewModel)DataContext).BiomFileInfo.GatherResourceDataFromImage(loadedBitmap, imgSide);
                break;
        }
    }
}
