using CommunityToolkit.Mvvm.ComponentModel;
using Mutagen.Bethesda.Starfield;
using System;
using System.ComponentModel;

namespace ErisToolkit.Planet.ViewModels;

public partial class StarInfo : ObservableObject, INotifyPropertyChanged
{
    private bool customEditorID = false;
    private bool editable = false;

    [ObservableProperty]
    private string _editorID;

    private string? _name;
    private string? Name
    {
        get => _name;
        set {
            _name = value;

            if (customEditorID)
            {
                EditorID = $"{_name?.Replace(" ", "")}Star";
                OnSpecialPropertyChanged(nameof(EditorID));
            }

            OnSpecialPropertyChanged(nameof(Name));
            
        }
    }

    [ObservableProperty]
    private StarProp<int?> _StarID;

    [ObservableProperty]
    private CoordsXYZ _systemParsecLocation;

    [ObservableProperty]
    private StarProp<string> _SunPreset;

    [ObservableProperty]
    public StarProp<string?> _StarClass;

    // Orbited Data Component
    [ObservableProperty]
    public OrbitedDataInfo? _StarOrbitedDataComponent;

    // Star Data Component
    [ObservableProperty]
    public pStarDataComponent? _StarDataComponent;

    // Planet Model Component
    [ObservableProperty]
    public PlanetModelInfo? _StarModelComponent;

    public event PropertyChangedEventHandler? SpecialPropertyChanged;

    public StarInfo(IStarGetter star) {
        LoadStarsystemData(star);
    }
    public StarInfo(Star star)
    {
        LoadStarsystemData(star);
        editable = true;
    }

    protected virtual void OnSpecialPropertyChanged(string propertyName)
    {
        SpecialPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void LoadStarsystemData<T>(T star) where T : IStarGetter
    {
        Name = star.Name;
        EditorID = star.EditorID ?? "";
        StarID = new StarProp<int?>((int?)star.ID);

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

        customEditorID = true;
    }
}
