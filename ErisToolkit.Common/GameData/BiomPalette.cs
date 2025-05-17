using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
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

public partial class BiomPaletteItem<T> : ObservableObject
{
    [ObservableProperty] public Color _Color;
    [ObservableProperty] public T _Data;

    public BiomPaletteItem(Color color, T data)
    {
        Color = color;
        Data = data;
    }
}
