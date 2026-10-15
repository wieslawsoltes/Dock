using Avalonia.Controls;
using Avalonia.Layout;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal static class LayoutGroupLengths
{
    internal static void MakeStars<T>(LayoutPositionableGroup<T> group, Orientation orientation) where T : class, ILayoutPanelElement
    {
        for (int index = 0; index < group.Children.Count; index++)
        {
            if (group.Children[index] is not ILayoutPositionableGroup child) continue;
            if (orientation == Orientation.Horizontal)
            {
                if (!child.DockWidth.IsStar) child.DockWidth = new GridLength(1, GridUnitType.Star);
            }
            else if (!child.DockHeight.IsStar) child.DockHeight = new GridLength(1, GridUnitType.Star);
        }
    }
}
