using CommunityToolkit.Mvvm.ComponentModel;
using ErisToolkit.Common;
using Microsoft.VisualBasic;
using Mutagen.Bethesda.Starfield;
using Noggog;
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

        GenerateProperties(Children);
    }

    private void GenerateProperties(ObservableCollection<StarsystemView> children)
    {
        if (Value == null || Value.GetType().IsPrimitive || Value is string)
            return;

        foreach (var prop in Value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetIndexParameters().Length > 0 || !prop.CanRead) { continue; }
            if (prop.Name == "Name" || prop.Name == "Value" || prop.Name == "Count") { continue; }
            if (prop.Name.ToLower().Contains("mutagen")) { continue; }


            string name = "";

            if (prop.Name != prop.GetType().Name)
            {
                name = prop.Name;
            }

            dynamic propValue = prop.GetValue(Value) ?? "";

            var childNode = new StarsystemView(
                name: name,
                value: propValue,
                index: -1,
                isRootNode: false
            );

            if (propValue != null && Utils.IsObservableCollection(propValue?.GetType()))
            {
                foreach (var item in propValue)
                {
                    if (item.GetType() == typeof(ObservableObject)) { continue; }

                    var subItemNode = new StarsystemView(
                        name: $"[{propValue.IndexOf(item) ?? '?'}]",
                        value: item ?? "",
                        index: -1,
                        isRootNode: false
                    );
                    //subItemNode.GenerateProperties(subItemNode.Children);
                    childNode.Children.Add(subItemNode);
                }
            }

            children.Add(childNode);
        }
    }
}