// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Dock.Controls.ProportionalStackPanel;

/// <summary>
/// Handles proportion constraints for controls.
/// </summary>
internal class ProportionConstraintHandler
{
    private readonly Orientation _orientation;
    private readonly double _availableDimension;

    public ProportionConstraintHandler(Orientation orientation, double availableDimension)
    {
        _orientation = orientation;
        _availableDimension = availableDimension;
    }

    public double ClampProportion(Control control, double proportion)
    {
        var (min, max) = ProportionUtils.GetSizeConstraints(control, _orientation);
        
        var minProp = !double.IsNaN(min) && min > 0 ? min / _availableDimension : 0.0;
        var maxProp = !double.IsNaN(max) && !double.IsPositiveInfinity(max) ? max / _availableDimension : double.PositiveInfinity;

#if NETSTANDARD2_0
        return ProportionUtils.Clamp(proportion, minProp, maxProp);
#else
        return Math.Clamp(proportion, minProp, maxProp);
#endif
    }
}