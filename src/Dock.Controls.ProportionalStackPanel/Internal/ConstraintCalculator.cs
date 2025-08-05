// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Dock.Controls.ProportionalStackPanel;

/// <summary>
/// Calculates min/max proportion constraints for a control.
/// </summary>
internal class ConstraintCalculator
{
    private readonly Orientation _orientation;
    private readonly double _availableSize;
    private readonly double _minimumProportionSize;

    public ConstraintCalculator(Orientation orientation, double availableSize, double minimumProportionSize)
    {
        _orientation = orientation;
        _availableSize = availableSize;
        _minimumProportionSize = minimumProportionSize;
    }

    public (double MinProportion, double MaxProportion) GetConstraints(Control control)
    {
        var (minSize, maxSize) = ProportionUtils.GetSizeConstraints(control, _orientation);

        var minProportion = ProportionUtils.DimensionToProportion(_minimumProportionSize, _availableSize);
        var maxProportion = double.PositiveInfinity;

        if (!double.IsNaN(minSize) && minSize > 0)
        {
            var sizeBasedMin = ProportionUtils.DimensionToProportion(minSize, _availableSize);
            minProportion = Math.Max(minProportion, sizeBasedMin);
        }

        if (!double.IsNaN(maxSize) && !double.IsPositiveInfinity(maxSize))
        {
            maxProportion = ProportionUtils.DimensionToProportion(maxSize, _availableSize);
        }

        return (minProportion, maxProportion);
    }
}