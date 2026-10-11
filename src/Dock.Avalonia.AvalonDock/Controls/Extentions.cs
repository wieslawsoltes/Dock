using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace AvalonDock.Controls;

/// <summary>Provides AvalonDock's visual and logical tree helpers over Avalonia objects.</summary>
/// <remarks>The spelling of this type is retained from the upstream public API.</remarks>
public static class Extentions
{
    /// <summary>Enumerates matching visual descendants in depth-first order, excluding the starting object.</summary>
    /// <typeparam name="T">The descendant type.</typeparam>
    /// <param name="depObj">The visual to search, or null for an empty sequence.</param>
    /// <returns>The matching descendants in visual child order.</returns>
    /// <exception cref="InvalidOperationException">A non-null starting object is not a visual.</exception>
    public static IEnumerable<T> FindVisualChildren<T>(this AvaloniaObject? depObj) where T : AvaloniaObject
    {
        if (depObj is null) yield break;
        foreach (Visual descendant in RequireVisual(depObj).GetVisualDescendants())
            if (descendant is T match) yield return match;
    }

    /// <summary>Enumerates matching logical descendants in depth-first order, excluding the starting object.</summary>
    /// <typeparam name="T">The descendant type.</typeparam>
    /// <param name="depObj">The object to search, or null for an empty sequence.</param>
    /// <returns>The matching descendants in logical child order.</returns>
    public static IEnumerable<T> FindLogicalChildren<T>(this AvaloniaObject? depObj) where T : AvaloniaObject
    {
        if (depObj is not ILogical logical) yield break;
        depObj.VerifyAccess();
        foreach (ILogical descendant in logical.GetLogicalDescendants())
            if (descendant is T match) yield return match;
    }

    /// <summary>Finds the topmost visual, using logical parents to reach the visual tree from a nonvisual object.</summary>
    /// <param name="initial">The starting object, or null.</param>
    /// <returns>The topmost reachable object, or null for a null starting object.</returns>
    public static AvaloniaObject? FindVisualTreeRoot(this AvaloniaObject? initial)
    {
        AvaloniaObject? root = initial;
        while (root is not null)
        {
            root.VerifyAccess();
            AvaloniaObject? parent = root is Visual visual ? visual.GetVisualParent() : (root as ILogical)?.LogicalParent as AvaloniaObject;
            if (parent is null) break;
            root = parent;
        }
        return root;
    }

    /// <summary>Finds the nearest matching visual ancestor, excluding the starting visual.</summary>
    /// <typeparam name="T">The ancestor class or interface.</typeparam>
    /// <param name="dependencyObject">The visual whose ancestors are searched.</param>
    /// <returns>The nearest matching ancestor, or null.</returns>
    /// <exception cref="ArgumentNullException">The starting object is null.</exception>
    /// <exception cref="InvalidOperationException">The starting object is not a visual.</exception>
    public static T? FindVisualAncestor<T>(this AvaloniaObject dependencyObject) where T : class =>
        RequireVisual(dependencyObject).FindAncestorOfType<T>();

    /// <summary>Finds the nearest matching logical ancestor, falling back to visual parents when needed.</summary>
    /// <typeparam name="T">The ancestor class or interface.</typeparam>
    /// <param name="dependencyObject">The object whose ancestors are searched.</param>
    /// <returns>The nearest matching ancestor, excluding the starting object, or null.</returns>
    /// <exception cref="ArgumentNullException">The starting object is null.</exception>
    /// <exception cref="InvalidOperationException">A nonvisual object has no logical parent to follow.</exception>
    public static T? FindLogicalAncestor<T>(this AvaloniaObject dependencyObject) where T : class
    {
        ArgumentNullException.ThrowIfNull(dependencyObject);
        for (AvaloniaObject? parent = LogicalOrVisualParent(dependencyObject); parent is not null; parent = LogicalOrVisualParent(parent))
            if (parent is T match) return match;
        return null;
    }

    /// <summary>Enumerates the object and its logical ancestors, falling back to visual parents when needed.</summary>
    /// <param name="self">The starting object, or null for an empty sequence.</param>
    /// <returns>The starting object followed by its ancestors, nearest first.</returns>
    /// <exception cref="InvalidOperationException">A nonvisual object has no logical parent to follow.</exception>
    public static IEnumerable<AvaloniaObject> FindLogicalAncestorsAndSelf(this AvaloniaObject? self)
    {
        for (AvaloniaObject? current = self; current is not null; current = LogicalOrVisualParent(current))
            yield return current;
    }

    private static AvaloniaObject? LogicalOrVisualParent(AvaloniaObject value)
    {
        value.VerifyAccess();
        return (value as ILogical)?.LogicalParent as AvaloniaObject ?? RequireVisual(value).GetVisualParent();
    }

    private static Visual RequireVisual(AvaloniaObject value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value is not Visual visual) throw new InvalidOperationException("The object is not an Avalonia visual.");
        visual.VerifyAccess();
        return visual;
    }
}
