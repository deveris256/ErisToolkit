using CommunityToolkit.Mvvm.ComponentModel;
using Mutagen.Bethesda.Starfield;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErisToolkit.Planet.ViewModels;

public partial class SingleLoadOrderMod : ObservableObject
{
    [ObservableProperty]
    IStarfieldModGetter _loadOrderMod;

    [ObservableProperty]
    private string _loadOrderModName;

    [ObservableProperty]
    private string _loadOrderModFileName;

    public bool editable;

    public SingleLoadOrderMod(IStarfieldModGetter mod)
    {
        LoadOrderMod = mod;
        LoadOrderModFileName = mod.ModKey.FileName;
        LoadOrderModName = $"Click to remove {LoadOrderModFileName}";
        editable = false;
    }

    public void RemoveModFromLoadOrder()
    {
        Common.LoadOrder.RemoveModFromLoadOrder(this);
    }
}
