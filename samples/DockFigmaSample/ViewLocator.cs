using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Dock.Model.Core;
using ReactiveUI;
using Splat;

namespace DockFigmaSample;

public partial class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
        {
            return null;
        }

        var viewLocator = Locator.Current.GetService<IViewLocator>();
        if (viewLocator?.ResolveView(data) is Control control)
        {
            return control;
        }

        throw new Exception($"Unable to create view for type: {data.GetType()}");
    }

    public bool Match(object? data)
    {
        if (data is null)
        {
            return false;
        }

        if (data is IDockable)
        {
            return true;
        }

        var viewLocator = Locator.Current.GetService<IViewLocator>();
        return viewLocator?.ResolveView(data) is not null;
    }
}
