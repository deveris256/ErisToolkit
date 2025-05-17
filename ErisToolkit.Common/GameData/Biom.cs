using System.Runtime.InteropServices;
using System.Drawing;
using DynamicData;
using Mutagen.Bethesda.Starfield;
using System.Reflection.PortableExecutable;
using Avalonia.Media.Imaging;

namespace ErisToolkit.Common.GameData;

/*
 * ErisToolkit by Deveris256
 * 
 * This file contains data relevant to .biom file,
 * which is used to set the biome and resource per-"pixel"
 * of a planet surface.
 * 
 * The reverse engineering of the .biom file was inspired
 * by the repository
 * https://github.com/PixelRick/StarfieldScripts
 * 
 */


/*
 * Most of the functions of the Biom class
 * are low-level, therefore require additional
 * handling.
 */
public class Biom
{
    public static int[] known_resource_ids = [8, 88, 0, 80, 1, 81, 2, 82, 3, 83, 4, 84];
    public static readonly uint[] gridSize = { 0x100, 0x100 };
    public static readonly uint gridFlatSize = gridSize[0] * gridSize[1];

    public BiomStruct biomStruct;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BiomStruct
    {
        public const ushort Magic = 0x105;

        public UInt32 NumBiomes;
        public UInt32[] BiomeIds;

        public const uint Constant2 = 2;
        public static readonly uint[] GridSize = { 0x100, 0x100 };
        public uint GridFlatSize = gridFlatSize;

        public UInt32[] BiomeGridN;
        public byte[] ResrcGridN;
        public UInt32[] BiomeGridS;
        public byte[] ResrcGridS;

        public BiomStruct() { }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Magic);
            writer.Write((uint)BiomeIds.Length);
            foreach (var id in BiomeIds) { writer.Write(id); }
            writer.Write((uint)2);

            writer.Write(GridSize[0]);
            writer.Write(GridSize[1]);
            writer.Write(GridFlatSize);

            for (int i = 0; i < GridFlatSize; i++) { writer.Write(BiomeGridN[i]); }

            writer.Write(GridFlatSize);
            for (int i = 0; i < GridFlatSize; i++) { writer.Write(ResrcGridN[i]); }


            writer.Write(GridSize[0]);
            writer.Write(GridSize[1]);
            writer.Write(GridFlatSize);

            for (int i = 0; i < GridFlatSize; i++) { writer.Write(BiomeGridS[i]); }

            writer.Write(GridFlatSize);
            for (int i = 0; i < GridFlatSize; i++) { writer.Write(ResrcGridS[i]); }

