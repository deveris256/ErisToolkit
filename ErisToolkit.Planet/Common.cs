using Avalonia.Media.Imaging;
using ErisToolkit.Common;
using ErisToolkit.Common.GameData;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Starfield;
using Noggog;
using System;
using Mutagen.Bethesda.Plugins.Records;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Mutagen.Bethesda.Plugins.Cache.Internals.Implementations;
using Mutagen.Bethesda.Plugins.Order;
using CommunityToolkit.Mvvm.ComponentModel;
using Mutagen.Bethesda.Installs;
using Avalonia.Controls;
using Mutagen.Bethesda.Plugins.Exceptions;
using Avalonia.Platform.Storage;

/* 
 * ErisToolkit by Deveris256
 * 
 * 'Common' class contains static variables used
 * by various aspects of the program, such as
 * images.
 * 
 */

namespace ErisToolkit.Planet;

public static class Common
{
    public static ObservableCollection<EditableLoadOrderMod> loadOrder = new();

    public static IGameEnvironment<IStarfieldMod, IStarfieldModGetter>? env;

    public static IStarfieldModDisposableGetter? mod { get; private set; }

    public static void SetMod(IStarfieldModDisposableGetter plugin, TopLevel topLevel)
    {
        if (plugin == null) { return; }
        if (Utils.ForbiddenModNames.Contains(plugin?.ModKey.FileName)) { return; }
        if (mod == plugin) { return; }

        mod = plugin;
        try { UpdateData(); } catch { }
        AddModToLoadOrder(plugin, topLevel);
    }

    public static ImmutableLoadOrderLinkCache<IStarfieldMod, IStarfieldModGetter>? linkCache { get; private set; }

    private static Biom? _biom;
    public static Biom? biom
    {
        get => _biom;
        set
        {
            if (_biom == value) return;

            _biom = value;
            try { UpdateData(); } catch { }
        }
    }

    public static int currentEditableIndex = 0;

    public static ObservableCollection<BiomDataList> biomesList = new();
    public static ObservableCollection<BiomDataList> resourcesList = new();

    public static Palette? palette;

    public static int StringToIndex(string str)
    {
        switch (str.ToLower())
        {
            case "biomgridn": return 0;
            case "biomgrids": return 1;
            case "resgridn": return 2;
            case "resgrids": return 3;
            default: return -1;
        }
    }

    // Gets bitmap
    public static System.Drawing.Bitmap? GetBitmap(int index = -1)
    {
        if (biom == null) return null;
        if (index == -1) { index = currentEditableIndex; }

        switch (index)
        {
            case 0: return biom.GetBiomeImage(biom.biomStruct.BiomeGridN);
            case 1: return biom.GetBiomeImage(biom.biomStruct.BiomeGridS);
            case 2: return biom.GetResourceImage(biom.biomStruct.ResrcGridN);
            case 3: return biom.GetResourceImage(biom.biomStruct.ResrcGridS);
            default: return null;
        }
    }

    // Gets Avalonia-compatible image
    public static WriteableBitmap? GetImage(int index = -1)
    {
        if (biom == null) return null;
        if (index == -1) { index = currentEditableIndex; }

        switch (index)
        {
            case 0: return Utils.ConvertToAvaloniaBitmap(biom.GetBiomeImage(biom.biomStruct.BiomeGridN));
            case 1: return Utils.ConvertToAvaloniaBitmap(biom.GetBiomeImage(biom.biomStruct.BiomeGridS));
            case 2: return Utils.ConvertToAvaloniaBitmap(biom.GetResourceImage(biom.biomStruct.ResrcGridN));
            case 3: return Utils.ConvertToAvaloniaBitmap(biom.GetResourceImage(biom.biomStruct.ResrcGridS));
            default: return null;
        }
    }

    public static LoadOrder<IStarfieldModGetter> GetLoadOrder()
    {
        var lo = new LoadOrder<IStarfieldModGetter>();

        foreach (var mod in Common.loadOrder)
        {
            lo.Add(mod.LoadOrderMod);
        }

        return lo;
    }

