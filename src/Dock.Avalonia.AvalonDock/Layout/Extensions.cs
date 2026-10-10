using System.Collections.Generic;

namespace Xceed.Wpf.AvalonDock.Layout;

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
}
