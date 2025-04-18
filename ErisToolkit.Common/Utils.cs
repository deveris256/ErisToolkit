using Mutagen.Bethesda.Starfield;
using System.Drawing.Imaging;
using System.Drawing;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Order;
using System.Collections.ObjectModel;

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
    public static List<string> ForbiddenModNames = [
        "Starfield.esm",
        "BlueprintShips-Starfield.esm",
        "ShatteredSpace.esm",
        "Constellation.esm",
        "SFBGS003.esm",
        "SFBGS004.esm",
        "SFBGS005.esm",
        "SFBGS006.esm",
        "SFBGS007.esm",
        "SFBGS008.esm"
    ];

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
        Patterns = new[] { "*.esp", "*.esm" }
    };

    /*
     * Loads a read-only mod
     */
    public static IStarfieldModGetter? LoadModReadOnly(string pluginFile)
    {
        try
        { 
            IStarfieldModDisposableGetter mod = StarfieldMod.Create(StarfieldRelease.Starfield)
                                                        .FromPath(pluginFile)
                                                        .WithDefaultLoadOrder()
                                                        .WithDefaultDataFolder()
                                                        .Construct();
            return mod;
        } catch (Exception) { return null;  }
    }

    /*
     * Loads mod as editable
     */
    public static IStarfieldMod LoadModEditable(string pluginFile, LoadOrder<IStarfieldModGetter> loadOrder)
    {
        IStarfieldMod mod = StarfieldMod.Create(StarfieldRelease.Starfield)
                                                        .FromPath(pluginFile)
                                                        .WithLoadOrder(loadOrder)
                                                        .WithDefaultDataFolder()
                                                        .Mutable()
                                                        .Construct();
        return mod;
    }

    // Conversion to Avalonia bitmap
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public static WriteableBitmap? ConvertToAvaloniaBitmap(Image bitmap)
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

    public static bool IsObservableCollection(Type type)
    {
        return type.IsGenericType &&
               type.GetGenericTypeDefinition() == typeof(ObservableCollection<>);
    }
}
