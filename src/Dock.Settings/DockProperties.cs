﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace Dock.Settings;

/// <summary>
/// Dock properties.
/// </summary>
public class DockProperties : AvaloniaObject
{
    /// <summary>
    /// Defines the IsDockTarget attached property.
    /// </summary>
    public static readonly AttachedProperty<bool> IsDockTargetProperty =
        AvaloniaProperty.RegisterAttached<DockProperties, Control, bool>("IsDockTarget", false, false, BindingMode.TwoWay);

    /// <summary>
    /// Defines the IsDragArea attached property.
    /// </summary>
    public static readonly AttachedProperty<bool> IsDragAreaProperty =
        AvaloniaProperty.RegisterAttached<DockProperties, Control, bool>("IsDragArea", false, false, BindingMode.TwoWay);

    /// <summary>
    /// Defines the IsDropArea attached property.
    /// </summary>
    public static readonly AttachedProperty<bool> IsDropAreaProperty =
        AvaloniaProperty.RegisterAttached<DockProperties, Control, bool>("IsDropArea", false, false, BindingMode.TwoWay);

    /// <summary>
    /// Define IsDragEnabled attached property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDragEnabledProperty =
        AvaloniaProperty.RegisterAttached<DockProperties, Control, bool>("IsDragEnabled", true, true, BindingMode.TwoWay);

    /// <summary>
    /// Define IsDropEnabled attached property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDropEnabledProperty =
        AvaloniaProperty.RegisterAttached<DockProperties, Control, bool>("IsDropEnabled", true, true, BindingMode.TwoWay);

    /// <summary>
    /// Defines the ShowDockSelectorOnly attached property.
    /// When set to true the dock adorner displays only
    /// drop selectors and hides the drop indicators.
    /// </summary>
    public static readonly AttachedProperty<bool> ShowDockSelectorOnlyProperty =
        AvaloniaProperty.RegisterAttached<DockProperties, Control, bool>("ShowDockSelectorOnly", false, false, BindingMode.TwoWay);

    /// <summary>
    /// Defines the DockAdornerHost attached property.
    /// When set it specifies the element that will host
    /// the dock target adorner instead of the adorned control itself.
    /// </summary>
    public static readonly AttachedProperty<Control?> DockAdornerHostProperty =
        AvaloniaProperty.RegisterAttached<DockProperties, Control, Control?>("DockAdornerHost", null, false, BindingMode.TwoWay);

    /// <summary>
    /// Gets the value of the IsDockTarget attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <returns>The IsDockTarget attached property.</returns>
    public static bool GetIsDockTarget(AvaloniaObject control)
    {
        return control.GetValue(IsDockTargetProperty);
    }

    /// <summary>
    /// Sets the value of the IsDockTarget attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="value">The value of the IsDockTarget property.</param>
    public static void SetIsDockTarget(AvaloniaObject control, bool value)
    {
        control.SetValue(IsDockTargetProperty, value);
    }

    /// <summary>
    /// Gets the value of the IsDragArea attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <returns>The IsDragArea attached property.</returns>
    public static bool GetIsDragArea(AvaloniaObject control)
    {
        return control.GetValue(IsDragAreaProperty);
    }

    /// <summary>
    /// Sets the value of the IsDragArea attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="value">The value of the IsDragArea property.</param>
    public static void SetIsDragArea(AvaloniaObject control, bool value)
    {
        control.SetValue(IsDragAreaProperty, value);
    }

    /// <summary>
    /// Gets the value of the IsDropArea attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <returns>The IsDropArea attached property.</returns>
    public static bool GetIsDropArea(AvaloniaObject control)
    {
        return control.GetValue(IsDropAreaProperty);
    }

    /// <summary>
    /// Sets the value of the IsDropArea attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="value">The value of the IsDropArea property.</param>
    public static void SetIsDropArea(AvaloniaObject control, bool value)
    {
        control.SetValue(IsDropAreaProperty, value);
    }

    /// <summary>
    /// Gets the value of the IsDragEnabled attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <returns>The IsDragEnabled attached property.</returns>
    public static bool GetIsDragEnabled(AvaloniaObject control)
    {
        return control.GetValue(IsDragEnabledProperty);
    }

    /// <summary>
    /// Sets the value of the IsDragEnabled attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="value">The value of the IsDragEnabled property.</param>
    public static void SetIsDragEnabled(Control control, bool value)
    {
        control.SetValue(IsDragEnabledProperty, value);
    }

    /// <summary>
    /// Gets the value of the IsDropEnabled attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <returns>The IsDropEnabled attached property.</returns>
    public static bool GetIsDropEnabled(AvaloniaObject control)
    {
        return control.GetValue(IsDropEnabledProperty);
    }

    /// <summary>
    /// Sets the value of the IsDropEnabled attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="value">The value of the IsDropEnabled property.</param>
    public static void SetIsDropEnabled(AvaloniaObject control, bool value)
    {
        control.SetValue(IsDropEnabledProperty, value);
    }

    /// <summary>
    /// Gets the value of the ShowDockSelectorOnly attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <returns>The ShowDockSelectorOnly attached property.</returns>
    public static bool GetShowDockSelectorOnly(AvaloniaObject control)
    {
        return control.GetValue(ShowDockSelectorOnlyProperty);
    }

    /// <summary>
    /// Sets the value of the ShowDockSelectorOnly attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="value">The value of the ShowDockSelectorOnly property.</param>
    public static void SetShowDockSelectorOnly(AvaloniaObject control, bool value)
    {
        control.SetValue(ShowDockSelectorOnlyProperty, value);
    }

    /// <summary>
    /// Gets the value of the DockAdornerHost attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <returns>The DockAdornerHost attached property.</returns>
    public static Control? GetDockAdornerHost(AvaloniaObject control)
    {
        return control.GetValue(DockAdornerHostProperty);
    }

    /// <summary>
    /// Sets the value of the DockAdornerHost attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="value">The host control.</param>
    public static void SetDockAdornerHost(AvaloniaObject control, Control? value)
    {
        control.SetValue(DockAdornerHostProperty, value);
    }
}
