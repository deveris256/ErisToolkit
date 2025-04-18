using CommunityToolkit.Mvvm.ComponentModel;
using Mutagen.Bethesda.Starfield;

namespace ErisToolkit.Planet.ViewModels;

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
