using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;

namespace Dock.ProportionalStackPanel;

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

    internal List<Control?> GetChildren()
    {
        return Children.OfType<Control>().Select(c =>
        {
            if (c is ContentPresenter cp)
            {
                cp.UpdateChild();
                return (Control?)cp.Child;
            }
            return (Control?)c;
        }).ToList();
    }

    // /// <inheritdoc/>
    // protected override void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    // {
    //     base.ChildrenChanged(sender, e);
    //     switch (e.Action)
    //     {
    //         case NotifyCollectionChangedAction.Add:
    //             if (e.NewItems is not null)
    //             {
    //                 foreach (var item in e.NewItems.OfType<Control>())
    //                 {
    //                     ProportionalStackPanelSplitter.SetProportion(item, double.NaN);
    //                 }
    //             }
    //             break;
    //     }
    // }

    private void AssignProportions(IList<Control?> children)
    {
        var assignedProportion = 0.0;
        var unassignedProportions = 0;

        foreach (var control in children)
        {
            if (control is { } and not ProportionalStackPanelSplitter)
            {
                var proportion = ProportionalStackPanelSplitter.GetProportion(control);

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
            foreach (var control in children.Where(c => c is { } && double.IsNaN(ProportionalStackPanelSplitter.GetProportion(c))))
            {
                if (control is { } and not ProportionalStackPanelSplitter)
                {
                    var proportion = (1.0 - toAssign) / unassignedProportions;
                    ProportionalStackPanelSplitter.SetProportion(control, proportion);
                    assignedProportion += (1.0 - toAssign) / unassignedProportions;
                }
            }
        }

        if (assignedProportion < 1)
        {
            var numChildren = (double)children.Count(c => c is not ProportionalStackPanelSplitter);

            var toAdd = (1.0 - assignedProportion) / numChildren;

            foreach (var child in children.Where(c => c is not ProportionalStackPanelSplitter))
            {
                if (child is { })
                {
                    var proportion = ProportionalStackPanelSplitter.GetProportion(child) + toAdd;
                    ProportionalStackPanelSplitter.SetProportion(child, proportion);
                }
            }
        }
        else if (assignedProportion > 1)
        {
            var numChildren = (double)children.Count(c => c is not ProportionalStackPanelSplitter);

            var toRemove = (assignedProportion - 1.0) / numChildren;

            foreach (var child in children.Where(c => c is not ProportionalStackPanelSplitter))
            {
                if (child is { })
                {
                    var proportion = ProportionalStackPanelSplitter.GetProportion(child) - toRemove;
                    ProportionalStackPanelSplitter.SetProportion(child, proportion);
                }
            }
        }
    }

    private double GetTotalSplitterThickness(IList<Control?> children)
    {
        var result = children.OfType<ProportionalStackPanelSplitter>().Sum(c => c.Thickness);

        return double.IsNaN(result) ? 0 : result;
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
        var children = GetChildren();
        var splitterThickness = GetTotalSplitterThickness(children);

        AssignProportions(children);

        // Measure each of the Children
        foreach (var control in children)
        {
            if (control is null)
            {
                continue;
            }
            
            // Get the child's desired size
            var remainingSize = new Size(
                Math.Max(0.0, constraint.Width - usedWidth - splitterThickness),
                Math.Max(0.0, constraint.Height - usedHeight - splitterThickness));

            var proportion = ProportionalStackPanelSplitter.GetProportion(control);

            if (control is not ProportionalStackPanelSplitter)
            {
                Debug.Assert(!double.IsNaN(proportion));

                switch (Orientation)
                {
                    case Orientation.Horizontal:
                        control.Measure(constraint.WithWidth(Math.Max(0, (constraint.Width - splitterThickness) * proportion)));
                        break;

                    case Orientation.Vertical:
                        control.Measure(constraint.WithHeight(Math.Max(0, (constraint.Height - splitterThickness) * proportion)));
                        break;
                }
            }
            else
            {
                control.Measure(remainingSize);
            }

            var desiredSize = control.DesiredSize;

            // Decrease the remaining space for the rest of the children
            switch (Orientation)
            {
                case Orientation.Horizontal:
                    maximumHeight = Math.Max(maximumHeight, usedHeight + desiredSize.Height);

                    if (control is ProportionalStackPanelSplitter)
                    {
                        usedWidth += desiredSize.Width;
                    }
                    else
                    {
                        usedWidth += Math.Max(0, (constraint.Width - splitterThickness) * proportion);
                    }
                    break;
                case Orientation.Vertical:
                    maximumWidth = Math.Max(maximumWidth, usedWidth + desiredSize.Width);

                    if (control is ProportionalStackPanelSplitter)
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
        var children = GetChildren();
        var splitterThickness = GetTotalSplitterThickness(children);
        var index = 0;

        AssignProportions(children);

        foreach (var control in children)
        {
            if (control is null)
            {
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
            if (index < children.Count)
            {
                var desiredSize = control.DesiredSize;
                var proportion = ProportionalStackPanelSplitter.GetProportion(control);

                switch (Orientation)
                {
                    case Orientation.Horizontal:
                        if (control is ProportionalStackPanelSplitter)
                        {
                            left += desiredSize.Width;
                            remainingRect = remainingRect.WithWidth(desiredSize.Width);
                        }
                        else
                        {
                            Debug.Assert(!double.IsNaN(proportion));
                            remainingRect = remainingRect.WithWidth(Math.Max(0, (arrangeSize.Width - splitterThickness) * proportion));
                            left += Math.Max(0, (arrangeSize.Width - splitterThickness) * proportion);
                        }
                        break;
                    case Orientation.Vertical:
                        if (control is ProportionalStackPanelSplitter)
                        {
                            top += desiredSize.Height;
                            remainingRect = remainingRect.WithHeight(desiredSize.Height);
                        }
                        else
                        {
                            Debug.Assert(!double.IsNaN(proportion));
                            remainingRect = remainingRect.WithHeight(Math.Max(0, (arrangeSize.Height - splitterThickness) * proportion));
                            top += Math.Max(0, (arrangeSize.Height - splitterThickness) * proportion);
                        }
                        break;
                }
            }

            control.Arrange(remainingRect);
            index++;
        }

        return arrangeSize;
    }
}
