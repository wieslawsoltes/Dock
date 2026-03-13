// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Dock.Controls.ProportionalStackPanel;

/// <summary>
/// Handles dimension calculations for proportional layout with constraint support.
/// </summary>
internal class DimensionCalculator
{
    /// <summary>
    /// Calculates the dimension for a child element based on proportion and handles fractional pixels.
    /// </summary>
    /// <param name="dimension">The total available dimension</param>
    /// <param name="proportion">The proportion of space this element should occupy</param>
    /// <param name="sumOfFractions">Running sum of fractional pixels for proper distribution</param>
    /// <returns>The calculated dimension for the child element</returns>
    public static double CalculateDimension(
        double dimension,
        double proportion,
        ref double sumOfFractions)
    {
        var childDimension = dimension * proportion;
        var flooredChildDimension = Math.Floor(childDimension);

        // sums fractions from the division
        sumOfFractions += childDimension - flooredChildDimension;

        // if the sum of fractions made up a whole pixel/pixels, add it to the dimension
        var round = Math.Round(sumOfFractions, 1);
        
        var clamp = Math.Clamp(Math.Floor(sumOfFractions), 1, double.MaxValue);
        if (round - clamp >= 0)
        {
            sumOfFractions -= Math.Round(sumOfFractions);
            return Math.Max(0, flooredChildDimension + 1);
        }

        return Math.Max(0, flooredChildDimension);
    }

    /// <summary>
    /// Calculates dimension with min/max constraints applied.
    /// </summary>
    /// <param name="control">The control to calculate dimension for</param>
    /// <param name="orientation">The layout orientation</param>
    /// <param name="dimension">The total available dimension</param>
    /// <param name="proportion">The proportion of space this element should occupy</param>
    /// <param name="sumOfFractions">Running sum of fractional pixels for proper distribution</param>
    /// <returns>The calculated dimension with constraints applied</returns>
    public static double CalculateDimensionWithConstraints(
        Control control,
        Orientation orientation,
        double dimension,
        double proportion,
        ref double sumOfFractions)
    {
        var calculatedDimension = CalculateDimension(dimension, proportion, ref sumOfFractions);
        
        // Apply min/max constraints to the final calculated dimension
        double min = orientation == Orientation.Horizontal ? control.MinWidth : control.MinHeight;
        double max = orientation == Orientation.Horizontal ? control.MaxWidth : control.MaxHeight;

        if (!double.IsNaN(min) && calculatedDimension < min)
        {
            calculatedDimension = min;
        }
        
        if (!double.IsNaN(max) && !double.IsPositiveInfinity(max) && calculatedDimension > max)
        {
            calculatedDimension = max;
        }

        return calculatedDimension;
    }
}
