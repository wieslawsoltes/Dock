using System;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Layout;

namespace Dock.Avalonia.Controls;

/// <summary>
/// A Panel that stacks controls either horizontally or vertically, with proportional resizing.
/// </summary>
public class ProportionalStackPanel : Panel
{
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
        AvaloniaProperty.RegisterAttached<ProportionalStackPanel, Control, double>("Proportion", double.NaN, false, BindingMode.TwoWay);

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
    /// Defines the IsEmpty attached property.
    /// </summary>
    public static readonly AttachedProperty<bool> IsEmptyProperty =
        AvaloniaProperty.RegisterAttached<ProportionalStackPanel, Control, bool>("IsEmpty", false, false, BindingMode.TwoWay);

    /// <summary>
    /// Gets the value of the IsEmpty attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <returns>The IsEmpty attached property.</returns>
    public static bool GetIsEmpty(AvaloniaObject control)
    {
        return control.GetValue(IsEmptyProperty);
    }

    /// <summary>
    /// Sets the value of the IsEmpty attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="value">The value of the IsEmpty property.</param>
    public static void SetIsEmpty(AvaloniaObject control, bool value)
    {
        control.SetValue(IsEmptyProperty, value);
    }

    private void AssignProportions(global::Avalonia.Controls.Controls children)
    {
        var assignedProportion = 0.0;
        var unassignedProportions = 0;

        for (var i = 0; i < children.Count; i++)
        {
            var control = children[i];
            var isEmpty = GetIsEmpty(control);
            var isSplitter = ProportionalStackPanelSplitter.IsSplitter(control, out _);

            if (!isSplitter)
            {
                var proportion = GetProportion(control);

                if (isEmpty)
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
                         var isEmpty = GetIsEmpty(c);
                         return !isEmpty && double.IsNaN(GetProportion(c));
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
            var numChildren = (double)children.Count(c => !ProportionalStackPanelSplitter.IsSplitter(c, out _));

            var toAdd = (1.0 - assignedProportion) / numChildren;

            foreach (var child in children.Where(c =>
                     {
                         var isEmpty = GetIsEmpty(c);
                         return !isEmpty && !ProportionalStackPanelSplitter.IsSplitter(c, out _);
                     }))
            {
                var proportion = GetProportion(child) + toAdd;
                SetProportion(child, proportion);
            }
        }
        else if (assignedProportion > 1)
        {
            var numChildren = (double)children.Count(c => !ProportionalStackPanelSplitter.IsSplitter(c, out _));

            var toRemove = (assignedProportion - 1.0) / numChildren;

            foreach (var child in children.Where(c =>
                     {
                         var isEmpty = GetIsEmpty(c);
                         return !isEmpty && !ProportionalStackPanelSplitter.IsSplitter(c, out _);
                     }))
            {
                var proportion = GetProportion(child) - toRemove;
                SetProportion(child, proportion);
            }
        }
    }

    private double GetTotalSplitterThickness(global::Avalonia.Controls.Controls children)
    {
        var previousIsEmpty = false;
        var totalThickness = 0.0;

        for (var i = 0; i < children.Count; i++)
        {
            var c = children[i];
            var isSplitter = ProportionalStackPanelSplitter.IsSplitter(c, out var proportionalStackPanelSplitter);

            if (isSplitter && proportionalStackPanelSplitter is not null)
            {
                if (previousIsEmpty)
                {
                    previousIsEmpty = false;
                    continue;
                }

                if (i + 1 < Children.Count)
                {
                    var nextControl = Children[i + 1];
                    var nextIsEmpty = GetIsEmpty(nextControl);
                    if (nextIsEmpty)
                    {
                        continue;
                    }
                }
                
                var thickness = proportionalStackPanelSplitter.Thickness;
                totalThickness += thickness;
            }
            else
            {
                previousIsEmpty = GetIsEmpty(c);
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

        AssignProportions(Children);

        var previousIsEmpty = false;
        
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

            var isEmpty = !isSplitter && GetIsEmpty(control);
            if (isEmpty)
            {
                // TODO: Also handle next is empty.
                previousIsEmpty = true;
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
                        var width = Math.Max(0, (constraint.Width - splitterThickness) * proportion);
                        var size = constraint.WithWidth(width);
                        control.Measure(size);
                        break;
                    }
                    case Orientation.Vertical:
                    {
                        var height = Math.Max(0, (constraint.Height - splitterThickness) * proportion);
                        var size = constraint.WithHeight(height);
                        control.Measure(size);
                        break;
                    }
                }
            }
            else
            {
                var nextIsEmpty = false;
                if (i + 1 < Children.Count)
                {
                    var nextControl = Children[i + 1];
                    nextIsEmpty = !ProportionalStackPanelSplitter.IsSplitter(nextControl, out _ ) && GetIsEmpty(nextControl);
                }

                if (previousIsEmpty || nextIsEmpty)
                {
                    var size = new Size();
                    control.Measure(size);
                    previousIsEmpty = true;
                    continue;
                }

                control.Measure(remainingSize);
            }

            previousIsEmpty = false;

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
                        usedWidth += Math.Max(0, (constraint.Width - splitterThickness) * proportion);
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
                        usedHeight += Math.Max(0, (constraint.Height - splitterThickness) * proportion);
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

        AssignProportions(Children);

        var previousIsEmpty = false;

        for (var i = 0; i < Children.Count; i++)
        {
            var control = Children[i];

            var isSplitter = ProportionalStackPanelSplitter.IsSplitter(control, out _);

            var isEmpty = !isSplitter && GetIsEmpty(control);
            if (isEmpty)
            {
                // TODO: Also handle next is empty.
                previousIsEmpty = true;
                var rect = new Rect();
                control.Arrange(rect);
                index++;
                continue;
            }

            var nextIsEmpty = false;
            if (i + 1 < Children.Count)
            {
                var nextControl = Children[i + 1];
                nextIsEmpty = !ProportionalStackPanelSplitter.IsSplitter(nextControl, out _) && GetIsEmpty(nextControl);
            }

            if (isSplitter && (previousIsEmpty || nextIsEmpty))
            {
                var rect = new Rect();
                control.Arrange(rect);
                index++;
                continue;
            }

            previousIsEmpty = false;

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
                            var width = Math.Max(0, (arrangeSize.Width - splitterThickness) * proportion);
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
                            var height = Math.Max(0, (arrangeSize.Height - splitterThickness) * proportion);
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
        AffectsParentMeasure<ProportionalStackPanel>(IsEmptyProperty);
        AffectsParentArrange<ProportionalStackPanel>(IsEmptyProperty);
        AffectsParentMeasure<ProportionalStackPanel>(ProportionProperty);
        AffectsParentArrange<ProportionalStackPanel>(ProportionProperty);
    }
}
