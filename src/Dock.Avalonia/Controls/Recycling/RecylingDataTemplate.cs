using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Controls.Recycling;

/// <summary>
/// 
/// </summary>
public class ControlRecyclingDataTemplate : AvaloniaObject, IRecyclingDataTemplate
{
    /// <summary>
    /// Defines the ControlRecycling attached property.
    /// </summary>
    public static readonly AttachedProperty<IControlRecycling?> ControlRecyclingProperty =
        AvaloniaProperty.RegisterAttached<ControlRecyclingDataTemplate, Control, IControlRecycling?>(
            "ControlRecycling",
            null, 
            true, 
            BindingMode.TwoWay);

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
    /// Gets the value of the ControlRecycling attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <returns>The ControlRecycling attached property.</returns>
    public static IControlRecycling? GetControlRecycling(AvaloniaObject control)
    {
        return control.GetValue(ControlRecyclingProperty);
    }

    /// <summary>
    /// Sets the value of the ControlRecycling attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="value">The value of the ControlRecycling property.</param>
    public static void SetControlRecycling(AvaloniaObject control, IControlRecycling? value)
    {
        control.SetValue(ControlRecyclingProperty, value);
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

        var controlRecycling = ControlRecyclingDataTemplate.GetControlRecycling(parent);
        if (controlRecycling is not null)
        {
            return controlRecycling.Build(data, existing, parent) as Control;
        }

        return parent.FindDataTemplate(data)?.Build(data);
    }
}
