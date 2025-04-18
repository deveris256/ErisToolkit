using CommunityToolkit.Mvvm.ComponentModel;
using Mutagen.Bethesda.Starfield;
using System;
using System.Collections.ObjectModel;

namespace ErisToolkit.Planet.ViewModels;

public partial class PlanetModelInfo : ObservableObject
{
    [ObservableProperty]
    public StarProp<string?> _StarModel;
    [ObservableProperty]
    public ObservableCollection<PlanetModelInfoSubItem> _SubItems;
    [ObservableProperty]
    public ObservableCollection<string> _StrSubItems;

    public PlanetModelInfo(IPlanetModelComponentGetter planetModelComp)
    {
        SubItems = new();
        StrSubItems = new();

        StarModel = new(planetModelComp.Model?.File?.ToString() ?? "");

        if (planetModelComp.XMPM != null)
        {
            if (planetModelComp.XMPM.UnknownSubItems != null)
            {
                foreach (var sub in planetModelComp.XMPM.UnknownSubItems)
                {
                    SubItems.Add(new(sub));
                }
            }
        }

    }
}

public partial class PlanetModelInfoSubItem : ObservableObject
{
    IPlanetModelComponentXMPMSubItemGetter SubItem;

    [ObservableProperty]
    StarProp<string?> _ResourceID;
    [ObservableProperty]
    StarProp<string?> _File;
    [ObservableProperty]
    StarProp<string?> _Extension;
    [ObservableProperty]
    StarProp<string?> _Folder;

    public PlanetModelInfoSubItem(IPlanetModelComponentXMPMSubItemGetter subItem)
    {
        SubItem = subItem;
        ResourceID = new(subItem.ResourceID);
        File = new(BitConverter.ToString(BitConverter.GetBytes(subItem.FileHash)).Replace("-", "").ToLower());
        Extension = new(subItem.Extension);
        Folder = new(BitConverter.ToString(BitConverter.GetBytes(subItem.FolderHash)).Replace("-", "").ToLower());
    }
}