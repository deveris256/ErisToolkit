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

    public static IStarfieldMod? mod { get; private set; }

    public static void SetMod(string pluginPath, TopLevel topLevel)
    {
        IStarfieldMod eMod;

        if (Utils.ForbiddenModNames.Contains(Path.GetFileName(pluginPath))) { return; }

        var masterRefs = MasterReferenceCollection.FromPath(pluginPath, GameRelease.Starfield).Masters;

        HandleModMasters(masterRefs, topLevel);
        try
        {
            eMod = Utils.LoadModEditable(pluginPath, GetLoadOrder());
        } catch { return; }

        AddModToLoadOrder(eMod, topLevel, true);

        mod = eMod;
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
            lo.Add((dynamic)lomod.LoadOrderMod);
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

    public static void AddModToLoadOrder<T>(T mod, TopLevel topLevel, bool editable = false) where T : IStarfieldModGetter
    {
        var loadOrderMod = new EditableLoadOrderMod(mod) { editable = editable } ;
        var masterRefsRaw = mod.ModHeader?.MasterReferences;

        List<dynamic> modsToAdd = new();

        foreach (var lomod in loadOrder)
        {
            if (lomod.LoadOrderModFileName == loadOrderMod.LoadOrderModFileName)
            {
                return;
            }
        }

        if (masterRefsRaw != null && masterRefsRaw.Count != 0)
        {
            var mastersToLoad = HandleModMasters(masterRefsRaw, topLevel);

            foreach (var mastMod in mastersToLoad) { modsToAdd.Add(mastMod); }
        }

        modsToAdd.Add(loadOrderMod);
        
        foreach (var lomod in modsToAdd) { loadOrder.Add(lomod); }
        UpdateData();
    }

    private static List<IStarfieldModDisposableGetter> HandleModMasters<T>(IReadOnlyList<T> masterRefsRaw, TopLevel topLevel) where T : IMasterReferenceGetter
    {
        var modNames = new List<string>();

        foreach (var lomod in loadOrder)
        {
            modNames.Add(lomod.LoadOrderModFileName);
        }

        List<IStarfieldModDisposableGetter> masterMods = new();

        int masterCount = 0;

        foreach (var masterRef in masterRefsRaw)
        {
            if (modNames.Contains(masterRef.Master.FileName))
            {
                masterCount += 1;
            }
        }

        if (masterCount == masterRefsRaw.Count)
        {
            return new();
        }

        foreach (var masterRef in masterRefsRaw)
        {
            if (modNames.Contains(masterRef.Master.FileName))
            {
                continue;
            }

            var files = topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = $"Please open {masterRef.Master.FileName}",
                AllowMultiple = false,
                FileTypeFilter = new[] {
                        new FilePickerFileType("Plugin file")
                        {
                            Patterns = new[] { masterRef.Master.FileName.ToString() }
                        }
                    }
            });

            if (files.Result.Count > 0 && files.Result[0] != null)
            {
                string filePath = Uri.UnescapeDataString(files.Result[0].Path.AbsolutePath);

                if (Uri.UnescapeDataString(files.Result[0].Name) != masterRef.Master.FileName)
                {
                    return new();
                }

                var masterMod = Utils.LoadModReadOnly(filePath);

                if (masterMod != null)
                {
                    masterMods.Add(masterMod);
                    AddModToLoadOrder(masterMod, topLevel);
                }
                else { return new(); }
            }
            else { return new(); }
        }

        return masterMods;
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
        if (mod == null)
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
                .WithOutputMod(mod)
                .Build();
        }
       
        linkCache = env.LoadOrder.ToImmutableLinkCache();

        AddBiomeData();
        AddResourceData();
        PopulateStarList();
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

public partial class CoordsXYZ : ObservableObject
{
    [ObservableProperty]
    double? _X;

    [ObservableProperty]
    double? _Y;

    [ObservableProperty]
    double? _Z;

    [ObservableProperty]
    string _value;

    [ObservableProperty]
    string _name;

    public CoordsXYZ(double? x, double? y, double? z, string name)
    {
        X = x;
        Y = y;
        Z = z;

        Value = "";
        Name = name;
    }

    public string GetValue() { return ""; }
    public override string ToString() { return ""; }
}

public class StarProp<T> : INotifyPropertyChanged
{
    private T? _value;
    public object? HiddenValue;

    private string Value
    {
        get { return _value == null ? "NULL" : _value.ToString(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public StarProp(T? value, object? hiddenValue = null)
    {
        SetValue(value);
        HiddenValue = hiddenValue;
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void SetValue(T? value)
    {
        _value = value;
        OnPropertyChanged(nameof(value));
    }

    public string GetValue() { return Value ?? ""; }

    public override string ToString() { return Value.ToString(); }
}

public partial class StarInfo : ObservableObject
{
    [ObservableProperty]
    private StarProp<string> _Name;

    [ObservableProperty]
    private StarProp<int?> _ID;

    [ObservableProperty]
    private CoordsXYZ _systemParsecLocation;

    [ObservableProperty]
    private StarProp<string> _SunPreset;

    public StarInfo(IStarGetter star) { LoadStarsystemData<IStarGetter>(star); }
    public StarInfo(Star star) { LoadStarsystemData<Star>(star); }

    public void LoadStarsystemData<T>(T star) where T : IStarGetter
    {
        Name = new StarProp<string>(star.EditorID);
        ID = new StarProp<int?>((int?)star.ID);

        SystemParsecLocation = new CoordsXYZ(
            star.BNAM == null ? null : star.BNAM.Value.X,
            star.BNAM == null ? null : star.BNAM.Value.Y,
            star.BNAM == null ? null : star.BNAM.Value.Z,
            "Parsec Location");

        if (Common.linkCache.TryResolve<ISunPresetGetter>(star.SunPreset.FormKey, out var sunPreset))
        {
            SunPreset = new StarProp<string>(sunPreset.EditorID?.ToString(), star.SunPreset);
        }
        else
        {
            SunPreset = new StarProp<string>(star.SunPreset.FormKey.ToString(), star.SunPreset);
        }
    }

    public override string ToString()
    {
        return "";
    }
}

public partial class StarsystemView : ObservableObject
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private object _value;

    public ObservableCollection<StarsystemView> Children { get; } = new();

    public StarsystemView(string name, object value, int index, bool isRootNode = true)
    {
        Name = name;
        Value = value ?? "";

        if (isRootNode)
        {
            if (Common.starListIsEditable[index])
            {
                var star = Common.mod.Stars.FirstOrDefault(x => x.EditorID == Common.starList[index]);
                if (star != null) { Value = new StarInfo(star); }
            }
            else
            {
                if (Common.linkCache.TryResolve<IStarGetter>(Common.starList[index], out var star))
                {
                    Value = new StarInfo(star);
                }
            }
        }

        GenerateProperties();
    }

    private void GenerateProperties()
    {
        try
        {
            if (Value.GetType().IsPrimitive || Value is string)
                return;

            foreach (var prop in Value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.GetIndexParameters().Length > 0 || !prop.CanRead)
                    continue;

                if (prop.Name == "Name" || prop.Name == "Value")
                {
                    continue;
                }

                var propValue = prop.GetValue(Value) ?? "";

                var childNode = new StarsystemView(
                    name: prop.Name,
                    value: propValue,
                    index: -1,
                    isRootNode: false
                );

                Children.Add(childNode);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating properties: {ex}");
        }
    }
}

public partial class EditableLoadOrderMod : ObservableObject
{
    [ObservableProperty]
    private object _loadOrderMod;
    
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
