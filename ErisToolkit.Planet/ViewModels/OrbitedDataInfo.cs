using CommunityToolkit.Mvvm.ComponentModel;
using Mutagen.Bethesda.Starfield;

namespace ErisToolkit.Planet.ViewModels;

public partial class OrbitedDataInfo : ObservableObject
{
    [ObservableProperty]
    public StarProp<ulong?> _GravityWell;
    [ObservableProperty]
    public StarProp<float?> _SurfaceGravity;
    [ObservableProperty]
    public StarProp<float?> _MassInSM;
    [ObservableProperty]
    public StarProp<float?> _RadiusInKM;

    public OrbitedDataInfo(IOrbitedDataComponentGetter orbitedComp)
    {
        GravityWell = new(orbitedComp.Unknown1, null);
        SurfaceGravity = new(orbitedComp.Unknown2, null);
        RadiusInKM = new(orbitedComp.RadiusInKm, null);
        MassInSM = new(orbitedComp.MassInSm, null);
    }
}
