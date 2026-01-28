// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Recycling.Model;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Recycling;

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
        return Build(param, null);
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
        if (data is IRecyclingDataTemplate recyclingTemplate && !ReferenceEquals(recyclingTemplate, this))
        {
            return BuildFromRecyclingTemplate(recyclingTemplate, data, existing);
        }

        if (data is Control control)
        {
            if (ReferenceEquals(control, existing))
            {
                return control;
            }

            return TryDetachFromParent(control) ? control : null;
        }

        if (Parent is not { } parent)
        {
            return null;
        }

        var controlRecycling = ControlRecyclingDataTemplate.GetControlRecycling(parent);
        if (controlRecycling is not null)
        {
            return controlRecycling.Build(data, existing, parent) as Control;
        }

        var dataTemplate = parent.FindDataTemplate(data);
        if (dataTemplate is IRecyclingDataTemplate recyclingDataTemplate)
        {
            return BuildFromRecyclingTemplate(recyclingDataTemplate, data, existing);
        }

        return BuildFromDataTemplate(dataTemplate, data, existing);
    }

    private static Control? BuildFromRecyclingTemplate(IRecyclingDataTemplate template, object? data, Control? existing)
    {
        var control = template.Build(data, existing);
        if (control is null)
        {
            return null;
        }

        if (ReferenceEquals(control, existing))
        {
            return control;
        }

        if (TryDetachFromParent(control))
        {
            return control;
        }

        var rebuilt = template.Build(data, null);
        if (rebuilt is null)
        {
            return null;
        }

        if (ReferenceEquals(rebuilt, existing))
        {
            return rebuilt;
        }

        return TryDetachFromParent(rebuilt) ? rebuilt : null;
    }

    private static Control? BuildFromDataTemplate(IDataTemplate? template, object? data, Control? existing)
    {
        var control = template?.Build(data) as Control;
        if (control is null)
        {
            return null;
        }

        if (ReferenceEquals(control, existing))
        {
            return control;
        }

        if (TryDetachFromParent(control))
        {
            return control;
        }

        var rebuilt = template?.Build(data) as Control;
        if (rebuilt is null)
        {
            return null;
        }

        if (ReferenceEquals(rebuilt, existing))
        {
            return rebuilt;
        }

        return TryDetachFromParent(rebuilt) ? rebuilt : null;
    }

    private static bool TryDetachFromParent(Control control)
    {
        var parent = control.Parent ?? control.GetVisualParent();

        if (parent is null)
        {
            return true;
        }

        switch (parent)
        {
            case Panel panel:
                return panel.Children.Remove(control);
            case ContentPresenter presenter when ReferenceEquals(presenter.Content, control):
                presenter.Content = null;
                return true;
            case ContentControl contentControl:
                if (ReferenceEquals(contentControl.Content, control))
                {
                    contentControl.Content = null;
                    return true;
                }

                return false;
            case Decorator decorator when ReferenceEquals(decorator.Child, control):
                decorator.Child = null;
                return true;
            default:
                return false;
        }
    }
}
