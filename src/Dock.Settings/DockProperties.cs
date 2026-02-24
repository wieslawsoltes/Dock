// Copyright (c) Wiesław Šoltés. All rights reserved.
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
    /// Defines the ShowDockIndicatorOnly attached property.
    /// When set to true the dock adorner displays only
    /// drop indicators and hides the dock target visuals.
    /// </summary>
    public static readonly AttachedProperty<bool> ShowDockIndicatorOnlyProperty =
        AvaloniaProperty.RegisterAttached<DockProperties, Control, bool>("ShowDockIndicatorOnly", false, false, BindingMode.TwoWay);

    /// <summary>
    /// Defines the IndicatorDockOperation attached property.
    /// When <see cref="ShowDockIndicatorOnlyProperty"/> is true this value
    /// specifies which dock operation the entire control represents.
    /// </summary>
    public static readonly AttachedProperty<Dock.Model.Core.DockOperation> IndicatorDockOperationProperty =
        AvaloniaProperty.RegisterAttached<DockProperties, Control, Dock.Model.Core.DockOperation>(
            "IndicatorDockOperation", Dock.Model.Core.DockOperation.Fill, false, BindingMode.TwoWay);

    /// <summary>
    /// Defines the DockAdornerHost attached property.
    /// When set it specifies the element that will host
    /// the dock target adorner instead of the adorned control itself.
    /// </summary>
    public static readonly AttachedProperty<Control?> DockAdornerHostProperty =
        AvaloniaProperty.RegisterAttached<DockProperties, Control, Control?>("DockAdornerHost", null, true, BindingMode.TwoWay);

    /// <summary>
    /// Defines the ExternalDockControl attached property.
    /// When set on controls outside the DockControl visual tree (for example an external DocumentTabStrip),
    /// it associates them with the owning DockControl for drag/drop hit testing and tab drag initiation.
    /// </summary>
    public static readonly AttachedProperty<Control?> ExternalDockControlProperty =
        AvaloniaProperty.RegisterAttached<DockProperties, Control, Control?>("ExternalDockControl", null, true, BindingMode.TwoWay);

    /// <summary>
    /// Defines the DockGroup attached property.
    /// Specifies the docking group for restriction-based docking operations.
    /// Elements can only dock with other elements of the same or compatible group.
    /// This property is inherited down the visual tree, allowing parent containers
    /// to set a group that automatically applies to all child dockables.
    /// </summary>
    public static readonly AttachedProperty<string?> DockGroupProperty =
        AvaloniaProperty.RegisterAttached<DockProperties, Control, string?>("DockGroup", null, inherits: true, defaultBindingMode: BindingMode.TwoWay);

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
    /// Gets the value of the ShowDockIndicatorOnly attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <returns>The ShowDockIndicatorOnly attached property.</returns>
    public static bool GetShowDockIndicatorOnly(AvaloniaObject control)
    {
        return control.GetValue(ShowDockIndicatorOnlyProperty);
    }

    /// <summary>
    /// Sets the value of the ShowDockIndicatorOnly attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="value">The value of the ShowDockIndicatorOnly property.</param>
    public static void SetShowDockIndicatorOnly(AvaloniaObject control, bool value)
    {
        control.SetValue(ShowDockIndicatorOnlyProperty, value);
    }

    /// <summary>
    /// Gets the value of the IndicatorDockOperation attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <returns>The IndicatorDockOperation attached property.</returns>
    public static Dock.Model.Core.DockOperation GetIndicatorDockOperation(AvaloniaObject control)
    {
        return control.GetValue(IndicatorDockOperationProperty);
    }

    /// <summary>
    /// Sets the value of the IndicatorDockOperation attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="value">The dock operation value.</param>
    public static void SetIndicatorDockOperation(AvaloniaObject control, Dock.Model.Core.DockOperation value)
    {
        control.SetValue(IndicatorDockOperationProperty, value);
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

    /// <summary>
    /// Gets the value of the ExternalDockControl attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <returns>The associated DockControl host reference.</returns>
    public static Control? GetExternalDockControl(AvaloniaObject control)
    {
        return control.GetValue(ExternalDockControlProperty);
    }

    /// <summary>
    /// Sets the value of the ExternalDockControl attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="value">The owning DockControl.</param>
    public static void SetExternalDockControl(AvaloniaObject control, Control? value)
    {
        control.SetValue(ExternalDockControlProperty, value);
    }

    /// <summary>
    /// Gets the value of the DockGroup attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <returns>The DockGroup attached property.</returns>
    public static string? GetDockGroup(AvaloniaObject control)
    {
        return control.GetValue(DockGroupProperty);
    }

    /// <summary>
    /// Sets the value of the DockGroup attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="value">The docking group name.</param>
    public static void SetDockGroup(AvaloniaObject control, string? value)
    {
        control.SetValue(DockGroupProperty, value);
    }
}