    public static void AddBiomeData()
    {
        biomesList.Clear();

        if (biom == null) return;

        for (int i = 0; i < biom.biomStruct.NumBiomes; i++)
        {
            var color = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(
                    (byte)Biom.palette.paletteData[i][0],
                    (byte)Biom.palette.paletteData[i][1],
                    (byte)Biom.palette.paletteData[i][2]
            ), 1);

            var biomeId = biom.biomStruct.BiomeIds[i];
            bool processed = false;


            foreach (var loMod in loadOrder)
            {
                if (linkCache != null &&
                    linkCache.TryResolve<IBiomeGetter>(FormKey.Factory($"{biomeId:x6}:{loMod.LoadOrderMod.ModKey.FileName}"), out var formLink))
                {
                    biomesList.Add(new BiomDataList(
                        biomeId,
                        formLink.EditorID,
                        color
                    ));
                    processed = true;
                    break;
                }
            }

            if (!processed)
            {
                biomesList.Add(new BiomDataList(
                        biomeId,
                        "???",
                        color
                    ));
            }
        }
    }

    public static void AddModToLoadOrder(IStarfieldModDisposableGetter mod, TopLevel topLevel)
    {
        var loadOrderMod = new EditableLoadOrderMod(mod);

        var masterRefsRaw = mod.ModHeader.MasterReferences;

        List<string> modNames = new();

        foreach (var lomod in loadOrder)
        {
            if (lomod.LoadOrderModName == loadOrderMod.LoadOrderModName)
            {
                return;
            }
            modNames.Add(lomod.LoadOrderMod.ModKey.FileName);
        }

        if (masterRefsRaw.Count == 0)
        {
            loadOrder.Add(loadOrderMod);
            UpdateData();
            return;
        }

        foreach (var masterRef in masterRefsRaw)
        {
            if (!modNames.Contains(masterRef.Master.FileName))
            {
                var files = topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = $"Please open {masterRef.Master.FileName}",
                    AllowMultiple = false,
                    FileTypeFilter = new[] { Utils.PluginFilePicker }
                });

                if (files.Result.Count > 0 && files.Result[0] != null)
                {
                    loadOrder.Add(loadOrderMod);

                    string filePath = Uri.UnescapeDataString(files.Result[0].Path.AbsolutePath);

                    var masterMod = Utils.LoadMod(filePath);

                    if (masterMod != null) { AddModToLoadOrder(masterMod, topLevel); }
                }
                else
                {
                    UpdateData();
                    return;
                }
            }
        }

        UpdateData();
        return;
    }

    public static void RemoveModFromLoadOrder(EditableLoadOrderMod mod)
    {
        var index = loadOrder.IndexOf(mod);
        loadOrder.Remove(mod);

        try
        {
            UpdateData();
        } catch (Exception)
        {
            loadOrder.Insert(index, mod);
            UpdateData();
        }
    }

    public static void UpdateData()
    {
        env = GameEnvironment.Typical.Builder<IStarfieldMod, IStarfieldModGetter>(GameRelease.Starfield)
            .WithTargetDataFolder(GameLocations.GetDataFolder(GameRelease.Starfield))
            .WithLoadOrder(GetLoadOrder())
            .Build();
       
        linkCache = env.LoadOrder.ToImmutableLinkCache();

        AddBiomeData();
        AddResourceData();
    }

    public static void AddResourceData()
    {
        resourcesList.Clear();

        if (biom == null) return;

        List<byte> tempList = new();

        for (int i = 0; i < biom.biomStruct.GridFlatSize; i++)
        {
            byte resourceIdN = (byte)Array.IndexOf(Biom.known_resource_ids, biom.biomStruct.ResrcGridN[i]);
            byte resourceIdS = (byte)Array.IndexOf(Biom.known_resource_ids, biom.biomStruct.ResrcGridS[i]);

            if (!tempList.Contains(resourceIdN))
            {
                tempList.Add(resourceIdN);

                var color = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(
                    (byte)Biom.palette.paletteData[resourceIdN][0],
                    (byte)Biom.palette.paletteData[resourceIdN][1],
                    (byte)Biom.palette.paletteData[resourceIdN][2]
                ), 1);

                resourcesList.Add(new BiomDataList(
                    biom.biomStruct.ResrcGridN[i],
                    "???",
                    color
                ));
            }

            if (!tempList.Contains(resourceIdS))
            {
                tempList.Add(resourceIdS);

                var color = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(
                    (byte)Biom.palette.paletteData[resourceIdS][0],
                    (byte)Biom.palette.paletteData[resourceIdS][1],
                    (byte)Biom.palette.paletteData[resourceIdS][2]
                ), 1);

                resourcesList.Add(new BiomDataList(
                    biom.biomStruct.ResrcGridS[i],
                    "???",
                    color
                ));
            }
        }
    }
}

public partial class EditableLoadOrderMod : ObservableObject
{
    [ObservableProperty]
    private IStarfieldModDisposableGetter _loadOrderMod;
    
    [ObservableProperty]
    private string _loadOrderModName;
    
    public EditableLoadOrderMod(IStarfieldModDisposableGetter mod)
    {
        LoadOrderMod = mod;
        LoadOrderModName = $"Click to remove {mod.ModKey.FileName}";
    }

    public void RemoveModFromLoadOrder()
    {
        Common.RemoveModFromLoadOrder(this);
    }
}
