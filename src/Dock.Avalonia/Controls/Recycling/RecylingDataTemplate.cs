using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Controls.Recycling;

/// <summary>
/// 
/// </summary>
public class ControlRecyclingDataTemplate : AvaloniaObject, IRecyclingDataTemplate
{
    /// <summary>
    /// 
    /// </summary>
    public static readonly StyledProperty<Control?> ParentProperty =
        AvaloniaProperty.Register<ControlRecyclingDataTemplate, Control?>(nameof(Parent));

    /// <summary>
    /// 
    /// </summary>
    public Control? Parent
    {
        get => GetValue(ParentProperty);
        set => SetValue(ParentProperty, value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public Control? Build(object? param)
    {
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool Match(object? data)
    {
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="existing"></param>
    /// <returns></returns>
    public Control? Build(object? data, Control? existing)
    {
        if (Parent is not { } parent)
        {
            return null;
        }

        var controlRecycling = DockProperties.GetControlRecycling(parent);
        if (controlRecycling is not null)
        {
            return controlRecycling.Build(data, existing, parent) as Control;
        }

        return parent.FindDataTemplate(data)?.Build(data);
    }
}
