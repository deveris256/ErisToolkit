using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Mutagen.Bethesda.Starfield;

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

    [ObservableProperty]
    private DataTypes _dataType;

    public enum DataTypes
    {
        None,
        Biome,
        Resource
    }

    public BiomDataList(IBiomeGetter biomeForm, Avalonia.Media.SolidColorBrush col)
    {
        RawID = biomeForm.FormKey.ID; //TODO: Check the ID, it should be 6 in length
        ID = $"{RawID:x6}";

        EditorID = biomeForm.EditorID ?? "UNKNOWN";
        ModName = biomeForm.FormKey.ModKey.Name;

        ButtonColor = col;

        DataType = DataTypes.Biome;
    }

    public BiomDataList(uint rawID, Avalonia.Media.SolidColorBrush col, DataTypes dataType = DataTypes.Biome)
    {
        RawID = rawID;
        ID = $"{RawID:x6}";

        EditorID = "UNKNOWN";
        ModName = "UNKNOWN";

        ButtonColor = col;

        DataType = dataType;
    }

    // Resources
    public BiomDataList(byte rawID, Avalonia.Media.SolidColorBrush col, DataTypes dataType = DataTypes.Resource)
    {
        RawID = rawID;
        ID = $"{RawID:x6}";

        EditorID = "UNKNOWN";
        ModName = "UNKNOWN";

        ButtonColor = col;

        DataType = dataType;
    }
}
