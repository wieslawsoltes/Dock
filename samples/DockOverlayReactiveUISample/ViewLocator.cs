using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using StaticViewLocator;

namespace DockOverlayReactiveUISample;

[StaticViewLocator]
public partial class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
        {
            return null;
        }

        var type = data.GetType();

        if (s_views.TryGetValue(type, out var factory))
        {
            return factory.Invoke();
        }

        // Let other templates (e.g., DockControl defaults) handle types we don't map.
        return null;
    }

    public bool Match(object? data)
    {
        if (data is null)
        {
            return false;
        }

        var type = data.GetType();
        return s_views.ContainsKey(type);
    }
}
