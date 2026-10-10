using System.Collections.Generic;
using Avalonia.Layout;

namespace AvalonDock.Layout;

/// <summary>Traversal helpers retaining AvalonDock's API spelling.</summary>
public static class Extensions
{
    /// <summary>Enumerates all descendants in depth-first order.</summary>
    public static IEnumerable<ILayoutElement> Descendents(this ILayoutElement element)
    {
        if (element is not ILayoutContainer container) yield break;
        foreach (ILayoutElement child in container.Children)
        {
            yield return child;
            foreach (ILayoutElement descendant in child.Descendents()) yield return descendant;
        }
    }
    /// <summary>Finds the closest parent of the requested type.</summary>
    public static T? FindParent<T>(this ILayoutElement element)
    {
        for (ILayoutContainer? parent = element.Parent; parent is not null; parent = parent.Parent)
            if (parent is T match) return match;
        return default;
    }
    /// <summary>Gets the layout root.</summary>
    public static ILayoutRoot? GetRoot(this ILayoutElement element) => element.Root;
    /// <summary>Tests whether a descendant has the requested type.</summary>
    public static bool ContainsChildOfType<T>(this ILayoutContainer element)
    {
        foreach (ILayoutElement descendant in element.Descendents()) if (descendant is T) return true;
        return false;
    }
    /// <summary>Tests whether a descendant has either requested type.</summary>
    public static bool ContainsChildOfType<T, S>(this ILayoutContainer container)
    {
        foreach (ILayoutElement descendant in container.Descendents()) if (descendant is T or S) return true;
        return false;
    }
    /// <summary>Tests whether the container has either requested type.</summary>
    public static bool IsOfType<T, S>(this ILayoutContainer container) => container is T or S;

    /// <summary>Gets the pane's edge from its position in the containing panel.</summary>
    public static AnchorSide GetSide(this ILayoutElement element)
    {
        if (element.Parent is not ILayoutOrientableGroup parent) return AnchorSide.Right;
        LayoutPanel? panel = parent as LayoutPanel ?? parent.FindParent<LayoutPanel>();
        if (panel is null || panel.ChildrenCount == 0) return AnchorSide.Right;
        int index = -1;
        for (int i = 0; i < panel.Children.Count; i++)
        {
            ILayoutPanelElement child = panel.Children[i];
            if (ReferenceEquals(child, element)) { index = i; break; }
            foreach (ILayoutElement descendant in child.Descendents())
            {
                if (ReferenceEquals(descendant, element)) { index = i; break; }
            }
            if (index >= 0) break;
        }
        bool firstHalf = index >= 0 && index < panel.Children.Count / 2d;
        return panel.Orientation == Orientation.Horizontal
            ? firstHalf ? AnchorSide.Left : AnchorSide.Right
            : firstHalf ? AnchorSide.Top : AnchorSide.Bottom;
    }
}
