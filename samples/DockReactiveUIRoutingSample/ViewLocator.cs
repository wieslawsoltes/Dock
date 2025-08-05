using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Dock.Model.Core;
using ReactiveUI;
using Splat;

namespace DockReactiveUIRoutingSample;

public partial class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
        {
            return null;
        }

        // Fallback to ReactiveUI's view locator for registered views
        var viewLocator = Locator.Current.GetService<IViewLocator>();
        if (viewLocator?.ResolveView(data) is Control control)
            return control;

        throw new Exception($"Unable to create view for type: {data.GetType()}");
    }

    public bool Match(object? data)
    {
        return data is ReactiveObject || data is IDockable;
    }
}
