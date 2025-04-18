using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErisToolkit.Common.GameData;

/*
 * An image palette data class. Currently, used in
 * (planet_data)<->(bitmap) pipeline.
 * 
 * Each planet data value corresponds to a color at the
 * index of the palette.
 */

public class BiomPalette
{
    public Dictionary<int, List<int>> paletteData = new();

    public BiomPalette(string json)
    {
        var col = JsonConvert.DeserializeObject<Dictionary<string, List<List<int>>>>(json);
        var colors = col["palette"];

        for (int i = 0; i < colors.Count; i++)
        {
            paletteData[i] = [colors[i][0], colors[i][1], colors[i][2]];
        }
    }
}
