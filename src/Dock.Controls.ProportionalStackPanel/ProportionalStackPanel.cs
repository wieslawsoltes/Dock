// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.VisualTree;

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

/// <summary>
/// A Panel that stacks controls either horizontally or vertically, with proportional resizing.
/// </summary>
public class ProportionalStackPanel : Panel
{
    private bool isAssigningProportions;

    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<ProportionalStackPanel, Orientation>(nameof(Orientation), Orientation.Vertical);

    /// <summary>
    /// Gets or sets the orientation in which child controls will be arranged out.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Defines the Proportion attached property.
    /// </summary>
    public static readonly AttachedProperty<double> ProportionProperty =
        AvaloniaProperty.RegisterAttached<ProportionalStackPanel, Control, double>("Proportion", double.NaN, false,
            BindingMode.TwoWay);

    /// <summary>
    /// Gets the value of the Proportion attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <returns>The Proportion attached property.</returns>
    public static double GetProportion(AvaloniaObject control)
    {
        return control.GetValue(ProportionProperty);
    }

    /// <summary>
    /// Sets the value of the Proportion attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="value">The value of the Proportion property.</param>
    public static void SetProportion(AvaloniaObject control, double value)
    {
        control.SetValue(ProportionProperty, value);
    }

    /// <summary>
    /// Defines the IsCollapsed attached property.
    /// </summary>
    public static readonly AttachedProperty<bool> IsCollapsedProperty =
        AvaloniaProperty.RegisterAttached<ProportionalStackPanel, Control, bool>("IsCollapsed", false, false,
            BindingMode.TwoWay);

    /// <summary>
    /// Defines the CollapsedProportion attached property.
    /// </summary>
    public static readonly AttachedProperty<double> CollapsedProportionProperty =
        AvaloniaProperty.RegisterAttached<ProportionalStackPanel, Control, double>(
            "CollapsedProportion", double.NaN, false,
            BindingMode.TwoWay);

    /// <summary>
    /// Gets the value of the CollapsedProportion attached property on the specified control.
    /// </summary>
    public static double GetCollapsedProportion(AvaloniaObject control)
    {
        return control.GetValue(CollapsedProportionProperty);
    }

    /// <summary>
    /// Sets the value of the CollapsedProportion attached property on the specified control.
    /// </summary>
    public static void SetCollapsedProportion(AvaloniaObject control, double value)
    {
        control.SetValue(CollapsedProportionProperty, value);
    }

    /// <summary>
    /// Gets the value of the IsCollapsed attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <returns>The IsCollapsed attached property.</returns>
    public static bool GetIsCollapsed(AvaloniaObject control)
    {
        return control.GetValue(IsCollapsedProperty);
    }

    /// <summary>
    /// Sets the value of the IsCollapsed attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="value">The value of the IsCollapsed property.</param>
    public static void SetIsCollapsed(AvaloniaObject control, bool value)
    {
        control.SetValue(IsCollapsedProperty, value);
    }

    private void AssignProportions(Size size, double splitterThickness)
    {
        isAssigningProportions = true;
        try
        {
            var proportionManager = new ProportionManager(Children, size, splitterThickness, Orientation);
            proportionManager.AssignProportions();
        }
        finally
        {
            isAssigningProportions = false;
        }
    }

