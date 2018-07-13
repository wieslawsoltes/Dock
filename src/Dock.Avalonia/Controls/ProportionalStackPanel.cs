using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;

namespace Dock.Avalonia.Controls
{
    public class ProportionalStackPanel : StackPanel
    {
        private void AssignProportions()
        {
            double assignedProportion = 0;
            int unassignedProportions = 0;

            foreach (Control element in Children)
            {
                if (!(element is ProportionalStackPanelSplitter))
                {
                    var proportion = ProportionalStackPanelSplitter.GetProportion(element);

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
                foreach (Control element in Children.Where(c => double.IsNaN(ProportionalStackPanelSplitter.GetProportion(c))))
                {
                    if (!(element is ProportionalStackPanelSplitter))
                    {
                        ProportionalStackPanelSplitter.SetProportion(element, (1.0 - assignedProportion) / unassignedProportions);
                    }
                }
            }
        }

        private double GetTotalSplitterThickness ()
        {
            var result = Children.OfType<ProportionalStackPanelSplitter>().Sum(c => c.Thickness);

            return double.IsNaN(result) ? 0 : result;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            double usedWidth = 0.0;
            double usedHeight = 0.0;
            double maximumWidth = 0.0;
            double maximumHeight = 0.0;

            var splitterThickness = GetTotalSplitterThickness();

            AssignProportions();

            // Measure each of the Children
            foreach (Control element in Children)
            {
                // Get the child's desired size
                Size remainingSize = new Size(
                    Math.Max(0.0, constraint.Width - usedWidth - splitterThickness),
                    Math.Max(0.0, constraint.Height - usedHeight - splitterThickness));

                var proportion = ProportionalStackPanelSplitter.GetProportion(element);

                if (!(element is ProportionalStackPanelSplitter))
                { 
                    switch (Orientation)
                    {
                        case Orientation.Horizontal:
                            element.Measure(constraint.WithWidth(Math.Max(0, (constraint.Width - splitterThickness) * proportion)));
                            break;

                        case Orientation.Vertical:
                            element.Measure(constraint.WithHeight(Math.Max(0, (constraint.Height - splitterThickness) * proportion)));
                            break;
                    }
                }
                else
                {
                    element.Measure(remainingSize);
                }
                
                Size desiredSize = element.DesiredSize;

                // Decrease the remaining space for the rest of the children
                switch (Orientation)
                {
                    case Orientation.Horizontal:
                        maximumHeight = Math.Max(maximumHeight, usedHeight + desiredSize.Height);

                        if (element is ProportionalStackPanelSplitter)
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

                        if (element is ProportionalStackPanelSplitter)
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

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            double left = 0.0;
            double top = 0.0;
            double right = 0.0;
            double bottom = 0.0;

            var splitterThickness = GetTotalSplitterThickness();

            // Arrange each of the Children
            var children = Children;
            int index = 0;

            foreach (Control element in children)
            {
                // Determine the remaining space left to arrange the element
                Rect remainingRect = new Rect(
                    left,
                    top,
                    Math.Max(0.0, arrangeSize.Width - left - right),
                    Math.Max(0.0, arrangeSize.Height - top - bottom));

                // Trim the remaining Rect to the docked size of the element
                // (unless the element should fill the remaining space because
                // of LastChildFill)
                if (index < children.Count)
                {
                    var desiredSize = element.DesiredSize;
                    var proportion = ProportionalStackPanelSplitter.GetProportion(element);

                    switch (Orientation)
                    {
                        case Orientation.Horizontal:
                            if (element is ProportionalStackPanelSplitter)
                            {
                                left += desiredSize.Width;
                                remainingRect = remainingRect.WithWidth(desiredSize.Width);
                            }
                            else
                            {
                                remainingRect = remainingRect.WithWidth(Math.Max(0, (arrangeSize.Width - splitterThickness) * proportion));
                                left += Math.Max(0, (arrangeSize.Width - splitterThickness) * proportion);
                            }
                            break;
                        case Orientation.Vertical:
                            if (element is ProportionalStackPanelSplitter)
                            {
                                top += desiredSize.Height;
                                remainingRect = remainingRect.WithHeight(desiredSize.Height);
                            }
                            else
                            {
                                remainingRect = remainingRect.WithHeight(Math.Max(0, (arrangeSize.Height - splitterThickness) * proportion));
                                top += Math.Max(0, (arrangeSize.Height - splitterThickness) * proportion);
                            }
                            break;
                    }
                }

                element.Arrange(remainingRect);
                index++;
            }

            return arrangeSize;
        }
    }
}
