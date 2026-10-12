// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Dock.Controls.ProportionalStackPanel;

/// <summary>
/// Manages proportion assignment logic for ProportionalStackPanel children.
/// </summary>
internal class ProportionManager
{
    private readonly Avalonia.Controls.Controls _children;
    private readonly double _availableDimension;
    private readonly Orientation _orientation;
    private readonly List<ChildInfo> _childInfos;
    private readonly ProportionConstraintHandler _constraintHandler;

    public ProportionManager(Avalonia.Controls.Controls children, Size size, double splitterThickness, Orientation orientation)
    {
        _children = children;
        _orientation = orientation;
        _availableDimension = Math.Max(1.0, ProportionUtils.GetRelevantDimension(size, orientation) - splitterThickness);
        _childInfos = CollectChildInfo();
        _constraintHandler = new ProportionConstraintHandler(_orientation, _availableDimension);
    }

    public void AssignProportions()
    {
        HandleCollapsedChildren();
        AssignUnassignedProportions();
        NormalizeProportions();
        RedistributeConstrainedProportions();
        ApplyProportions();
    }

    private List<ChildInfo> CollectChildInfo()
    {
        var infos = new List<ChildInfo>();
        foreach (var control in ProportionUtils.GetNonSplitterChildren(_children))
        {
            infos.Add(new ChildInfo(control));
        }
        return infos;
    }

    private void HandleCollapsedChildren()
    {
        foreach (var info in _childInfos)
        {
            if (info.IsCollapsed)
            {
                // Store current proportion before collapsing
                if (ProportionUtils.IsValidProportion(info.CurrentProportion) && info.CurrentProportion > 0)
                {
                    info.Control.SetCurrentValue(ProportionalStackPanel.CollapsedProportionProperty, info.CurrentProportion);
                }
                info.TargetProportion = 0.0;
            }
            else
            {
                // Restore from collapsed state if available
                var stored = ProportionalStackPanel.GetCollapsedProportion(info.Control);
                info.TargetProportion = ProportionUtils.IsValidProportion(stored) ? stored : info.CurrentProportion;
            }
        }
    }

    private void AssignUnassignedProportions()
    {
        var unassignedChildren = _childInfos
            .Where(info => !info.IsCollapsed && !ProportionUtils.IsValidProportion(info.TargetProportion))
            .ToList();
            
        if (unassignedChildren.Count == 0) return;

        var assignedTotal = _childInfos
            .Where(info => ProportionUtils.IsValidProportion(info.TargetProportion))
            .Sum(info => info.TargetProportion);
            
        var remainingProportion = Math.Max(0, 1.0 - assignedTotal);
        var proportionPerChild = remainingProportion / unassignedChildren.Count;

        foreach (var info in unassignedChildren)
        {
            info.TargetProportion = proportionPerChild;
        }
    }

    private void NormalizeProportions()
    {
        var activeChildren = _childInfos.Where(info => !info.IsCollapsed).ToList();
        if (activeChildren.Count == 0) return;

        var totalProportion = activeChildren.Sum(info => info.TargetProportion);
        const double tolerance = 1e-10;
        
        if (Math.Abs(totalProportion - 1.0) < tolerance) return; // Already normalized
        if (totalProportion <= 0) return; // Avoid division by zero

        var scaleFactor = 1.0 / totalProportion;
        foreach (var info in activeChildren)
        {
            info.TargetProportion *= scaleFactor;
        }
    }

    private void RedistributeConstrainedProportions()
    {
        const double tolerance = 1e-10;
        foreach (var info in _childInfos)
        {
            if (!info.IsCollapsed)
                info.TargetProportion = _constraintHandler.ClampProportion(info.Control, info.TargetProportion);
        }

        // Clamping alone changes the total. Re-normalizing on the next layout pass
        // then changes it again, making preview geometry depend on the pass count.
        // Share the excess/deficit among panes that can still shrink/grow. Each
        // iteration either finishes or reaches another bound, so at most N are needed.
        for (var pass = 0; pass <= _childInfos.Count; pass++)
        {
            var total = 0.0;
            foreach (var info in _childInfos)
                if (!info.IsCollapsed) total += info.TargetProportion;
            var remaining = 1.0 - total;
            if (Math.Abs(remaining) < tolerance) return;

            var weight = 0.0;
            var eligible = 0;
            foreach (var info in _childInfos)
            {
                if (info.IsCollapsed) continue;
                var candidate = _constraintHandler.ClampProportion(info.Control, info.TargetProportion + remaining);
                if (Math.Abs(candidate - info.TargetProportion) < tolerance) continue;
                weight += info.TargetProportion;
                eligible++;
            }
            // Infeasible minimums overflow; exhausted maximums leave unused space.
            // Preserve those constraints instead of repeatedly normalizing them away.
            if (eligible == 0) return;
            foreach (var info in _childInfos)
            {
                if (info.IsCollapsed) continue;
                var candidate = _constraintHandler.ClampProportion(info.Control, info.TargetProportion + remaining);
                if (Math.Abs(candidate - info.TargetProportion) < tolerance) continue;
                var share = weight > tolerance ? info.TargetProportion / weight : 1.0 / eligible;
                info.TargetProportion = _constraintHandler.ClampProportion(info.Control, info.TargetProportion + remaining * share);
            }
        }
    }

    private void ApplyProportions()
    {
        var hasCollapsedChildren = _childInfos.Any(info => info.IsCollapsed);

        foreach (var info in _childInfos)
        {
            var clampedProportion = info.TargetProportion;
            // Layout writes must preserve model bindings so later splits/resizes can update the pane.
            info.Control.SetCurrentValue(ProportionalStackPanel.ProportionProperty, clampedProportion);
            
            if (!info.IsCollapsed && !hasCollapsedChildren)
            {
                info.Control.SetCurrentValue(ProportionalStackPanel.CollapsedProportionProperty, clampedProportion);
            }
        }
    }

    private class ChildInfo
    {
        public Control Control { get; }
        public bool IsCollapsed { get; }
        public double CurrentProportion { get; }
        public double TargetProportion { get; set; }

        public ChildInfo(Control control)
        {
            Control = control;
            IsCollapsed = ProportionalStackPanel.GetIsCollapsed(control);
            CurrentProportion = ProportionalStackPanel.GetProportion(control);
            TargetProportion = double.NaN;
        }
    }
}
