using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Dock.Model.Core;
using ReactiveUI;

namespace DockReactiveUIDiSample;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
        {
            return null;
        }

        var viewName = data.GetType().FullName?.Replace("ViewModel", "View");
        if (viewName is null)
            return null;

        var viewType = Type.GetType(viewName);
        if (viewType != null && (Application.Current as App)?.ServiceProvider?.GetService(viewType) is Control control)
        {
            control.DataContext = data;
            return control;
        }

        return new TextBlock { Text = $"Not Found: {viewName}" };
    }

    public bool Match(object? data)
    {
        return data is ReactiveObject || data is IDockable;
    }
}
