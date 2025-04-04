using Avalonia.Media.Imaging;
using ErisToolkit.Common;
using ErisToolkit.Common.GameData;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Starfield;
using System.Globalization;
using DynamicData;
using Noggog;
using System;
using System.Diagnostics;
using Mutagen.Bethesda.Plugins.Records;

/* 
 * ErisToolkit by Deveris256
 * 
 * 'Common' class contains static variables used
 * by various aspects of the program, such as
 * images.
 * 
 */

namespace ErisToolkit.Planet
{
    public static class Common
    {
        public static IStarfieldModDisposableGetter? mod;
        public static Biom? biom;
        public static int currentEditableIndex = 0;

        // Gets bitmap
        public static System.Drawing.Bitmap? GetBitmap(int index = -1)
        {
            if (biom == null) return null;
            if (index == -1) { index = currentEditableIndex; }

            switch (index)
            {
                case 0: return biom.GetBiomeImage(biom.biomStruct.BiomeGridN);
                case 1: return biom.GetBiomeImage(biom.biomStruct.BiomeGridS);
                case 2: return biom.GetResourceImage(biom.biomStruct.ResrcGridN);
                case 3: return biom.GetResourceImage(biom.biomStruct.ResrcGridS);
                default: return null;
            }
        }

        // Gets Avalonia-compatible image
        public static WriteableBitmap? GetImage(int index = -1)
        {
            if (biom == null) return null;
            if (index == -1) { index = currentEditableIndex; }

            switch (index)
            {
                case 0: return Utils.ConvertToAvaloniaBitmap(biom.GetBiomeImage(biom.biomStruct.BiomeGridN));
                case 1: return Utils.ConvertToAvaloniaBitmap(biom.GetBiomeImage(biom.biomStruct.BiomeGridS));
                case 2: return Utils.ConvertToAvaloniaBitmap(biom.GetResourceImage(biom.biomStruct.ResrcGridN));
                case 3: return Utils.ConvertToAvaloniaBitmap(biom.GetResourceImage(biom.biomStruct.ResrcGridS));
                default: return null;
            }
        }
    }
}
