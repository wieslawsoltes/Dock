// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls;

namespace Dock.Controls.ProportionalStackPanel;

/// <summary>
/// Internal utility class for calculating splitter-related metrics.
/// </summary>
internal static class SplitterCalculator
{
    /// <summary>
    /// Calculates the total thickness of all visible splitters in the children collection.
    /// </summary>
    /// <param name="children">The collection of child controls.</param>
    /// <param name="getIsCollapsed">Function to determine if a control is collapsed.</param>
    /// <returns>The total thickness of visible splitters.</returns>
    public static double GetTotalSplitterThickness(Avalonia.Controls.Controls children, System.Func<Control, bool> getIsCollapsed)
    {
        var totalThickness = 0.0;

        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
            var isSplitter = ProportionalStackPanelSplitter.IsSplitter(child, out var splitter);

            if (isSplitter && splitter is not null && ShouldUseSplitter(i, children, getIsCollapsed))
            {
                totalThickness += splitter.Thickness;
            }
        }

        return double.IsNaN(totalThickness) ? 0 : totalThickness;
    }

    /// <summary>
    /// Determines whether a splitter separates two live children. When collapsed children
    /// or consecutive splitters occur between the live children, only the first splitter
    /// after the preceding live child is used.
    /// </summary>
    /// <param name="splitterIndex">The index of the splitter in the children collection.</param>
    /// <param name="children">The collection of child controls.</param>
    /// <param name="getIsCollapsed">Function to determine if a control is collapsed.</param>
    /// <returns><c>true</c> when the splitter should participate in layout; otherwise <c>false</c>.</returns>
    public static bool ShouldUseSplitter(
        int splitterIndex,
        Avalonia.Controls.Controls children,
        System.Func<Control, bool> getIsCollapsed)
    {
        if (splitterIndex < 0
            || splitterIndex >= children.Count
            || !ProportionalStackPanelSplitter.IsSplitter(children[splitterIndex], out _))
        {
            return false;
        }

        var hasPreviousLiveChild = false;
        for (var i = splitterIndex - 1; i >= 0; i--)
        {
            var child = children[i];
            if (ProportionalStackPanelSplitter.IsSplitter(child, out _))
            {
                return false;
            }

            if (!getIsCollapsed(child))
            {
                hasPreviousLiveChild = true;
                break;
            }
        }

        if (!hasPreviousLiveChild)
        {
            return false;
        }

        for (var i = splitterIndex + 1; i < children.Count; i++)
        {
            var child = children[i];
            if (!ProportionalStackPanelSplitter.IsSplitter(child, out _)
                && !getIsCollapsed(child))
            {
                return true;
            }
        }

        return false;
    }
}
