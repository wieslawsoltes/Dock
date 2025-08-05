// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace Dock.Controls.ProportionalStackPanel;

/// <summary>
/// Helper for navigating and managing panel relationships.
/// </summary>
internal static class PanelNavigationHelper
{
    /// <summary>
    /// Gets the parent ProportionalStackPanel for a control.
    /// </summary>
    public static ProportionalStackPanel? GetParentPanel(Control control)
    {
        if (control.Parent is ContentPresenter presenter)
        {
            if (presenter.GetVisualParent() is ProportionalStackPanel panel)
            {
                return panel;
            }
        }
        else if (control.GetVisualParent() is ProportionalStackPanel psp)
        {
            return psp;
        }

        return null;
    }

    /// <summary>
    /// Gets a sibling control in the specified direction, skipping collapsed and splitter controls.
    /// </summary>
    public static Control? GetSiblingInDirection(Control control, ProportionalStackPanel panel, int direction)
    {
        Debug.Assert(direction == -1 || direction == 1);

        var children = panel.Children;
        int siblingIndex;

        if (control.Parent is ContentPresenter parentContentPresenter)
        {
            siblingIndex = children.IndexOf(parentContentPresenter) + direction;
        }
        else
        {
            siblingIndex = children.IndexOf(control) + direction;
        }

        while (siblingIndex >= 0 && siblingIndex < children.Count &&
               (ProportionalStackPanel.GetIsCollapsed(children[siblingIndex]) || 
                ProportionalStackPanelSplitter.IsSplitter(children[siblingIndex], out _)))
        {
            siblingIndex += direction;
        }

        return siblingIndex >= 0 && siblingIndex < children.Count ? children[siblingIndex] : null;
    }

    /// <summary>
    /// Updates visual state based on panel orientation.
    /// </summary>
    public static void UpdateControlVisualState(Control control, ProportionalStackPanel panel, double thickness, bool isResizingEnabled)
    {
        if (panel.Orientation == Orientation.Vertical)
        {
            control.Height = thickness;
            control.Width = double.NaN;
            control.Cursor = isResizingEnabled
                ? new Cursor(StandardCursorType.SizeNorthSouth)
                : new Cursor(StandardCursorType.Arrow);
        }
        else
        {
            control.Width = thickness;
            control.Height = double.NaN;
            control.Cursor = isResizingEnabled
                ? new Cursor(StandardCursorType.SizeWestEast)
                : new Cursor(StandardCursorType.Arrow);
        }
    }
}