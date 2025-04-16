using CommunityToolkit.Mvvm.ComponentModel;
using Mutagen.Bethesda.Starfield;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ErisToolkit.Planet.ViewModels;
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
