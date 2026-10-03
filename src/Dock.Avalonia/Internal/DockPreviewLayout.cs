using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

/// <summary>Measures a detached docking projection with the same panel used by the live layout.</summary>
internal static class DockPreviewLayout
{
    internal static Rect? Measure(DockSplitPreview preview, Size size, double splitterThickness = 4)
    {
        var controls = new Dictionary<IDockable, Control>();
        var layout = Build(preview.Layout, controls, splitterThickness);
        if (layout is null || !controls.TryGetValue(preview.InsertedDock, out var inserted)) return null;
        layout.Measure(size);
        layout.Arrange(new Rect(size));
        var position = inserted.TranslatePoint(default, layout);
        return position.HasValue ? new Rect(position.Value, inserted.Bounds.Size) : null;
    }

    private static Control? Build(IDockable model, Dictionary<IDockable, Control> controls, double splitterThickness)
    {
        Control control;
        if (model is IProportionalDock proportional)
        {
            var panel = new ProportionalStackPanel
            {
                Orientation = proportional.Orientation == Orientation.Horizontal
                    ? global::Avalonia.Layout.Orientation.Horizontal : global::Avalonia.Layout.Orientation.Vertical
            };
            if (proportional.VisibleDockables is { } children)
            {
                foreach (var child in children)
                {
                    var childControl = Build(child, controls, splitterThickness);
                    if (childControl is null) return null;
                    panel.Children.Add(childControl);
                }
            }
            control = panel;
        }
        else if (model is IRootDock root && root.VisibleDockables is { Count: 1 } visible)
        {
            var child = Build(visible[0], controls, splitterThickness);
            if (child is null) return null;
            control = new Border { Child = child };
        }
        else if (model is IProportionalDockSplitter)
            control = new ProportionalStackPanelSplitter { Thickness = splitterThickness };
        else if (model is IToolDock or IDocumentDock)
            control = new Border();
        else return null;

        control.MinWidth = double.IsNaN(model.MinWidth) ? 0 : model.MinWidth;
        control.MinHeight = double.IsNaN(model.MinHeight) ? 0 : model.MinHeight;
        control.MaxWidth = double.IsNaN(model.MaxWidth) ? double.PositiveInfinity : model.MaxWidth;
        control.MaxHeight = double.IsNaN(model.MaxHeight) ? double.PositiveInfinity : model.MaxHeight;
        ProportionalStackPanel.SetProportion(control, model.Proportion);
        ProportionalStackPanel.SetCollapsedProportion(control, model.CollapsedProportion);
        ProportionalStackPanel.SetIsCollapsed(control, model.IsCollapsable && model.IsEmpty);
        controls.Add(model, control);
        return control;
    }
}
