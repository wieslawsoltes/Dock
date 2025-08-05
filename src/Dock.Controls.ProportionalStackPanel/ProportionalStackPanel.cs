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

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == OrientationProperty)
        {
            InvalidateMeasure();
        }
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
                        var width = DimensionCalculator.CalculateDimensionWithConstraints(control, Orientation.Horizontal, constraint.Width - splitterThickness, proportion,
                            ref sumOfFractions);
                        var size = constraint.WithWidth(width);
                        control.Measure(size);
                        break;
                    }
                    case Orientation.Vertical:
                    {
                        var height = DimensionCalculator.CalculateDimensionWithConstraints(control, Orientation.Vertical, constraint.Height - splitterThickness, proportion,
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
                        usedWidth += DimensionCalculator.CalculateDimensionWithConstraints(control, Orientation.Horizontal, constraint.Width - splitterThickness, proportion,
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
                        usedHeight += DimensionCalculator.CalculateDimensionWithConstraints(control, Orientation.Vertical, constraint.Height - splitterThickness, proportion,
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
                            var width = DimensionCalculator.CalculateDimensionWithConstraints(control, Orientation.Horizontal, arrangeSize.Width - splitterThickness, proportion,
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
                            var height = DimensionCalculator.CalculateDimensionWithConstraints(control, Orientation.Vertical, arrangeSize.Height - splitterThickness, proportion,
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
}
