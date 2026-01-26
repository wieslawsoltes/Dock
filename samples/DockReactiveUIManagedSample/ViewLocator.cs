using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
//using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Core;
using ReactiveUI;
using StaticViewLocator;

namespace DockReactiveUIManagedSample;

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

        if (s_views.TryGetValue(type, out var func))
        {
            return func.Invoke();
        }

        throw new Exception($"Unable to create view for type: {type}");
    }

    public bool Match(object? data)
    {
        if (data is null)
        {
            return false;
        }

        var type = data.GetType();
        return data is IDockable || s_views.ContainsKey(type);
    }
}
