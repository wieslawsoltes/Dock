using Avalonia.Layout;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Infrastructure;

internal static class LayoutAnchorablePlacement
{
    internal static void Add(LayoutRoot root, LayoutAnchorable tool, AnchorableShowStrategy strategy)
    {
        bool left = (strategy & AnchorableShowStrategy.Left) != 0;
        bool right = (strategy & AnchorableShowStrategy.Right) != 0;
        bool top = (strategy & AnchorableShowStrategy.Top) != 0;
        bool bottom = (strategy & AnchorableShowStrategy.Bottom) != 0;
        if ((strategy & AnchorableShowStrategy.Most) == 0)
        {
            AnchorSide side = bottom ? AnchorSide.Bottom : top ? AnchorSide.Top : right ? AnchorSide.Right : AnchorSide.Left;
            foreach (ILayoutElement element in root.Descendents())
            {
                if (element is not LayoutAnchorablePane pane || pane.GetSide() != side) continue;
                pane.Children.Add(tool);
                return;
            }
        }

        Orientation direction = left || right ? Orientation.Horizontal : Orientation.Vertical;
        LayoutPanel panel = root.RootPanel;
        if (panel.Orientation != direction && panel.ChildrenCount > 1)
        {
            LayoutPanel outer = new() { Orientation = direction };
            root.RootPanel = outer;
            outer.Children.Add(panel);
            panel = outer;
        }
        panel.Orientation = direction;
        bool atStart = direction == Orientation.Horizontal ? left : top;
        panel.Children.Insert(atStart ? 0 : panel.Children.Count, new LayoutAnchorablePane(tool));
    }
}
