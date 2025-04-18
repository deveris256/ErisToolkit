using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using ErisToolkit.Common.GameData;
using ErisToolkit.Common;

namespace ErisToolkit.Planet.ViewModels;

public partial class BiomImage : ObservableObject
{
    [ObservableProperty]
    private string _Name;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    System.Drawing.Bitmap _bitmapData;

    [ObservableProperty]
    WriteableBitmap _image;

    [ObservableProperty]
    string _dataType;

    [ObservableProperty]
    private Biom.BiomDataSide _Side;

    public BiomImage(string dataType, string name, System.Drawing.Bitmap image, Biom.BiomDataSide side)
    {
        Name = name;
        BitmapData = image;
        Image = Utils.ConvertToAvaloniaBitmap(image);
        IsSelected = false;
        DataType = dataType;
        _Side = side;
    }
}
