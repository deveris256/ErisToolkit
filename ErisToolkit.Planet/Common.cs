using ErisToolkit.Common.GameData;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Starfield;
using Noggog;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Mutagen.Bethesda.Plugins.Cache.Internals.Implementations;
using Mutagen.Bethesda.Installs;
using System.Linq;
using ErisToolkit.Planet.ViewModels;

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
    public static IGameEnvironment<IStarfieldMod, IStarfieldModGetter>? env;
    public static List<IKeywordGetter> starClasses = new();

    public static GroupMask Mask = new GroupMask()
    {
        Planets = true,
        Stars = true,
        SunPresets = true,
        Keywords = true
    };

    public static IStarfieldMod? currentMod {
        get
        {
            var editableMods = LoadOrder.LoadOrder.Where(x => x.editable == true);
            if (editableMods.Any()) { return (IStarfieldMod?)editableMods.First().LoadOrderMod; }
            return null;
        }
    }

    public static ImmutableLoadOrderLinkCache<IStarfieldMod, IStarfieldModGetter>? linkCache { get; private set; }

    public static StarfieldLoadOrder LoadOrder = new();

    private static BiomInfo? _biomInfo;
    public static event Action? BiomInfoChanged;

    public static int currentEditableIndex = 0;

    public static ObservableCollection<BiomDataList> biomesList = new();
    public static ObservableCollection<BiomDataList> resourcesList = new();
    public static ObservableCollection<string> starList = new();
    public static List<bool> starListIsEditable = new();

    public static BiomPalette? palette;

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

    public static void UpdateData()
    {
        if (currentMod == null)
        {
            env = GameEnvironment.Typical.Builder<IStarfieldMod, IStarfieldModGetter>(GameRelease.Starfield)
                .WithTargetDataFolder(GameLocations.GetDataFolder(GameRelease.Starfield))
                .WithLoadOrder(LoadOrder.GetLoadOrder())
                .Build();
        } else
        {
            env = GameEnvironment.Typical.Builder<IStarfieldMod, IStarfieldModGetter>(GameRelease.Starfield)
                .WithTargetDataFolder(GameLocations.GetDataFolder(GameRelease.Starfield))
                .WithLoadOrder(LoadOrder.GetLoadOrder())
                .WithOutputMod(currentMod)
                .Build();
        }
        
        linkCache = env.LoadOrder.ToImmutableLinkCache();

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

        foreach (var modLo in LoadOrder.LoadOrder)
        {
            var loadOrderMod = modLo.LoadOrderMod;
            
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

    public static StarInfo? GetStar(int index)
    {
        if (starListIsEditable[index])
        {
            var star = currentMod.Stars.FirstOrDefault(x => x.EditorID == starList[index]);
            if (star != null) { return new StarInfo(star); }
        }
        else
        {
            if (linkCache.TryResolve<IStarGetter>(starList[index], out var star))
            {
                return new StarInfo(star);
            }
        }
        return null;
    }
}
