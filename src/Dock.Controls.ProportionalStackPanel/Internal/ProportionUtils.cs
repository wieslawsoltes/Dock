// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Dock.Controls.ProportionalStackPanel;

/// <summary>
/// Shared utilities for proportion and control management.
/// </summary>
internal static class ProportionUtils
{
    /// <summary>
    /// Safely clamps a value between min and max bounds.
    /// </summary>
    public static double Clamp(double value, double min, double max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    /// <summary>
    /// Checks if a proportion value is valid (not NaN and non-negative).
    /// </summary>
    public static bool IsValidProportion(double proportion)
    {
        return !double.IsNaN(proportion) && proportion >= 0;
    }

    /// <summary>
    /// Safely converts a dimension to proportion based on available size.
    /// </summary>
    public static double DimensionToProportion(double dimension, double availableSize)
    {
        return availableSize > 0 ? dimension / availableSize : 0;
    }

    /// <summary>
    /// Gets the relevant dimension (width or height) based on orientation.
    /// </summary>
    public static double GetRelevantDimension(Size size, Orientation orientation)
    {
        return orientation == Orientation.Horizontal ? size.Width : size.Height;
    }

    /// <summary>
    /// Gets the relevant min/max size constraint for a control based on orientation.
    /// </summary>
    public static (double Min, double Max) GetSizeConstraints(Control control, Orientation orientation)
    {
        if (orientation == Orientation.Horizontal)
        {
            return (control.MinWidth, control.MaxWidth);
        }
        else
        {
            return (control.MinHeight, control.MaxHeight);
        }
    }

    /// <summary>
    /// Filters children to get only non-splitter controls.
    /// </summary>
    public static IEnumerable<Control> GetNonSplitterChildren(Avalonia.Controls.Controls children)
    {
        return children.OfType<Control>()
            .Where(control => !ProportionalStackPanelSplitter.IsSplitter(control, out _));
    }
}
