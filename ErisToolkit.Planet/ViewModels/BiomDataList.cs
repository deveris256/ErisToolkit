using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Mutagen.Bethesda.Starfield;
using System.Drawing;

namespace ErisToolkit.Planet.ViewModels;

public partial class BiomDataList : ObservableObject
{
    public object? RawID { get; set; }

    [ObservableProperty]
    private string _iD;

    [ObservableProperty]
    private string _modName;

    [ObservableProperty]
    private string _editorID;

    [ObservableProperty]
    private System.Drawing.Color _Color;

    [ObservableProperty]
    private IBrush _Brush;

    [ObservableProperty]
    private DataTypes _dataType;

    public dynamic Data;

    public enum DataTypes
    {
        None,
        Biome,
        Resource
    }

    public BiomDataList(IBiomeGetter? biomeForm, dynamic data, System.Drawing.Color col, DataTypes dataType)
    {
        Data = data;

        if (biomeForm != null) {
            EditorID = biomeForm.EditorID ?? "";
            ModName = biomeForm.FormKey.ModKey.Name;
            ID = $"{biomeForm.FormKey.ID:x6}";
        } else
        {
            EditorID = "UNKNOWN";
            ModName = "UNKNOWN";
            if (dataType == DataTypes.Biome)
            {
                ID = $"{(uint)data:x6}";
            }
            else
            {
                ID = data.ToString();
            }
        }

        Color = col;
        Brush = new Avalonia.Media.SolidColorBrush(new Avalonia.Media.Color(255, col.R, col.G, col.B));

        DataType = dataType;
    }
}
