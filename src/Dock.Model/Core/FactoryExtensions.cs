// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Linq;
using Dock.Model.Controls;

namespace Dock.Model.Core;

/// <summary>
/// Helper methods for <see cref="IFactory"/>.
/// </summary>
public static class FactoryExtensions
{
    /// <summary>
    /// Finds the root dock that currently has focus.
    /// </summary>
    /// <param name="factory">The dock factory.</param>
    /// <returns>The active <see cref="IRootDock"/> if found, otherwise <c>null</c>.</returns>
    public static IRootDock? GetActiveRoot(this IFactory factory)
    {
        if (factory is null)
        {
            return null;
        }

        if (factory.CurrentRootDock is { } trackedRoot)
        {
            return trackedRoot;
        }

        return factory
            .Find(d => d is IRootDock root && root.IsActive)
            .OfType<IRootDock>()
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets the document that is currently focused across all docks and windows.
    /// </summary>
    /// <param name="factory">The dock factory.</param>
    /// <returns>The active <see cref="IDocument"/> or <c>null</c> if no document is focused.</returns>
    public static IDocument? GetCurrentDocument(this IFactory factory)
    {
        if (factory is null)
        {
            return null;
        }

        return factory.CurrentDockable as IDocument
            ?? factory.GetActiveRoot()?.FocusedDockable as IDocument;
    }

    /// <summary>
    /// Closes the dockable that is currently focused across all docks and windows.
    /// </summary>
    /// <param name="factory">The dock factory.</param>
    public static void CloseFocusedDockable(this IFactory factory)
    {
        if (factory is null)
        {
            return;
        }

        var dockable = factory.CurrentDockable ?? factory.GetActiveRoot()?.FocusedDockable;
        if (dockable is { })
        {
            factory.CloseDockable(dockable);
        }
    }

    /// <summary>
    /// Gets currently tracked dock window.
    /// </summary>
    /// <param name="factory">The dock factory.</param>
    /// <returns>The current dock window or null.</returns>
    public static IDockWindow? GetCurrentDockWindow(this IFactory factory)
    {
        return factory?.CurrentDockWindow;
    }

    /// <summary>
    /// Gets currently tracked host window.
    /// </summary>
    /// <param name="factory">The dock factory.</param>
    /// <returns>The current host window or null.</returns>
    public static IHostWindow? GetCurrentHostWindow(this IFactory factory)
    {
        return factory?.CurrentHostWindow;
    }
}
