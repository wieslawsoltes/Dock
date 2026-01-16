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
                    ProportionalStackPanel.SetCollapsedProportion(info.Control, info.CurrentProportion);
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

    private void ApplyProportions()
    {
        var hasCollapsedChildren = _childInfos.Any(info => info.IsCollapsed);

        foreach (var info in _childInfos)
        {
            var clampedProportion = _constraintHandler.ClampProportion(info.Control, info.TargetProportion);
            ProportionalStackPanel.SetProportion(info.Control, clampedProportion);
            
            if (!info.IsCollapsed && !hasCollapsedChildren)
            {
                ProportionalStackPanel.SetCollapsedProportion(info.Control, clampedProportion);
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
