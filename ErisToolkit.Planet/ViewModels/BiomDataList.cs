using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Mutagen.Bethesda.Starfield;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErisToolkit.Planet.ViewModels;

public partial class BiomDataList : ObservableObject
{
    public uint RawID { get; set; }

    [ObservableProperty]
    private string _iD;

    [ObservableProperty]
    private string _modName;

    [ObservableProperty]
    private string _editorID;

    [ObservableProperty]
    private IBrush _buttonColor;

    public BiomDataList(IBiomeGetter biomeForm, Avalonia.Media.SolidColorBrush col)
    {
        RawID = biomeForm.FormKey.ID; //TODO: Check the ID, it should be 6 in length
        ID = $"{RawID:x6}";

        EditorID = biomeForm.EditorID ?? "UNKNOWN";
        ModName = biomeForm.FormKey.ModKey.Name;

        ButtonColor = col;
    }

    public BiomDataList(uint rawID, Avalonia.Media.SolidColorBrush col)
    {
        RawID = rawID;
        ID = $"{RawID:x6}";

        EditorID = "UNKNOWN";
        ModName = "UNKNOWN";

        ButtonColor = col;
    }

    // Resources
    public BiomDataList(byte rawID, Avalonia.Media.SolidColorBrush col)
    {
        RawID = rawID;
        ID = $"{RawID:x6}";

        EditorID = "UNKNOWN";
        ModName = "UNKNOWN";

        ButtonColor = col;
    }
}
