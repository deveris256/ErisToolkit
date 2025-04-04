using System.Runtime.InteropServices;
using System.Drawing;
using Newtonsoft.Json;

namespace ErisToolkit.Common.GameData;

/*
 * ErisToolkit by Deveris256
 * 
 * This file contains data relevant to .biom file,
 * which is used to set the biome and resource per-"pixel"
 * of a planet surface.
 * 
 * The reverse engineering of the .biom file was inspuired
 * by the repository
 * https://github.com/PixelRick/StarfieldScripts
 * 
 */

public class Biom
{
    private static int[] known_resource_ids = [8, 88, 0, 80, 1, 81, 2, 82, 3, 83, 4, 84];
    private static readonly uint[] gridSize = { 0x100, 0x100 };
    private static readonly uint gridFlatSize = gridSize[0] * gridSize[1];

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
        { // TODO: Write
            writer.Write(Magic);
            writer.Write((uint)BiomeIds.Length);
            foreach (var id in BiomeIds)
            {
                writer.Write(id);
            }

            Span<byte> structData = MemoryMarshal.AsBytes(new Span<BiomStruct>(ref this));
            writer.Write(structData);
        }
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

    public System.Drawing.Bitmap GetBiomeImage(uint[] grid)
    {
        System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap((int)gridSize[0], (int)gridSize[1]);

        string json = Properties.Resources.palette1;
        Console.WriteLine(json);

        var palette = new Palette(json);
        Dictionary<int, List<int>> colors = palette.paletteData;

        for (int i = 0; i < gridFlatSize; i++)
        {
            int value = Array.IndexOf(biomStruct.BiomeIds, grid[i]);

            int r = colors[value][0];
            int g = colors[value][1];
            int b = colors[value][2];

            Color color = System.Drawing.Color.FromArgb(255, r, g, b);

            int x = i % (int)gridSize[0];
            int y = i / (int)gridSize[0];
            bitmap.SetPixel(x, y, color);
        }

        return bitmap;
    }

    public System.Drawing.Bitmap GetResourceImage(byte[] grid)
    {
        System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap((int)gridSize[0], (int)gridSize[1]);

        string json = Properties.Resources.palette1;
        Console.WriteLine(json);

        var palette = new Palette(json);
        Dictionary<int, List<int>> colors = palette.paletteData;

        for (int i = 0; i < gridFlatSize; i++)
        {
            int value = Array.IndexOf(known_resource_ids, grid[i]);

            int r = colors[value][0];
            int g = colors[value][1];
            int b = colors[value][2];

            Color color = System.Drawing.Color.FromArgb(255, r, g, b);

            int x = i % (int)gridSize[0];
            int y = i / (int)gridSize[0];
            bitmap.SetPixel(x, y, color);
        }

        return bitmap;
    }
}
