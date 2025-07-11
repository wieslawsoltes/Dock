using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Dock.Model.Core;
using ReactiveUI;
using StaticViewLocator;

namespace DockReactiveUIRoutingSample;

[StaticViewLocator]
public partial class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
            return null;

        var type = data.GetType();
        if (s_views.TryGetValue(type, out var func))
            return func.Invoke();

        throw new Exception($"Unable to create view for type: {type}");
    }

    public bool Match(object? data)
    {
        return data is ReactiveObject || data is IDockable;
    }
}
