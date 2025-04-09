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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using ErisToolkit.Planet.Properties;
using Avalonia.Controls;
using System.Drawing;

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

        public static ObservableCollection<BiomDataList> biomesList = new();
        public static ObservableCollection<BiomDataList> resourcesList = new();

        public static Palette? palette;

        public static int StringToIndex(string str)
        {
            switch (str.ToLower())
            {
                case "biomgridn": return 0;
                case "biomgrids": return 1;
                case "resgridn": return 2;
                case "resgrids": return 3;
                default: return -1;
            }
        }

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

        public static void AddBiomeData(DataGrid dataGrid)
        {
            biomesList.Clear();

            if (biom == null) return;

            for (int i = 0; i < biom.biomStruct.NumBiomes; i++)
            {
                var color = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(
                        (byte)Biom.palette.paletteData[i][0],
                        (byte)Biom.palette.paletteData[i][1],
                        (byte)Biom.palette.paletteData[i][2]
                ), 1);

                biomesList.Add(new BiomDataList(
                        biom.biomStruct.BiomeIds[i].ToString(),
                        "???",
                        color
                   ));
            }
        }

        public static void AddResourceData()
        {
            resourcesList.Clear();

            if (biom == null) return;

            List<byte> tempList = new();

            for (int i = 0; i < biom.biomStruct.GridFlatSize; i++)
            {
                byte resourceIdN = (byte)Array.IndexOf(Biom.known_resource_ids, biom.biomStruct.ResrcGridN[i]);
                byte resourceIdS = (byte)Array.IndexOf(Biom.known_resource_ids, biom.biomStruct.ResrcGridS[i]);

                if (!tempList.Contains(resourceIdN))
                {
                    tempList.Add(resourceIdN);

                    var color = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(
                        (byte)Biom.palette.paletteData[resourceIdN][0],
                        (byte)Biom.palette.paletteData[resourceIdN][1],
                        (byte)Biom.palette.paletteData[resourceIdN][2]
                    ), 1);

                    resourcesList.Add(new BiomDataList(
                        biom.biomStruct.ResrcGridN[i].ToString(),
                        "???",
                        color
                    ));
                }

                if (!tempList.Contains(resourceIdS))
                {
                    tempList.Add(resourceIdS);

                    var color = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(
                        (byte)Biom.palette.paletteData[resourceIdS][0],
                        (byte)Biom.palette.paletteData[resourceIdS][1],
                        (byte)Biom.palette.paletteData[resourceIdS][2]
                    ), 1);

                    resourcesList.Add(new BiomDataList(
                        biom.biomStruct.ResrcGridS[i].ToString(),
                        "???",
                        color
                    ));
                }
            }
        }
    }
}