            writer.Close();
        }
    }

    public enum BiomDataSide
    {
        N,
        S,
        NULL
    }

    public Biom(string filePath)
    {
        using (var stream = new FileStream(filePath, FileMode.Open))
        using (var reader = new BinaryReader(stream))
        {
            if (reader.ReadUInt16() != BiomStruct.Magic)
            { throw new InvalidDataException("Invalid biom file (invalid magic number)"); }

            BiomStruct biom = new();

            biom.NumBiomes = reader.ReadUInt32();

            // Biome IDs
            biom.BiomeIds = new uint[biom.NumBiomes];
            for (int i = 0; i < biom.NumBiomes; i++)
            {
                biom.BiomeIds[i] = reader.ReadUInt32();
            }

            reader.ReadUInt32(); // Unk, Value is 2(?)

            reader.ReadUInt32(); // Grid Size [2]
            reader.ReadUInt32(); // Grid Size [2]
            reader.ReadUInt32(); // Grid Flatsize

            // Biome Grid 1
            biom.BiomeGridN = new uint[biom.GridFlatSize];
            for (int i = 0; i < biom.GridFlatSize; i++)
            {
                biom.BiomeGridN[i] = reader.ReadUInt32();
            }
            reader.ReadUInt32(); // Grid Flatsize

            // Res Grid 1
            biom.ResrcGridN = reader.ReadBytes((int)biom.GridFlatSize);

            reader.ReadUInt32(); // Grid Size [2]
            reader.ReadUInt32(); // Grid Size [2]
            reader.ReadUInt32(); // Grid Flatsize

            // Biome Grid 2
            biom.BiomeGridS = new uint[biom.GridFlatSize];
            for (int i = 0; i < biom.GridFlatSize; i++)
            {
                biom.BiomeGridS[i] = reader.ReadUInt32();
            }
            reader.ReadUInt32(); // Grid Flatsize

            // Res Grid 2
            biom.ResrcGridS = reader.ReadBytes((int)biom.GridFlatSize);

            biomStruct = biom;
        }
    }

    public void AddBiome(UInt32 biomeID)
    {
        biomStruct.NumBiomes += 1;
        var list = biomStruct.BiomeIds.ToList();
        list.Add(biomeID);
        biomStruct.BiomeIds = list.ToArray();
    }

    public void RemoveBiome(int index)
    {
        biomStruct.NumBiomes -= 1;
        var list = biomStruct.BiomeIds.ToList();
        list.RemoveAt(index);
        biomStruct.BiomeIds = list.ToArray();
    }

    public void ReplaceBiomeData(uint[] newData, BiomDataSide side)
    {
        switch (side)
        {
            case BiomDataSide.N:
                if (newData.Length != biomStruct.BiomeGridN.Length) { throw new Exception(); }
                biomStruct.BiomeGridN = newData;
                break;
            case BiomDataSide.S:
                if (newData.Length != biomStruct.BiomeGridS.Length) { throw new Exception(); }
                biomStruct.BiomeGridS = newData;
                break;
            default: return;
        }
    }

    public void ReplaceResourceData(byte[] newData, BiomDataSide side)
    {
        switch (side)
        {
            case BiomDataSide.N:
                if (newData.Length != biomStruct.ResrcGridN.Length) { return; }
                biomStruct.ResrcGridN = newData;
                break;
            case BiomDataSide.S:
                if (newData.Length != biomStruct.ResrcGridS.Length) { return; }
                biomStruct.ResrcGridS = newData;
                break;
            default: return;
        }
    }
    /*
    public bool LoadBiomeImage(System.Drawing.Bitmap bitmap, BiomDataSide side)
    {
        UInt32[] biomGrid;

        switch (side)
        {
            case BiomDataSide.N: biomGrid = biomStruct.BiomeGridN; break;
            case BiomDataSide.S: biomGrid = biomStruct.BiomeGridS; break;
            default: return false;
        }
        if (bitmap.Size.Width != (int)gridSize[0] || bitmap.Size.Height != (int)gridSize[1])
        {
            return false;
        }

        for (int i = 0; i < gridFlatSize; i++)
        {
            biomGrid[i] = 0;
        }

        Dictionary<int, List<int>> colors = palette.paletteData;

        for (int i = 0; i < gridFlatSize; i++)
        {
            int x = i % (int)gridSize[0];
            int y = i / (int)gridSize[0];
            Color pixel;

            try { pixel = bitmap.GetPixel(x, y); } catch (Exception e) { return false; }

            foreach (var col in colors)
            {
                var colValue = col.Value;

                if (colValue[0] == pixel.R && colValue[1] == pixel.G && colValue[2] == pixel.B)
                {
                    biomGrid[i] = biomStruct.BiomeIds[col.Key];
                    break;
                }
            }
        }

        return true;
    }

    public bool LoadResourceImage(System.Drawing.Bitmap bitmap, BiomDataSide side)
    {
        byte[] resGrid;

        switch (side)
        {
            case BiomDataSide.N: resGrid = biomStruct.ResrcGridN; break;
            case BiomDataSide.S: resGrid = biomStruct.ResrcGridS; break;
            default: return false;
        }
        if (bitmap.Size.Width != (int)gridSize[0] || bitmap.Size.Height != (int)gridSize[1])
        {
            return false;
        }

        for (int i = 0; i < gridFlatSize; i++)
        {
            resGrid[i] = 0;
        }

        Dictionary<int, List<int>> colors = palette.paletteData;

        for (int i = 0; i < gridFlatSize; i++)
        {
            int x = i % (int)gridSize[0];
            int y = i / (int)gridSize[0];
            Color pixel;

            try { pixel = bitmap.GetPixel(x, y); } catch (Exception e) { return false; }

            foreach (var col in colors)
            {
                var colValue = col.Value;

                if (colValue[0] == pixel.R && colValue[1] == pixel.G && colValue[2] == pixel.B)
                {
                    resGrid[i] = (byte)known_resource_ids[col.Key];
                    break;
                }
            }
        }

        return true;
    }*/
}
