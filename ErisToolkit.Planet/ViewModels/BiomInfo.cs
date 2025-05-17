using CommunityToolkit.Mvvm.ComponentModel;
using ErisToolkit.Common.GameData;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Starfield;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Collections.ObjectModel;
using static ErisToolkit.Common.GameData.Biom;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;
using Noggog;
using DynamicData;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Controls;

namespace ErisToolkit.Planet.ViewModels;

public partial class BiomInfo : ObservableObject, INotifyPropertyChanged
{
    private List<List<int>> DefaultPalette;

    [ObservableProperty]
    private object _SelectedBiomDataListItem;

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
        DefaultPalette = JsonConvert.DeserializeObject<Dictionary<string, List<List<int>>>>(Properties.Resources.palette1)["palette"];
        FileName = Path.GetFileName(path);

        SetBiom(new(path));
    }

    public void RemoveBiome(int index)
    {
        BiomData.RemoveBiome(index);
        AddBiomeData();
    }

    public async void SaveBiomFile(TopLevel topLevel)
    {
        var biomesUnassigned = BiomesList.Select(x => x.Assigned == false);
        var resourcesUnassigned = ResourcesList.Where(x => x.Assigned == false);

        if (biomesUnassigned.Count() + resourcesUnassigned.Count() != 0)
        {
            // For now, return. Later - notify the user on the
            // notifications panel. TODO
            return;
        }

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save .Biom File",
            DefaultExtension = "biom"
        });

        if (file != null)
        {
            await using var stream = await file.OpenWriteAsync();
            using var writer = new BinaryWriter(stream);
            BiomData.biomStruct.Write(writer);
        }
    }

    /*
     * Gathers biome data from an image, loading unique
     * colors as data items.
     */
    public void GatherBiomeDataFromImage(System.Drawing.Bitmap bitmap, BiomDataSide side)
    {
        if (bitmap.Size.Width != (int)gridSize[0] || bitmap.Size.Height != (int)gridSize[1])
        {
            return;
        }

        List<Color> uniqueColors = BiomesList.Select(x => x.Color).ToList();
        UInt32[] biomGrid;

        switch (side)
        {
            case BiomDataSide.N: biomGrid = BiomData.biomStruct.BiomeGridN; break;
            case BiomDataSide.S: biomGrid = BiomData.biomStruct.BiomeGridS; break;
            default: return;
        }

        for (int i = 0; i < gridFlatSize; i++)
        {
            int x = i % (int)gridSize[0];
            int y = i / (int)gridSize[0];

            Color pixel = bitmap.GetPixel(x, y);

            if (!uniqueColors.Contains(pixel))
            {
                uniqueColors.Add(pixel);
            }
        }

        for (int i = 0; i < gridFlatSize; i++)
        {
            int x = i % (int)gridSize[0];
            int y = i / (int)gridSize[0];

            Color pixel = bitmap.GetPixel(x, y);
            
            if (BiomesList.Where(x => x.Color == pixel).Count() == 0) {
                var tempId = (uint)-uniqueColors.IndexOf(pixel);
                while (BiomData.biomStruct.BiomeIds.Contains(tempId))
                {
                    tempId -= 1;
                }
                BiomData.AddBiome(tempId);
                BiomesList.Add(new(null, tempId, pixel, BiomDataList.DataTypes.Biome, false));
            }

            biomGrid[i] = (uint)BiomesList.Where(x => x.Color == pixel).First().Data;
        }

        BiomData.ReplaceBiomeData(biomGrid, side);
        CleanUpUnusedBiomeData();
        LoadImages();
    }

    public void CleanUpUnusedBiomeData()
    {
        var dataN = BiomData.biomStruct.BiomeGridN;
        var dataS = BiomData.biomStruct.BiomeGridS;

        List<uint> uniqueIDData = new();

        for (int i = 0; i < gridFlatSize; i++)
        {
            int x = i % (int)gridSize[0];
            int y = i / (int)gridSize[0];

            uint data1 = dataN[i];
            uint data2 = dataS[i];

            if (!uniqueIDData.Contains(data1)) { uniqueIDData.Add(data1); }
            if (!uniqueIDData.Contains(data2)) { uniqueIDData.Add(data2); }
        }

        var currentIDs = BiomesList.Select(x => (uint)x.Data).ToList();

        foreach (uint curID in currentIDs)
        {
            if (!uniqueIDData.Contains(curID))
            {
                int idx = BiomesList.Select(x => (uint)x.Data).ToList().IndexOf(curID);
                BiomData.RemoveBiome(idx);
                BiomesList.RemoveAt(idx);
            }
        }
    }

    /*
     * Gathers resource data from an image, loading unique
     * colors as data items.
     */
    public void GatherResourceDataFromImage(System.Drawing.Bitmap bitmap, BiomDataSide side)
    {
        if (bitmap.Size.Width != (int)gridSize[0] || bitmap.Size.Height != (int)gridSize[1])
        {
            return;
        }

        List<Color> uniqueColors = ResourcesList.Select(x => x.Color).ToList();
        byte[] resGrid;

        switch (side)
        {
            case BiomDataSide.N: resGrid = BiomData.biomStruct.ResrcGridN; break;
            case BiomDataSide.S: resGrid = BiomData.biomStruct.ResrcGridS; break;
            default: return;
        }

        for (int i = 0; i < gridFlatSize; i++)
        {
            int x = i % (int)gridSize[0];
            int y = i / (int)gridSize[0];

            Color pixel = bitmap.GetPixel(x, y);

            if (!uniqueColors.Contains(pixel))
            {
                uniqueColors.Add(pixel);
            }
        }

        for (int i = 0; i < gridFlatSize; i++)
        {
            int x = i % (int)gridSize[0];
            int y = i / (int)gridSize[0];

            Color pixel = bitmap.GetPixel(x, y);

            if (ResourcesList.Where(x => x.Color == pixel).Count() == 0)
            {
                var tempId = (byte)-uniqueColors.IndexOf(pixel);
                var tempIdList = ResourcesList.Select(x => (byte)x.Data);

                while (tempIdList.Contains(tempId))
                {
                    tempId -= 1;
                }
                ResourcesList.Add(new(null, tempId, pixel, BiomDataList.DataTypes.Resource, false));
            }

            resGrid[i] = (byte)ResourcesList.Where(x => x.Color == pixel).First().Data;
        }

        BiomData.ReplaceResourceData(resGrid, side);
        CleanUpUnusedResourceData();
        LoadImages();
    }

    public void CleanUpUnusedResourceData()
    {
        var dataN = BiomData.biomStruct.ResrcGridN;
        var dataS = BiomData.biomStruct.ResrcGridS;

        List<byte> uniqueIDData = new();

        for (int i = 0; i < gridFlatSize; i++)
        {
            int x = i % (int)gridSize[0];
            int y = i / (int)gridSize[0];

            byte data1 = dataN[i];
            byte data2 = dataS[i];

            if (!uniqueIDData.Contains(data1)) { uniqueIDData.Add(data1); }
            if (!uniqueIDData.Contains(data2)) { uniqueIDData.Add(data2); }
        }

        var currentIDs = ResourcesList.Select(x => (byte)x.Data).ToList();

        foreach (var curID in currentIDs)
        {
            if (!uniqueIDData.Contains(curID))
            {
                int idx = ResourcesList.Select(x => (byte)x.Data).ToList().IndexOf(curID);
                ResourcesList.RemoveAt(idx);
            }
        }
    }

    /*
     * Gets image from biome data grid
     */
    public System.Drawing.Bitmap GetBiomeImage(uint[] grid)
    {
        System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap((int)gridSize[0], (int)gridSize[1]);
        Console.WriteLine(BiomesList);
        for (int i = 0; i < gridFlatSize; i++)
        {
            var biomeID = grid[i];
            var colors = BiomesList.Where(x => (uint)x.Data == biomeID);
            if (colors.Count() == 0)
            {
                string temp = $"Can't find {biomeID} in " + string.Join(",", BiomesList.Select(x => x.ID).ToArray());
                throw new Exception(temp);
            }
            var color = colors.First().Color;

            int x = i % (int)gridSize[0];
            int y = i / (int)gridSize[0];
            bitmap.SetPixel(x, y, color);
        }

        return bitmap;
    }

    /*
     * Gets image from resource data grid
     */
    public System.Drawing.Bitmap GetResourceImage(byte[] grid)
    {
        System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap((int)gridSize[0], (int)gridSize[1]);

        for (int i = 0; i < gridFlatSize; i++)
        {
            int resourceID = grid[i];
            Color color = ResourcesList.Where(x => (byte)x.Data == resourceID).First().Color;

            int x = i % (int)gridSize[0];
            int y = i / (int)gridSize[0];
            bitmap.SetPixel(x, y, color);
        }

        return bitmap;
    }

    //TODO
    public void AddBiome(uint id)
    {
        BiomData.AddBiome(id);
        AddBiomeData();
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
            GetBiomeImage(BiomData.biomStruct.BiomeGridN),
            Biom.BiomDataSide.N));
        ImageButtons.Add(new(
            "biome",
            "Biomes South",
            GetBiomeImage(BiomData.biomStruct.BiomeGridS),
            Biom.BiomDataSide.S));
        ImageButtons.Add(new(
            "res",
            "Resources North",
            GetResourceImage(BiomData.biomStruct.ResrcGridN),
            Biom.BiomDataSide.N));
        ImageButtons.Add(new(
            "res",
            "Resources South",
           GetResourceImage(BiomData.biomStruct.ResrcGridS),
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
        if (BiomData == null) return;

        var temp = BiomesList.Select(x => (uint)x.Data);
        List<uint> biomeIds = BiomData.biomStruct.BiomeIds.ToList().Where(x=> !temp.Contains(x)).ToList();

        // Map biome IDs
        foreach (var biomeId in biomeIds)
        {
            bool found = false;

            foreach (var loMod in Common.LoadOrder.LoadOrder)
            {
                var lmod = loMod.LoadOrderMod;

                if (Common.linkCache != null &&
                Common.linkCache.TryResolve<IBiomeGetter>(FormKey.Factory($"{biomeId:x6}:{lmod.ModKey.FileName}"), out var formLink))
                {
                    BiomesList.Add(new BiomDataList(
                        formLink,
                        biomeId,
                        GetDefaultUniqueBiomeColor(biomeId),
                        BiomDataList.DataTypes.Biome
                    ));
                    found = true;
                    break;
                }
            }

            // Add dummy if not found
            if (!found)
            {
                BiomesList.Add(new(
                        null,
                        biomeId,
                        GetDefaultUniqueBiomeColor(biomeId),
                        BiomDataList.DataTypes.Biome
                    ));
            }
        }
    }

    public System.Drawing.Color GetDefaultUniqueBiomeColor(uint biomeId)
    {
        Color color;
        int idx = Array.IndexOf(BiomData.biomStruct.BiomeIds, biomeId);
        color = System.Drawing.Color.FromArgb(255, DefaultPalette[idx][0], DefaultPalette[idx][1], DefaultPalette[idx][2]);

        return color;
    }

    public System.Drawing.Color GetDefaultUniqueResourceColor(uint resId)
    {
        Color color = default;
        int idx = Array.IndexOf(Biom.known_resource_ids, resId);

        if (idx == -1)
        {
            foreach (var colList in DefaultPalette)
            {
                color = System.Drawing.Color.FromArgb(255, colList[0], colList[1], colList[2]);
                if (ResourcesList.Select(x => x.Color).ToList().Contains(color))
                {
                    continue;
                }
                break;
            }
        } else
        {
            color = System.Drawing.Color.FromArgb(255, DefaultPalette[idx][0], DefaultPalette[idx][1], DefaultPalette[idx][2]);
        }

        return color;
    }

    /*
     * Populates UI resource list based on the
     * biom struct of the object.
     */
    public void AddResourceData()
    {
        if (BiomData == null) return;

        List<byte> resourceIdsList = ResourcesList.Select(x => (byte)x.Data).ToList();

        // Get all unique resource ids
        for (int i = 0; i < BiomData.biomStruct.GridFlatSize; i++)
        {
            byte resourceIdN = BiomData.biomStruct.ResrcGridN[i];
            byte resourceIdS = BiomData.biomStruct.ResrcGridS[i];

            if (!resourceIdsList.Contains(resourceIdN))
            {
                resourceIdsList.Add(resourceIdN);

                var dataColor = GetDefaultUniqueResourceColor(resourceIdN);
                var color = System.Drawing.Color.FromArgb(
                    255,
                    dataColor.R,
                    dataColor.G,
                    dataColor.B);

                ResourcesList.Add(new BiomDataList(
                    null,
                    resourceIdN,
                    color,
                    BiomDataList.DataTypes.Resource
                ));
            }

            if (!resourceIdsList.Contains(resourceIdS))
            {
                resourceIdsList.Add(resourceIdS);

                var dataColor = GetDefaultUniqueResourceColor(resourceIdS);
                var color = System.Drawing.Color.FromArgb(
                    255,
                    dataColor.R,
                    dataColor.G,
                    dataColor.B);

                ResourcesList.Add(new BiomDataList(
                    null,
                    resourceIdS,
                    color,
                    BiomDataList.DataTypes.Resource
                ));
            }
        }
    }
}

