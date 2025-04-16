using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using Mutagen.Bethesda.Starfield;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ErisToolkit.Planet.ViewModels;

public partial class pPlanetModelComponentSubItem : ObservableObject
{
    [ObservableProperty]
    string _name;
    [ObservableProperty]
    string _value;

    IPlanetModelComponentXMPMSubItemGetter SubItem;

    [ObservableProperty]
    StarProp<string?> _ResourceID;
    [ObservableProperty]
    StarProp<string?> _File;
    [ObservableProperty]
    StarProp<string?> _Extension;
    [ObservableProperty]
    StarProp<string?> _Folder;

    public pPlanetModelComponentSubItem(IPlanetModelComponentXMPMSubItemGetter subItem)
    {
        SubItem = subItem;
        ResourceID = new(subItem.ResourceID);
        File = new(BitConverter.ToString(BitConverter.GetBytes(subItem.FileHash)).Replace("-", "").ToLower());
        Extension = new(subItem.Extension);
        Folder = new(BitConverter.ToString(BitConverter.GetBytes(subItem.FolderHash)).Replace("-", "").ToLower());
    }

    public string GetValue() { return ""; }
    public override string ToString() { return ""; }
}

public partial class pPlanetModelComponent : ObservableObject
{
    [ObservableProperty]
    string _name;
    [ObservableProperty]
    string _value;

    [ObservableProperty]
    public StarProp<string?> _StarModel;
    [ObservableProperty]
    public ObservableCollection<pPlanetModelComponentSubItem> _SubItems;
    [ObservableProperty]
    public ObservableCollection<string> _StrSubItems;

    public pPlanetModelComponent(IPlanetModelComponentGetter planetModelComp)
    {
        SubItems = new();
        StrSubItems = new();

        StarModel = new(planetModelComp.Model?.File?.ToString() ?? "");
        
        if (planetModelComp.XMPM != null)
        {
            if (planetModelComp.XMPM.UnknownSubItems != null)
            {
                foreach (var sub in planetModelComp.XMPM.UnknownSubItems)
                {
                    SubItems.Add(new(sub));
                }
            }
        }
        
    }
}

public partial class pOrbitedDataComponent : ObservableObject
{
    [ObservableProperty]
    string _name;
    [ObservableProperty]
    string _value;

    [ObservableProperty]
    public StarProp<ulong?> _GravityWell;
    [ObservableProperty]
    public StarProp<float?> _SurfaceGravity;
    [ObservableProperty]
    public StarProp<float?> _MassInSM;
    [ObservableProperty]
    public StarProp<float?> _RadiusInKM;

    public pOrbitedDataComponent(IOrbitedDataComponentGetter orbitedComp)
    {
        GravityWell = new(orbitedComp.Unknown1, null);
        SurfaceGravity = new(orbitedComp.Unknown2, null);
        RadiusInKM = new(orbitedComp.RadiusInKm, null);
        MassInSM = new(orbitedComp.MassInSm, null);

        Name = "";
        Value = "";
    }

    public string GetValue() { return ""; }
    public override string ToString() { return ""; }
}

public partial class pStarDataComponent : ObservableObject
{
    [ObservableProperty]
    string _name;
    [ObservableProperty]
    string _value;

    [ObservableProperty]
    public StarProp<string?> _CatalogueId;
    [ObservableProperty]
    public StarProp<string?> _SpectralClass;
    [ObservableProperty]
    public StarProp<float?> _Magnitude;
    [ObservableProperty]
    public StarProp<float?> _InnerHabitableZone;
    [ObservableProperty]
    public StarProp<float?> _OuterHabitableZone;
    [ObservableProperty]
    public StarProp<uint?> _HIP;
    [ObservableProperty]
    public StarProp<uint?> _Radius;
    [ObservableProperty]
    public StarProp<uint?> _TemperatureInK;

    public pStarDataComponent(IStarDataComponentGetter stardataComp)
    {
        CatalogueId = new(stardataComp.CatalogueId, null);
        SpectralClass = new(stardataComp.SpectralClass, null);
        Magnitude = new(stardataComp.Magnitude, null);
        InnerHabitableZone = new(stardataComp.InnerHabitableZone, null);
        OuterHabitableZone = new(stardataComp.OuterHabitableZone, null);
        HIP = new(stardataComp.HIP, null);
        Radius = new(stardataComp.Radius, null);
        TemperatureInK = new(stardataComp.TemperatureInK, null);

        Name = "";
        Value = "";
    }

    public string GetValue() { return ""; }
    public override string ToString() { return ""; }
}

public partial class StarInfo : ObservableObject
{
    IStarfieldMod? mod; // If it's editable
    IStarGetter star;

    [ObservableProperty]
    private StarProp<string?> _EditorID;

    [ObservableProperty]
    private StarProp<string?> _Name;

    [ObservableProperty]
    private StarProp<int?> _ID;

    [ObservableProperty]
    private CoordsXYZ _systemParsecLocation;

    [ObservableProperty]
    private StarProp<string> _SunPreset;

    [ObservableProperty]
    public StarProp<string?> _StarClass;

    // Orbited Data Component
    [ObservableProperty]
    public pOrbitedDataComponent? _StarOrbitedDataComponent;

    // Star Data Component
    [ObservableProperty]
    public pStarDataComponent? _StarDataComponent;

    // Planet Model Component
    [ObservableProperty]
    public pPlanetModelComponent? _StarModelComponent;

    public StarInfo(IStarGetter star, IStarfieldMod? starMod = null) {
        LoadStarsystemData(star);
        mod = starMod;
    }
    public StarInfo(Star star, IStarfieldMod? starMod = null)
    {
        LoadStarsystemData(star);
        mod = starMod;
    }

    public void LoadStarsystemData<T>(T star) where T : IStarGetter
    {
        this.star = star;

        Name = new StarProp<string?>(star.Name);
        EditorID = new StarProp<string?>(star.EditorID);
        ID = new StarProp<int?>((int?)star.ID);

        SystemParsecLocation = new CoordsXYZ(
            star.BNAM == null ? null : star.BNAM.Value.X,
            star.BNAM == null ? null : star.BNAM.Value.Y,
            star.BNAM == null ? null : star.BNAM.Value.Z,
            "Parsec Location");

        if (Common.linkCache.TryResolve<ISunPresetGetter>(star.SunPreset.FormKey, out var sunPreset))
        {
            SunPreset = new StarProp<string>(sunPreset.EditorID?.ToString(), star.SunPreset.FormKey);
        }
        else
        {
            SunPreset = new StarProp<string>(star.SunPreset.FormKey.ToString(), star.SunPreset.FormKey);
        }

        if (star.Keywords != null)
        {
            foreach (var kw in star.Keywords)
            {
                if (Common.linkCache.TryResolve<IKeywordGetter>(kw.FormKey, out var keyword))
                {
                    if (Common.starClasses.Contains(keyword))
                    {
                        StarClass = new StarProp<string?>(keyword.EditorID, keyword.FormKey);
                    }
                }
            }
        } else
        {
            StarClass = new StarProp<string?>("UNSET", null);
        }

        foreach (var component in star.Components)
        {
            if (component is IOrbitedDataComponentGetter orbitedComp)
            {
                StarOrbitedDataComponent = new(orbitedComp);
            } else if (component is IStarDataComponentGetter stardataComp)
            {
                StarDataComponent = new(stardataComp);
            } else if (component is IPlanetModelComponentGetter planetModelComp)
            {
                StarModelComponent = new(planetModelComp);
                //planetModelComp.XMPM.UnknownSubItems
            }
        }
    }

    public override string ToString()
    {
        return "";
    }
}
