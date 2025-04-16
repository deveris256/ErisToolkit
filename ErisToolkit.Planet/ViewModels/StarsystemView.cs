using CommunityToolkit.Mvvm.ComponentModel;
using Mutagen.Bethesda.Starfield;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ErisToolkit.Planet.ViewModels;

public partial class StarsystemView : ObservableObject
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private object _value;

    public ObservableCollection<StarsystemView> Children { get; } = new();

    public StarsystemView(string name, object value, int index, bool isRootNode = true)
    {
        Name = name;
        Value = value ?? "";

        if (isRootNode)
        {
            if (Common.starListIsEditable[index])
            {
                var star = Common.currentMod.Stars.FirstOrDefault(x => x.EditorID == Common.starList[index]);
                if (star != null) { Value = new StarInfo(star); }
            }
            else
            {
                if (Common.linkCache.TryResolve<IStarGetter>(Common.starList[index], out var star))
                {
                    Value = new StarInfo(star);
                }
            }
        }

        GenerateProperties();
    }

    private void GenerateProperties()
    {
        try
        {
            if (Value.GetType().IsPrimitive || Value is string)
                return;

            foreach (var prop in Value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.GetIndexParameters().Length > 0 || !prop.CanRead)
                    continue;

                if (prop.Name == "Name" || prop.Name == "Value")
                {
                    continue;
                }

                var propValue = prop.GetValue(Value) ?? "";

                var childNode = new StarsystemView(
                    name: prop.Name,
                    value: propValue,
                    index: -1,
                    isRootNode: false
                );

                Children.Add(childNode);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating properties: {ex}");
        }
    }
}