    /// <summary>
    /// Manages proportion assignment logic for ProportionalStackPanel children.
    /// </summary>
    private class ProportionManager
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
                        SetCollapsedProportion(info.Control, info.CurrentProportion);
                    }
                    info.TargetProportion = 0.0;
                }
                else
                {
                    // Restore from collapsed state if available
                    var stored = GetCollapsedProportion(info.Control);
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
            foreach (var info in _childInfos)
            {
                var clampedProportion = _constraintHandler.ClampProportion(info.Control, info.TargetProportion);
                SetProportion(info.Control, clampedProportion);
                
                if (!info.IsCollapsed)
                {
                    SetCollapsedProportion(info.Control, clampedProportion);
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
                IsCollapsed = GetIsCollapsed(control);
                CurrentProportion = GetProportion(control);
                TargetProportion = double.NaN;
            }
        }
    }

    /// <summary>
    /// Handles proportion constraints for controls.
    /// </summary>
    private class ProportionConstraintHandler
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

    private double GetTotalSplitterThickness(Avalonia.Controls.Controls children)
    {
        var totalThickness = 0.0;
        var previousWasCollapsed = false;

        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
            var isSplitter = ProportionalStackPanelSplitter.IsSplitter(child, out var splitter);

            if (isSplitter && splitter is not null)
            {
                // Skip splitters adjacent to collapsed elements
                if (ShouldSkipSplitter(i, children, previousWasCollapsed))
                {
                    continue;
                }

                totalThickness += splitter.Thickness;
            }
            else
            {
                previousWasCollapsed = GetIsCollapsed(child);
            }
        }

        return double.IsNaN(totalThickness) ? 0 : totalThickness;
    }

    private bool ShouldSkipSplitter(int splitterIndex, Avalonia.Controls.Controls children, bool previousWasCollapsed)
    {
        // Skip if previous element was collapsed
        if (previousWasCollapsed)
        {
            return true;
        }

        // Skip if next element is collapsed
        if (splitterIndex + 1 < children.Count)
        {
            var nextChild = children[splitterIndex + 1];
            if (GetIsCollapsed(nextChild))
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size constraint)
    {
        var horizontal = Orientation == Orientation.Horizontal;

        if (constraint == Size.Infinity
            || (horizontal && double.IsInfinity(constraint.Width))
            || (!horizontal && double.IsInfinity(constraint.Height)))
        {
            throw new Exception("Proportional StackPanel cannot be inside a control that offers infinite space.");
        }

        var usedWidth = 0.0;
        var usedHeight = 0.0;
        var maximumWidth = 0.0;
        var maximumHeight = 0.0;
        var splitterThickness = GetTotalSplitterThickness(Children);

        AssignProportions(constraint, splitterThickness);

        var needsNextSplitter = false;
        double sumOfFractions = 0;

        // Measure each of the Children
        for (var i = 0; i < Children.Count; i++)
        {
            var control = Children[i];
            var isSplitter = ProportionalStackPanelSplitter.IsSplitter(control, out _);

            // Get the child's desired size
            var remainingSize = new Size(
                Math.Max(0.0, constraint.Width - usedWidth - splitterThickness),
                Math.Max(0.0, constraint.Height - usedHeight - splitterThickness));

            var proportion = GetProportion(control);

            var isCollapsed = !isSplitter && GetIsCollapsed(control);
            if (isCollapsed)
            {
                var size = new Size();
                control.Measure(size);
                continue;
            }

            if (!isSplitter)
            {
                Debug.Assert(!double.IsNaN(proportion));

                switch (Orientation)
                {
                    case Orientation.Horizontal:
                    {
                        var width = CalculateDimensionWithConstraints(control, Orientation.Horizontal, constraint.Width - splitterThickness, proportion,
                            ref sumOfFractions);
                        var size = constraint.WithWidth(width);
                        control.Measure(size);
                        break;
                    }
                    case Orientation.Vertical:
                    {
                        var height = CalculateDimensionWithConstraints(control, Orientation.Vertical, constraint.Height - splitterThickness, proportion,
                            ref sumOfFractions);
                        var size = constraint.WithHeight(height);
                        control.Measure(size);
                        break;
                    }
                }

                needsNextSplitter = true;
            }
            else
            {
                if (!needsNextSplitter)
                {
                    var size = new Size();
                    control.Measure(size);
                    continue;
                }

                control.Measure(remainingSize);
                needsNextSplitter = false;
            }

            var desiredSize = control.DesiredSize;

            // Decrease the remaining space for the rest of the children
            switch (Orientation)
            {
                case Orientation.Horizontal:
                {
                    maximumHeight = Math.Max(maximumHeight, usedHeight + desiredSize.Height);

                    if (isSplitter)
                    {
                        usedWidth += desiredSize.Width;
                    }
                    else
                    {
                        usedWidth += CalculateDimensionWithConstraints(control, Orientation.Horizontal, constraint.Width - splitterThickness, proportion,
                            ref sumOfFractions);
                    }

                    break;
                }
                case Orientation.Vertical:
                {
                    maximumWidth = Math.Max(maximumWidth, usedWidth + desiredSize.Width);

                    if (isSplitter)
                    {
                        usedHeight += desiredSize.Height;
                    }
                    else
                    {
                        usedHeight += CalculateDimensionWithConstraints(control, Orientation.Vertical, constraint.Height - splitterThickness, proportion,
                            ref sumOfFractions);
                    }

                    break;
                }
            }
        }

        maximumWidth = Math.Max(maximumWidth, usedWidth);
        maximumHeight = Math.Max(maximumHeight, usedHeight);

        return new Size(maximumWidth, maximumHeight);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size arrangeSize)
    {
        var left = 0.0;
        var top = 0.0;
        var right = 0.0;
        var bottom = 0.0;

        // Arrange each of the Children
        var splitterThickness = GetTotalSplitterThickness(Children);
        var index = 0;

        AssignProportions(arrangeSize, splitterThickness);

        var needsNextSplitter = false;
        double sumOfFractions = 0;

        for (var i = 0; i < Children.Count; i++)
        {
            var control = Children[i];

            var isSplitter = ProportionalStackPanelSplitter.IsSplitter(control, out _);

            var isCollapsed = !isSplitter && GetIsCollapsed(control);
            if (isCollapsed)
            {
                var rect = new Rect();
                control.Arrange(rect);
                index++;
                continue;
            }

            if (!isSplitter)
                needsNextSplitter = true;
            else if (isSplitter && !needsNextSplitter)
            {
                var rect = new Rect();
                control.Arrange(rect);
                index++;
                needsNextSplitter = false;
                continue;
            }

            // Determine the remaining space left to arrange the element
            var remainingRect = new Rect(
                left,
                top,
                Math.Max(0.0, arrangeSize.Width - left - right),
                Math.Max(0.0, arrangeSize.Height - top - bottom));

            // Trim the remaining Rect to the docked size of the element
            // (unless the element should fill the remaining space because
            // of LastChildFill)
            if (index < Children.Count)
            {
                var desiredSize = control.DesiredSize;
                var proportion = GetProportion(control);

                switch (Orientation)
                {
                    case Orientation.Horizontal:
                    {
                        if (isSplitter)
                        {
                            left += desiredSize.Width;
                            remainingRect = remainingRect.WithWidth(desiredSize.Width);
                        }
                        else
                        {
                            Debug.Assert(!double.IsNaN(proportion));
                            var width = CalculateDimensionWithConstraints(control, Orientation.Horizontal, arrangeSize.Width - splitterThickness, proportion,
                                ref sumOfFractions);
                            remainingRect = remainingRect.WithWidth(width);
                            left += width;
                        }

                        break;
                    }
                    case Orientation.Vertical:
                    {
                        if (isSplitter)
                        {
                            top += desiredSize.Height;
                            remainingRect = remainingRect.WithHeight(desiredSize.Height);
                        }
                        else
                        {
                            Debug.Assert(!double.IsNaN(proportion));
                            var height = CalculateDimensionWithConstraints(control, Orientation.Vertical, arrangeSize.Height - splitterThickness, proportion,
                                ref sumOfFractions);
                            remainingRect = remainingRect.WithHeight(height);
                            top += height;
                        }

                        break;
                    }
                }
            }

            control.Arrange(remainingRect);
            index++;
        }

        return arrangeSize;
    }

    private double CalculateDimension(
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
        
#if NETSTANDARD2_0
        var clamp = Clamp(Math.Floor(sumOfFractions), 1, double.MaxValue);
#else
        var clamp = Math.Clamp(Math.Floor(sumOfFractions), 1, double.MaxValue);
#endif
        if (round - clamp >= 0)
        {
            sumOfFractions -= Math.Round(sumOfFractions);
            return Math.Max(0, flooredChildDimension + 1);
        }

        return Math.Max(0, flooredChildDimension);

#if NETSTANDARD2_0
        static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0)
                return min;
            if (value.CompareTo(max) > 0)
                return max;
            return value;
        }
#endif
    }

    private double CalculateDimensionWithConstraints(
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

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == OrientationProperty)
        {
            InvalidateMeasure();
        }
    }

    static ProportionalStackPanel()
    {
        AffectsParentMeasure<ProportionalStackPanel>(IsCollapsedProperty);
        AffectsParentArrange<ProportionalStackPanel>(IsCollapsedProperty);

        ProportionProperty.Changed.AddClassHandler<Control>((sender, e) =>
        {
            if (sender.GetVisualParent() is not ProportionalStackPanel parent)
                return;

            if (parent.isAssigningProportions)
                return;

            if (!GetIsCollapsed(sender) && e.NewValue is double value && !double.IsNaN(value))
            {
                SetCollapsedProportion(sender, value);
            }

            parent.InvalidateMeasure();
            parent.InvalidateArrange();
        });

        CollapsedProportionProperty.Changed.AddClassHandler<Control>((sender, _) =>
        {
            if (sender.GetVisualParent() is not ProportionalStackPanel parent)
                return;

            if (parent.isAssigningProportions)
                return;

            parent.InvalidateMeasure();
            parent.InvalidateArrange();
        });
    }
}
