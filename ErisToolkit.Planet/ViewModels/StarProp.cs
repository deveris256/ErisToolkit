using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErisToolkit.Planet.ViewModels;

public class StarProp<T> : INotifyPropertyChanged
{
    private T? _value;
    public object? HiddenValue;

    private string Value
    {
        get { return _value == null ? "NULL" : _value.ToString(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public StarProp(T? value, object? hiddenValue = null)
    {
        SetValue(value);
        HiddenValue = hiddenValue;
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void SetValue(T? value)
    {
        _value = value;
        OnPropertyChanged(nameof(value));
    }

    public string GetValue() { return Value ?? ""; }

    public override string ToString() { return Value.ToString(); }
}
