using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ErisToolkit.Planet.ViewModels;

public partial class CoordsXYZ : ObservableObject
{
    [ObservableProperty]
    double? _X;

    [ObservableProperty]
    double? _Y;

    [ObservableProperty]
    double? _Z;

    [ObservableProperty]
    string _value;

    [ObservableProperty]
    string _name;

    public CoordsXYZ(double? x, double? y, double? z, string name)
    {
        X = x;
        Y = y;
        Z = z;

        Value = "";
        Name = name;
    }

    public string GetValue() { return ""; }
    public override string ToString() { return ""; }
}
