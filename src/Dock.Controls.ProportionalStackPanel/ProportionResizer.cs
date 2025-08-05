// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Dock.Controls.ProportionalStackPanel;

/// <summary>
/// Handles proportion resizing logic between two adjacent elements.
/// </summary>
internal class ProportionResizer
{
    private readonly ProportionalStackPanel _panel;
    private readonly Control _target;
    private readonly Control _neighbor;
    private readonly double _dragDelta;
    private readonly double _minimumProportionSize;
    private readonly double _availableSize;

    public ProportionResizer(ProportionalStackPanel panel, Control target, Control neighbor, double dragDelta, double minimumProportionSize)
    {
        _panel = panel;
        _target = target;
        _neighbor = neighbor;
        _dragDelta = dragDelta;
        _minimumProportionSize = minimumProportionSize;
        _availableSize = panel.Orientation == Orientation.Vertical ? panel.Bounds.Height : panel.Bounds.Width;
    }

    public void ApplyResize()
    {
        var targetProportion = ProportionalStackPanel.GetProportion(_target);
        var neighborProportion = ProportionalStackPanel.GetProportion(_neighbor);

        var deltaProportionRaw = ProportionUtils.DimensionToProportion(_dragDelta, _availableSize);
        var deltaProportionClamped = ClampDeltaProportion(deltaProportionRaw, targetProportion, neighborProportion);

        var newTargetProportion = targetProportion + deltaProportionClamped;
        var newNeighborProportion = neighborProportion - deltaProportionClamped;

        // Apply constraints and adjust proportions accordingly
        ApplyConstraints(ref newTargetProportion, ref newNeighborProportion);

        ProportionalStackPanel.SetProportion(_target, Math.Max(0, newTargetProportion));
        ProportionalStackPanel.SetProportion(_neighbor, Math.Max(0, newNeighborProportion));
    }

    private double ClampDeltaProportion(double delta, double targetProportion, double neighborProportion)
    {
        // Prevent negative proportions
        if (targetProportion + delta < 0)
            delta = -targetProportion;
        if (neighborProportion - delta < 0)
            delta = neighborProportion;
        return delta;
    }

    private void ApplyConstraints(ref double targetProportion, ref double neighborProportion)
    {
        var constraints = new ConstraintCalculator(_panel.Orientation, _availableSize, _minimumProportionSize);
        
        var targetConstraints = constraints.GetConstraints(_target);
        var neighborConstraints = constraints.GetConstraints(_neighbor);

        // Apply constraints in a balanced way
        ApplyConstraintPair(ref targetProportion, ref neighborProportion, targetConstraints, neighborConstraints);
        ApplyConstraintPair(ref neighborProportion, ref targetProportion, neighborConstraints, targetConstraints);
    }

    private static void ApplyConstraintPair(
        ref double primaryProportion, 
        ref double secondaryProportion, 
        (double Min, double Max) primaryConstraints, 
        (double Min, double Max) secondaryConstraints)
    {
        // Apply minimum constraint
        if (primaryProportion < primaryConstraints.Min)
        {
            var deficit = primaryConstraints.Min - primaryProportion;
            primaryProportion = primaryConstraints.Min;
            secondaryProportion = Math.Max(secondaryConstraints.Min, secondaryProportion - deficit);
        }
        // Apply maximum constraint
        else if (primaryProportion > primaryConstraints.Max)
        {
            var excess = primaryProportion - primaryConstraints.Max;
            primaryProportion = primaryConstraints.Max;
            secondaryProportion = Math.Min(secondaryConstraints.Max, secondaryProportion + excess);
        }
    }
}