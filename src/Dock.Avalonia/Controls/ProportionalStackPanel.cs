using System;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
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

    private void AssignProportions(global::Avalonia.Controls.Controls children)
    {
        var assignedProportion = 0.0;
        var unassignedProportions = 0;

        for (var i = 0; i < children.Count; i++)
        {
            var control = children[i];
            var isEmpty = ProportionalStackPanelSplitter.GetControlIsEmpty(control);
            var isSplitter = ProportionalStackPanelSplitter.IsSplitter(control, out _);

            if (!isSplitter)
            {
                var proportion = ProportionalStackPanelSplitter.GetControlProportion(control);

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
                         var isEmpty = ProportionalStackPanelSplitter.GetControlIsEmpty(c);
                         return !isEmpty && double.IsNaN(ProportionalStackPanelSplitter.GetControlProportion(c));
                     }))
            {
                if (!ProportionalStackPanelSplitter.IsSplitter(control, out _))
                {
                    var proportion = (1.0 - toAssign) / unassignedProportions;
                    ProportionalStackPanelSplitter.SetControlProportion(control, proportion);
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
                         var isEmpty = ProportionalStackPanelSplitter.GetControlIsEmpty(c);
                         return !isEmpty && !ProportionalStackPanelSplitter.IsSplitter(c, out _);
                     }))
            {
                var proportion = ProportionalStackPanelSplitter.GetControlProportion(child) + toAdd;
                ProportionalStackPanelSplitter.SetControlProportion(child, proportion);
            }
        }
        else if (assignedProportion > 1)
        {
            var numChildren = (double)children.Count(c => !ProportionalStackPanelSplitter.IsSplitter(c, out _));

            var toRemove = (assignedProportion - 1.0) / numChildren;

            foreach (var child in children.Where(c =>
                     {
                         var isEmpty = ProportionalStackPanelSplitter.GetControlIsEmpty(c);
                         return !isEmpty && !ProportionalStackPanelSplitter.IsSplitter(c, out _);
                     }))
            {
                var proportion = ProportionalStackPanelSplitter.GetControlProportion(child) - toRemove;
                ProportionalStackPanelSplitter.SetControlProportion(child, proportion);
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
                    var nextIsEmpty = ProportionalStackPanelSplitter.GetControlIsEmpty(nextControl);
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
                previousIsEmpty = ProportionalStackPanelSplitter.GetControlIsEmpty(c);
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

            var proportion = ProportionalStackPanelSplitter.GetControlProportion(control);

            var isEmpty = ProportionalStackPanelSplitter.GetControlIsEmpty(control);
            if (isEmpty)
            {
                // TODO: Also handle next is empty.
                previousIsEmpty = true;
                control.Measure(new Size());
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
                    nextIsEmpty = ProportionalStackPanelSplitter.GetControlIsEmpty(nextControl);
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

            var isEmpty = ProportionalStackPanelSplitter.GetControlIsEmpty(control);
            if (isEmpty)
            {
                // TODO: Also handle next is empty.
                previousIsEmpty = true;
                var rect = new Rect();
                control.Arrange(rect);
                index++;
                continue;
            }

            var isSplitter = ProportionalStackPanelSplitter.IsSplitter(control, out _);
            
            var nextIsEmpty = false;
            if (i + 1 < Children.Count)
            {
                var nextControl = Children[i + 1];
                nextIsEmpty = ProportionalStackPanelSplitter.GetControlIsEmpty(nextControl);
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
                var proportion = ProportionalStackPanelSplitter.GetControlProportion(control);

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
}
