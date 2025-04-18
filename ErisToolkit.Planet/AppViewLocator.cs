using ReactiveUI;
using System;

namespace ErisToolkit.Planet;

public class AppViewLocator : ReactiveUI.IViewLocator
{
    public IViewFor ResolveView<T>(T viewModel, string contract = null) => viewModel switch
    {
        _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
    };
}
