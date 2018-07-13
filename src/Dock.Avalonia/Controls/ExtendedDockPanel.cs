using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;

namespace Dock.Avalonia.Controls
{
    public class ExtendedDockPanel : DockPanel
    {
        private void AssignProportions()
        {
            double assignedProportion = 0;
            int unassignedProportions = 0;

            foreach (Control element in Children)
            {
                if (!(element is DockPanelSplitter))
                {
                    var proportion = DockPanelSplitter.GetProportion(element);

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
                foreach (Control element in Children.Where(c => double.IsNaN(DockPanelSplitter.GetProportion(c))))
                {
                    if (!(element is DockPanelSplitter))
                    {
                        DockPanelSplitter.SetProportion(element, (1.0 - assignedProportion) / unassignedProportions);
                    }
                }
            }
        }

        private double GetTotalSplitterWidth ()
        {
            var result = Children.OfType<DockPanelSplitter>().Sum(c => c.Width);

            return double.IsNaN(result) ? 0 : result;
        }

        private double GetTotalSplitterHeight ()
        {
            var result = Children.OfType<DockPanelSplitter>().Sum(c => c.Height);

            return double.IsNaN(result) ? 0 : result;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            double usedWidth = 0.0;
            double usedHeight = 0.0;
            double maximumWidth = 0.0;
            double maximumHeight = 0.0;

            var splitterHeight = GetTotalSplitterHeight();
            var splitterWidth = GetTotalSplitterWidth();

            AssignProportions();

            // Measure each of the Children
            foreach (Control element in Children)
            {
                // Get the child's desired size
                Size remainingSize = new Size(
                    Math.Max(0.0, constraint.Width - usedWidth - splitterWidth),
                    Math.Max(0.0, constraint.Height - usedHeight - splitterHeight));

                var proportion = DockPanelSplitter.GetProportion(element);

                if (!(element is DockPanelSplitter))
                { 
                    switch (GetDock(element))
                    {
                        case global::Avalonia.Controls.Dock.Left:
                        case global::Avalonia.Controls.Dock.Right:
                            element.Measure(constraint.WithWidth(Math.Max(0, (constraint.Width - splitterWidth) * proportion)));
                            break;
                        case global::Avalonia.Controls.Dock.Top:
                        case global::Avalonia.Controls.Dock.Bottom:
                            element.Measure(constraint.WithHeight(Math.Max(0, (constraint.Height - splitterHeight) * proportion)));
                            break;
                    }
                }
                else
                {
                    element.Measure(remainingSize);
                }
                
                Size desiredSize = element.DesiredSize;

                // Decrease the remaining space for the rest of the children
                switch (GetDock(element))
                {
                    case global::Avalonia.Controls.Dock.Left:
                    case global::Avalonia.Controls.Dock.Right:
                        maximumHeight = Math.Max(maximumHeight, usedHeight + desiredSize.Height);

                        if (element is DockPanelSplitter)
                        {
                            usedWidth += desiredSize.Width;
                        }
                        else
                        {
                            usedWidth += Math.Max(0, (constraint.Width - splitterWidth) * proportion);
                        }
                        break;
                    case global::Avalonia.Controls.Dock.Top:
                    case global::Avalonia.Controls.Dock.Bottom:
                        maximumWidth = Math.Max(maximumWidth, usedWidth + desiredSize.Width);

                        if (element is DockPanelSplitter)
                        {    
                            usedHeight += desiredSize.Height;
                        }
                        else
                        {
                            usedHeight += Math.Max(0, (constraint.Height - splitterHeight) * proportion);
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

            var splitterHeight = GetTotalSplitterHeight();
            var splitterWidth = GetTotalSplitterWidth();

            // Arrange each of the Children
            var children = Children;
            int dockedCount = children.Count - (LastChildFill ? 1 : 0);
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
                if (index < dockedCount)
                {
                    var desiredSize = element.DesiredSize;
                    var proportion = DockPanelSplitter.GetProportion(element);

                    switch (GetDock(element))
                    {
                        case global::Avalonia.Controls.Dock.Left:
                            left += desiredSize.Width;
                            remainingRect = remainingRect.WithWidth(desiredSize.Width);
                            break;
                        case global::Avalonia.Controls.Dock.Top:
                            if (element is DockPanelSplitter)
                            {
                                top += desiredSize.Height;
                                remainingRect = remainingRect.WithHeight(desiredSize.Height);
                            }
                            else
                            {
                                remainingRect = remainingRect.WithHeight(Math.Max(0, (arrangeSize.Height - splitterHeight) * proportion));
                                top += Math.Max(0, (arrangeSize.Height - splitterHeight) * proportion);
                            }
                            break;
                        case global::Avalonia.Controls.Dock.Right:
                            right += desiredSize.Width;
                            remainingRect = new Rect(
                                Math.Max(0.0, arrangeSize.Width - right),
                                remainingRect.Y,
                                desiredSize.Width,
                                remainingRect.Height);
                            break;
                        case global::Avalonia.Controls.Dock.Bottom:
                            bottom += desiredSize.Height;
                            remainingRect = new Rect(
                                remainingRect.X,
                                Math.Max(0.0, arrangeSize.Height - bottom),
                                remainingRect.Width,
                                desiredSize.Height);
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
