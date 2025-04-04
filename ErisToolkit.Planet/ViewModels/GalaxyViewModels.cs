using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ErisToolkit.Planet.ViewModels;

public class StarViewModel : ReactiveObject
{
    public float X { get; set; }
    public float Y { get; set; }
    public float OriginalX { get; }
    public float OriginalY { get; }

    public string Name { get; set; }

    public double Size => 20;

    public StarViewModel(string name, float x, float y)
    {
        Name = name;
        OriginalX = x;
        OriginalY = y;

        X = x;
        Y = y;
    }
}

public partial class PlanetViewModel : ReactiveObject
{
    public Mutagen.Bethesda.Starfield.Planet.BodyTypeEnum BodyType { get; set; }

}