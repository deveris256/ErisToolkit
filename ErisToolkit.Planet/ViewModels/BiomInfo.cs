using CommunityToolkit.Mvvm.ComponentModel;
using ErisToolkit.Common.GameData;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Starfield;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Collections.ObjectModel;

namespace ErisToolkit.Planet.ViewModels;

public partial class BiomInfo : ObservableObject, INotifyPropertyChanged
{
    [ObservableProperty]
    string _FileName;

    [ObservableProperty]
    private Biom _BiomData;

    // Used in UI only
    [ObservableProperty]
    public ObservableCollection<BiomDataList> _BiomesList = new();

    [ObservableProperty]
    public ObservableCollection<BiomDataList> _ResourcesList = new();

    public int NumBiomes { get => BiomData == null ? -1 : (int)BiomData.biomStruct.NumBiomes; }
    public int NumResources { get => BiomData == null ? -1 : ResourcesList.Count; }

    [ObservableProperty]
    public ObservableCollection<BiomImage> _ImageButtons = new();

    public event PropertyChangedEventHandler? CustomPropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        CustomPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public BiomInfo(string path)
    {
        FileName = Path.GetFileName(path);

        SetBiom(new(path));
    }

    /* Used in loading images from biom struct of
     * the object to UI
     */
    public void LoadImages()
    {
        ImageButtons.Clear();
        ImageButtons.Add(new(
            "biome",
            "Biomes North",
            BiomData.GetBiomeImage(BiomData.biomStruct.BiomeGridN),
            Biom.BiomDataSide.N));
        ImageButtons.Add(new(
            "biome",
            "Biomes South",
            BiomData.GetBiomeImage(BiomData.biomStruct.BiomeGridS),
            Biom.BiomDataSide.S));
        ImageButtons.Add(new(
            "res",
            "Resources North",
            BiomData.GetResourceImage(BiomData.biomStruct.ResrcGridN),
            Biom.BiomDataSide.N));
        ImageButtons.Add(new(
            "res",
            "Resources South",
            BiomData.GetResourceImage(BiomData.biomStruct.ResrcGridS),
            Biom.BiomDataSide.S));

    }

    /*
     * Sets biom struct of the object,
     * updating the UI too
     */
    public void SetBiom(string filePath)
    {
        BiomData = new(filePath);
        OnPropertyChanged(nameof(BiomData));
        OnPropertyChanged(nameof(NumBiomes));
        OnPropertyChanged(nameof(NumResources));
        AddBiomeData();
        AddResourceData();
        LoadImages();
    }

    /*
     * Populates UI biome list based on the
     * biom struct of the object.
     */
    public void AddBiomeData()
    {
        BiomesList.Clear();

        if (BiomData == null) return;

        List<uint> biomeIds = new();

        // Get all unique biome ids
        for (int i = 0; i < BiomData.biomStruct.NumBiomes; i++)
        {
            var biomeId = BiomData.biomStruct.BiomeIds[i];
            if (biomeIds.Contains(biomeId)) { continue; }

            biomeIds.Add(biomeId);
        }

        // Map biome IDs
        foreach (var biomeId in biomeIds)
        {
            bool found = false;

            var color = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(
                        (byte)Biom.palette.paletteData[biomeIds.IndexOf(biomeId)][0],
                        (byte)Biom.palette.paletteData[biomeIds.IndexOf(biomeId)][1],
                        (byte)Biom.palette.paletteData[biomeIds.IndexOf(biomeId)][2]
                ), 1);

            foreach (var loMod in Common.LoadOrder.LoadOrder)
            {
                var lmod = loMod.LoadOrderMod;

                if (Common.linkCache != null &&
                Common.linkCache.TryResolve<IBiomeGetter>(FormKey.Factory($"{biomeId:x6}:{lmod.ModKey.FileName}"), out var formLink))
                {
                    BiomesList.Add(new BiomDataList(
                        formLink,
                        color
                    ));
                    found = true;
                    break;
                }
            }

            // Add dummy if not found
            if (!found)
            {
                BiomesList.Add(new BiomDataList(
                        biomeId,
                        color
                    ));
            }
        }   
    }

    /*
     * Populates UI resource list based on the
     * biom struct of the object.
     */
    public void AddResourceData()
    {
        ResourcesList.Clear();

        if (BiomData == null) return;

        List<byte> resourceIdsList = new();

        // Get all unique resource ids
        for (int i = 0; i < BiomData.biomStruct.GridFlatSize; i++)
        {
            byte resourceIdN = (byte)Array.IndexOf(Biom.known_resource_ids, BiomData.biomStruct.ResrcGridN[i]);
            byte resourceIdS = (byte)Array.IndexOf(Biom.known_resource_ids, BiomData.biomStruct.ResrcGridS[i]);

            if (!resourceIdsList.Contains(resourceIdN))
            {
                resourceIdsList.Add(resourceIdN);
                var color = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(
                    (byte)Biom.palette.paletteData[resourceIdN][0],
                    (byte)Biom.palette.paletteData[resourceIdN][1],
                    (byte)Biom.palette.paletteData[resourceIdN][2]
                ), 1);

                ResourcesList.Add(new BiomDataList(
                    resourceIdN,
                    color
                ));
            }

            if (!resourceIdsList.Contains(resourceIdS))
            {
                resourceIdsList.Add(resourceIdS);
                var color = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(
                    (byte)Biom.palette.paletteData[resourceIdS][0],
                    (byte)Biom.palette.paletteData[resourceIdS][1],
                    (byte)Biom.palette.paletteData[resourceIdS][2]
                ), 1);

                ResourcesList.Add(new BiomDataList(
                    resourceIdS,
                    color
                ));
            }
        }
    }
}

