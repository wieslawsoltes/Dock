// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace Dock.Controls.ProportionalStackPanel;

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

    private void AssignProportions()
    {
        isAssigningProportions = true;
        try
        {
            AssignProportionsInternal(Children);
        }
        finally
        {
            isAssigningProportions = false;
        }
    }

    private static void AssignProportionsInternal(Avalonia.Controls.Controls children)
    {
        var assignedProportion = 0.0;
        var unassignedProportions = 0;

        for (var i = 0; i < children.Count; i++)
        {
            var control = children[i];
            var isCollapsed = GetIsCollapsed(control);
            var isSplitter = ProportionalStackPanelSplitter.IsSplitter(control, out _);

            if (!isSplitter)
            {
                var proportion = GetProportion(control);

                if (isCollapsed)
                {
                    proportion = 0.0;
                }

                if (double.IsNaN(proportion))
                {
                    unassignedProportions++;
                }
                else
                {
                    assignedProportion += proportion;
                }
            }
        }

        if (unassignedProportions > 0)
        {
            var toAssign = assignedProportion;
            foreach (var control in children.Where(c =>
                     {
                         var isCollapsed = GetIsCollapsed(c);
                         return !isCollapsed && double.IsNaN(GetProportion(c));
                     }))
            {
                if (!ProportionalStackPanelSplitter.IsSplitter(control, out _))
                {
                    var proportion = (1.0 - toAssign) / unassignedProportions;
                    SetProportion(control, proportion);
                    assignedProportion += (1.0 - toAssign) / unassignedProportions;
                }
            }
        }

        if (assignedProportion < 1)
        {
            var numChildren = (double)children.Count(c =>
            {
                var isCollapsed = GetIsCollapsed(c);
                return !ProportionalStackPanelSplitter.IsSplitter(c, out _) && !isCollapsed;
            });

            var toAdd = (1.0 - assignedProportion) / numChildren;

            foreach (var child in children.Where(c =>
                     {
                         var isCollapsed = GetIsCollapsed(c);
                         return !isCollapsed && !ProportionalStackPanelSplitter.IsSplitter(c, out _);
                     }))
            {
                var proportion = GetProportion(child) + toAdd;
                SetProportion(child, proportion);
            }
        }
        else if (assignedProportion > 1)
        {
            var numChildren = (double)children.Count(c =>
            {
                var isCollapsed = GetIsCollapsed(c);
                return !ProportionalStackPanelSplitter.IsSplitter(c, out _) && !isCollapsed;
            });

            var toRemove = (assignedProportion - 1.0) / numChildren;

            foreach (var child in children.Where(c =>
                     {
                         var isCollapsed = GetIsCollapsed(c);
                         return !isCollapsed && !ProportionalStackPanelSplitter.IsSplitter(c, out _);
                     }))
            {
                var proportion = GetProportion(child) - toRemove;
                SetProportion(child, proportion);
            }
        }
    }

    private double GetTotalSplitterThickness(Avalonia.Controls.Controls children)
    {
        var previousisCollapsed = false;
        var totalThickness = 0.0;

        for (var i = 0; i < children.Count; i++)
        {
            var c = children[i];
            var isSplitter = ProportionalStackPanelSplitter.IsSplitter(c, out var proportionalStackPanelSplitter);

            if (isSplitter && proportionalStackPanelSplitter is not null)
            {
                if (previousisCollapsed)
                {
                    previousisCollapsed = false;
                    continue;
                }

                if (i + 1 < Children.Count)
                {
                    var nextControl = Children[i + 1];
                    var nextIsCollapsed = GetIsCollapsed(nextControl);
                    if (nextIsCollapsed)
                    {
                        continue;
                    }
                }

                var thickness = proportionalStackPanelSplitter.Thickness;
                totalThickness += thickness;
            }
            else
            {
                previousisCollapsed = GetIsCollapsed(c);
            }
        }

        return double.IsNaN(totalThickness) ? 0 : totalThickness;
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

        AssignProportions();

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
                        var width = CalculateDimension(constraint.Width - splitterThickness, proportion,
                            ref sumOfFractions);
                        var size = constraint.WithWidth(width);
                        control.Measure(size);
                        break;
                    }
                    case Orientation.Vertical:
                    {
                        var height = CalculateDimension(constraint.Height - splitterThickness, proportion,
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
                        usedWidth += CalculateDimension(constraint.Width - splitterThickness, proportion,
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
                        usedHeight += CalculateDimension(constraint.Height - splitterThickness, proportion,
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

        AssignProportions();

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
                            var width = CalculateDimension(arrangeSize.Width - splitterThickness, proportion,
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
                            var height = CalculateDimension(arrangeSize.Height - splitterThickness, proportion,
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

        ProportionProperty.Changed.AddClassHandler<Control>((sender, _) =>
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
