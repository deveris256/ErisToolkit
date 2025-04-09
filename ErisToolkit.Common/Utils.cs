using Mutagen.Bethesda.Starfield;
using System.Drawing.Imaging;
using System.Drawing;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ErisToolkit.Common;

/*
 * ErisToolkit by Deveris256
 * 
 * This file contains common utilities used by the
 * programs in ErisToolkit.
 * 
 */


/*
 * A common class containing useful neat functions.
 */
public static class Utils
{
    // File pickers
    public static FilePickerFileType BiomFilePicker { get; } = new(".Biom file")
    {
        Patterns = new[] { "*.biom" }
    };

    public static FilePickerFileType PngFilePicker { get; } = new(".Png file")
    {
        Patterns = new[] { "*.png" }
    };

    public static FilePickerFileType PluginFilePicker { get; } = new("Plugin file")
    {
        Patterns = new[] { "*.esp", "*.esm", "*.*" }
    };

    /*
     * TODO:
     * - Load with specified masters
     */
    public static IStarfieldModDisposableGetter? LoadMod(string pluginFile)
    {
        {
            try
            { 
            IStarfieldModDisposableGetter mod = StarfieldMod.Create(StarfieldRelease.Starfield)
                                                            .FromPath(pluginFile)
                                                            .WithLoadOrderFromHeaderMasters()
                                                            .WithDefaultDataFolder()
                                                            .Construct();
                return mod;
            } catch (Exception) { return null;  }
            
        }
    }

    // Conversion to Avalonia bitmap
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public static WriteableBitmap? ConvertToAvaloniaBitmap(this Image bitmap)
    {
        if (bitmap == null)
            return null;

        System.Drawing.Bitmap bitmapTmp = new System.Drawing.Bitmap(bitmap);

        var bitmapdata = bitmapTmp.LockBits(
            new Rectangle(0, 0, bitmapTmp.Width, bitmapTmp.Height),
            ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb
        );

        WriteableBitmap bitmap1 = new WriteableBitmap(Avalonia.Platform.PixelFormat.Bgra8888, AlphaFormat.Premul,
            bitmapdata.Scan0,
            new Avalonia.PixelSize(bitmapdata.Width, bitmapdata.Height),
            new Avalonia.Vector(96, 96),
            bitmapdata.Stride
        );

        bitmapTmp.UnlockBits(bitmapdata);
        bitmapTmp.Dispose();
        return bitmap1;
    }
}

/*
 * An image palette data class. Currently, used in
 * (planet_data)<->(bitmap) pipeline.
 * 
 * Each planet data value corresponds to a color at the
 * index of the palette.
 */
public class Palette
{
    public Dictionary<int, List<int>> paletteData = new();

    public Palette(string json)
    {
        var col = JsonConvert.DeserializeObject<Dictionary<string, List<List<int>>>>(json);
        var colors = col["palette"];

        for (int i = 0; i < colors.Count; i++)
        {
            paletteData[i] = [colors[i][0], colors[i][1], colors[i][2]];
        }
    }
}

/*
 * Helper class for Avalonia DataGrid
 */
public partial class BiomDataList : ObservableObject
{
    public string Id {  get; set; }
    public string Name { get; set; }

    [ObservableProperty]
    private IBrush _buttonColor;

    public BiomDataList(string id, string name, Avalonia.Media.SolidColorBrush col)
    {
        Id = id;
        Name = name;
        ButtonColor = col;
    }
}
