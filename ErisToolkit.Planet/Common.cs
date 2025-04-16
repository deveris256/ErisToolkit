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
using DynamicData;
using Avalonia.Controls.Models.TreeDataGrid;
using System.Linq;
using System.Formats.Asn1;
using System.Reflection;
using System.Collections.Specialized;
using System.ComponentModel;
using ICSharpCode.SharpZipLib;
using System.IO;
using Mutagen.Bethesda.Plugins.Masters;

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

    public static List<IKeywordGetter> starClasses = new();

    public static IStarfieldMod? currentMod {
        get
        {
            var editableMods = loadOrder.Where(x => x.editable == true);
            if (editableMods.Any()) { return (IStarfieldMod?)editableMods.First().LoadOrderMod; }
            return null;
        }
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
            AddBiomeData();
            AddResourceData();
        }
    }

    public static int currentEditableIndex = 0;

    public static ObservableCollection<BiomDataList> biomesList = new();
    public static ObservableCollection<BiomDataList> resourcesList = new();
    public static ObservableCollection<string> starList = new();
    public static List<bool> starListIsEditable = new();

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

        foreach (var lomod in Common.loadOrder)
        {
            lo.Add((IStarfieldModGetter)lomod.LoadOrderMod);
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
                if (loMod.editable)
                {
                    var lmod = (IStarfieldMod)loMod.LoadOrderMod;

                    if (linkCache != null &&
                    linkCache.TryResolve<IBiomeGetter>(FormKey.Factory($"{biomeId:x6}:{lmod.ModKey.FileName}"), out var formLink))
                    {
                        biomesList.Add(new BiomDataList(
                            biomeId,
                            formLink.EditorID,
                            color
                        ));
                        processed = true;
                        break;
                    }
                } else
                {
                    var lmod = (IStarfieldModDisposableGetter)loMod.LoadOrderMod;

                    if (linkCache != null &&
                    linkCache.TryResolve<IBiomeGetter>(FormKey.Factory($"{biomeId:x6}:{lmod.ModKey.FileName}"), out var formLink))
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

    public static void AddModToLoadOrder(string modPath, TopLevel topLevel, bool editable)
    {
        IStarfieldModGetter mod;
        List<IStarfieldModGetter> masters;
        
        // Check for mod name and determine if it can't be editable
        if (editable &&
            Utils.ForbiddenModNames.Contains(Path.GetFileName(modPath))) { return; }

        masters = HandleModMasters(MasterReferenceCollection.FromPath(modPath, GameRelease.Starfield).Masters, topLevel);

        // Early check if masters are there...
        List<string> masterNames = masters.Select(x => x.ModKey.FileName.ToString()).ToList();
        List<string> loModsNames = loadOrder.Select(x => x.LoadOrderMod.ModKey.FileName.ToString()).ToList();

        foreach (var master in MasterReferenceCollection.FromPath(modPath, GameRelease.Starfield).Masters)
        {
            if (!masterNames.Contains(master.Master.FileName) && !loModsNames.Contains(master.Master.FileName))
            { return; }
        }

        // Place masters in LO
        foreach (var master in masters)
        {
            if (loModsNames.Contains(master.ModKey.FileName))
            { continue; }

            loadOrder.Add(new EditableLoadOrderMod(master));
        }

        // Set (temporary) mod variable
        var currentLoadOrder = GetLoadOrder();
        if (editable) { mod = Utils.LoadModEditable(modPath, currentLoadOrder); }
        else { mod = Utils.LoadModReadOnly(modPath); }

        // Check if mod hasn't been loaded
        if (mod == null) { return; }

        // Load mod
        var loadOrderMod = new EditableLoadOrderMod(mod) { editable = editable };
        loadOrder.Add(loadOrderMod);

        UpdateData();
    }

    private static List<IStarfieldModGetter> HandleModMasters<T>(IReadOnlyList<T> masterRefsRaw, TopLevel topLevel) where T : IMasterReferenceGetter
    {
        List<IStarfieldModGetter> masterMods = new();
        List<string> existingModNames = loadOrder.Select(x => x.LoadOrderModFileName).ToList();

        foreach (var master in masterRefsRaw)
        {
            if (existingModNames.Contains(master.Master.FileName)) { continue; }

            var files = topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = $"Please open {master.Master.FileName}",
                AllowMultiple = false,
                FileTypeFilter = new[] {
                        new FilePickerFileType("Plugin file")
                        {
                            Patterns = new[] { master.Master.FileName.ToString() }
                        }
                    }
            });

            if (files.Result.Count > 0 && files.Result[0] != null)
            {
                string filePath = Uri.UnescapeDataString(files.Result[0].Path.AbsolutePath);
                masterMods.Add(Utils.LoadModReadOnly(filePath));
            }
            else { return new(); }
        }

        return masterMods;
    }

    public static void RemoveModFromLoadOrder(EditableLoadOrderMod mod)
    {
        // At first, handle the masters..
        List<string> masterNames = new();

        foreach (var loMod in loadOrder)
        {
            foreach (var mast in loMod.LoadOrderMod.MasterReferences)
            {
                masterNames.Add(mast.Master.FileName);
            }
        }

        // Can't remove the mod that's a master of another mod
        if (masterNames.Contains(mod.LoadOrderModFileName)) { return; }

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
        if (currentMod == null)
        {
            env = GameEnvironment.Typical.Builder<IStarfieldMod, IStarfieldModGetter>(GameRelease.Starfield)
                .WithTargetDataFolder(GameLocations.GetDataFolder(GameRelease.Starfield))
                .WithLoadOrder(GetLoadOrder())
                .Build();
        } else
        {
            env = GameEnvironment.Typical.Builder<IStarfieldMod, IStarfieldModGetter>(GameRelease.Starfield)
                .WithTargetDataFolder(GameLocations.GetDataFolder(GameRelease.Starfield))
                .WithLoadOrder(GetLoadOrder())
                .WithOutputMod(currentMod)
                .Build();
        }
        
        linkCache = env.LoadOrder.ToImmutableLinkCache();

        AddBiomeData();
        AddResourceData();
        PopulateStarList();

        ResolveStarClasses();
    }

    public static void ResolveStarClasses()
    {
        starClasses.Clear();

        foreach (var starClass in KnownGameData.StarTypes)
        {
            if (linkCache.TryResolve<IKeywordGetter>(starClass, out var sc))
            {
                starClasses.Add(sc);
            }
        }
    }

    public static void PopulateStarList()
    {
        starList.Clear();
        starListIsEditable.Clear();

        foreach (var modLo in loadOrder)
        {
            if (modLo.editable) {
                var loadOrderMod = (IStarfieldMod)modLo.LoadOrderMod;

                foreach (var star in loadOrderMod.Stars)
                {
                    if (star.EditorID != null)
                    {
                        starList.Add(star.EditorID);

                        starListIsEditable.Add(modLo.editable);
                    }
                }
            }
            else {
                var loadOrderMod = (IStarfieldModDisposableGetter)modLo.LoadOrderMod;

                foreach (var star in loadOrderMod.Stars)
                {
                    if (star.EditorID != null)
                    {
                        starList.Add(star.EditorID);

                        starListIsEditable.Add(modLo.editable);
                    }
                }
            }
        }
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
    IStarfieldModGetter _loadOrderMod;
    
    [ObservableProperty]
    private string _loadOrderModName;

    [ObservableProperty]
    private string _loadOrderModFileName;

    public bool editable;
    
    public EditableLoadOrderMod(IStarfieldModGetter mod)
    {
        LoadOrderMod = mod;
        LoadOrderModFileName = mod.ModKey.FileName;
        LoadOrderModName = $"Click to remove {LoadOrderModFileName}";
        editable = false;
    }

    public void RemoveModFromLoadOrder()
    {
        Common.RemoveModFromLoadOrder(this);
    }
}
