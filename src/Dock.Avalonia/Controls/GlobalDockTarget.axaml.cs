// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Settings;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="GlobalDockTarget"/> xaml.
/// </summary>
public class GlobalDockTarget : DockTargetBase
{
    /// <summary>Defines the <see cref="EdgeHitTestThickness"/> property.</summary>
    public static readonly StyledProperty<double> EdgeHitTestThicknessProperty =
        AvaloniaProperty.Register<GlobalDockTarget, double>(nameof(EdgeHitTestThickness), double.NaN,
            validate: value => double.IsNaN(value) || (double.IsFinite(value) && value >= 0));

    /// <summary>
    /// Gets or sets the maximum outer-edge hit thickness in DIPs for stretched edge selectors.
    /// The effective thickness is also capped at one fifth of the hovered pane's width or height.
    /// NaN (the default) preserves template-defined selector hit areas, including docking buttons.
    /// </summary>
    public double EdgeHitTestThickness
    {
        get => GetValue(EdgeHitTestThicknessProperty);
        set => SetValue(EdgeHitTestThicknessProperty, value);
    }

    internal override bool IsWithinTarget(Point point, Visual relativeTo, Control dropControl, DockOperation operation)
    {
        if (double.IsNaN(EdgeHitTestThickness)) return true;
        var local = GetTargetPoint(point, relativeTo);
        if (local is not { } p) return false;
        var horizontal = operation is DockOperation.Left or DockOperation.Right;
        var pane = ResolvePane(dropControl);
        var paneSize = horizontal ? pane.Bounds.Width : pane.Bounds.Height;
        var thickness = Math.Min(EdgeHitTestThickness, paneSize / 5);
        var distance = operation switch
        {
            DockOperation.Left => p.X,
            DockOperation.Right => Bounds.Width - p.X,
            DockOperation.Top => p.Y,
            DockOperation.Bottom => Bounds.Height - p.Y,
            _ => double.PositiveInfinity
        };
        return distance >= 0 && distance < thickness;
    }

    private static Control ResolvePane(Control dropControl)
    {
        for (Visual? current = dropControl; current is not null and not DockControl; current = current.GetVisualParent())
        {
            if (current is not Control control) continue;
            if (DockProperties.GetDockAdornerHost(control) is { } host) return host;
            if (control.GetValue(DockProperties.IsDockTargetProperty)) return control;
        }
        return dropControl;
    }

    /// <inheritdoc />
    protected override DockOperation DefaultDockOperation => DockOperation.None;
}
