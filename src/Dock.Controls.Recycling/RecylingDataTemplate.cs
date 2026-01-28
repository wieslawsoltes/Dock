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
            return DetachIfNeeded(recyclingTemplate.Build(data, existing), existing);
        }

        if (data is Control control)
        {
            return DetachIfNeeded(control, existing);
        }

        if (Parent is not { } parent)
        {
            return null;
        }

        var controlRecycling = ControlRecyclingDataTemplate.GetControlRecycling(parent);
        if (controlRecycling is not null)
        {
            return DetachIfNeeded(controlRecycling.Build(data, existing, parent) as Control, existing);
        }

        var dataTemplate = parent.FindDataTemplate(data);
        if (dataTemplate is IRecyclingDataTemplate recyclingDataTemplate)
        {
            return DetachIfNeeded(recyclingDataTemplate.Build(data, existing), existing);
        }

        return DetachIfNeeded(dataTemplate?.Build(data), existing);
    }

    private static Control? DetachIfNeeded(Control? control, Control? existing)
    {
        if (control is null || ReferenceEquals(control, existing))
        {
            return control;
        }

        DetachFromParent(control);
        return control;
    }

    private static void DetachFromParent(Control control)
    {
        var parent = control.Parent ?? control.GetVisualParent();

        switch (parent)
        {
            case Panel panel:
                panel.Children.Remove(control);
                break;
            case ContentPresenter presenter:
                presenter.Content = null;
                break;
            case ContentControl contentControl:
                contentControl.Content = null;
                break;
            case Decorator decorator when ReferenceEquals(decorator.Child, control):
                decorator.Child = null;
                break;
        }
    }
}
