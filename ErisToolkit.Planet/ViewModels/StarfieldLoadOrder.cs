using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using ErisToolkit.Common;
using Mutagen.Bethesda.Plugins.Masters;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Starfield;
using Mutagen.Bethesda;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Noggog;

namespace ErisToolkit.Planet.ViewModels;

public partial class StarfieldLoadOrder : ObservableObject
{
    [ObservableProperty]
    ObservableCollection<SingleLoadOrderMod> _loadOrder;

    public StarfieldLoadOrder()
    {
        LoadOrder = new();
    }

    public LoadOrder<IStarfieldModGetter> GetLoadOrder()
    {
        var lo = new LoadOrder<IStarfieldModGetter>();
        LoadOrder.ForEach(x => lo.Add(x.LoadOrderMod));
        return lo;
    }

    public void RemoveModFromLoadOrder(SingleLoadOrderMod mod)
    {
        // At first, handle the masters..
        List<string> masterNames = new();

        foreach (var loMod in LoadOrder)
        {
            foreach (var mast in loMod.LoadOrderMod.MasterReferences)
            {
                masterNames.Add(mast.Master.FileName);
            }
        }

        // Can't remove the mod that's a master of another mod
        if (masterNames.Contains(mod.LoadOrderModFileName)) { return; }

        var index = LoadOrder.IndexOf(mod);
        LoadOrder.Remove(mod);

        try
        {
            Common.UpdateData();
        }
        catch (Exception)
        {
            LoadOrder.Insert(index, mod);
            Common.UpdateData();
        }
    }

    public void AddModToLoadOrder(string modPath, TopLevel topLevel, bool editable)
    {
        IStarfieldModGetter mod;
        List<IStarfieldModGetter> masters;

        // Check for mod name and determine if it can't be editable
        if (editable &&
            Utils.ForbiddenModNames.Contains(Path.GetFileName(modPath))) { return; }

        masters = HandleModMasters(MasterReferenceCollection.FromPath(modPath, GameRelease.Starfield).Masters, topLevel);

        // Early check if masters are there...
        List<string> masterNames = masters.Select(x => x.ModKey.FileName.ToString()).ToList();
        List<string> loModsNames = LoadOrder.Select(x => x.LoadOrderMod.ModKey.FileName.ToString()).ToList();

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

            LoadOrder.Add(new SingleLoadOrderMod(master));
        }

        // Set (temporary) mod variable
        var currentLoadOrder = GetLoadOrder();
        if (editable) { mod = Utils.LoadModEditable(modPath, currentLoadOrder); }
        else { mod = Utils.LoadModReadOnly(modPath); }

        // Check if mod hasn't been loaded
        if (mod == null) { return; }

        // Load mod
        var loadOrderMod = new SingleLoadOrderMod(mod) { editable = editable };
        LoadOrder.Add(loadOrderMod);

        Common.UpdateData();
    }

    private List<IStarfieldModGetter> HandleModMasters<T>(IReadOnlyList<T> masterRefsRaw, TopLevel topLevel) where T : IMasterReferenceGetter
    {
        List<IStarfieldModGetter> masterMods = new();
        List<string> existingModNames = LoadOrder.Select(x => x.LoadOrderModFileName).ToList();

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
}
