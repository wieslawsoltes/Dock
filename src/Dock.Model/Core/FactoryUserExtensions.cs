// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Dock.Model.Controls;

namespace Dock.Model.Core;

/// <summary>
/// User friendly helper methods for <see cref="IFactory"/>.
/// </summary>
public static class FactoryUserExtensions
{
    /// <summary>
    /// Adds a dockable to the dock and activates it.
    /// </summary>
    /// <param name="factory">The dock factory.</param>
    /// <param name="dock">The dock to add to.</param>
    /// <param name="dockable">The dockable to add.</param>
    public static void AddAndActivate(this IFactory factory, IDock dock, IDockable dockable)
    {
        if (factory == null) throw new ArgumentNullException(nameof(factory));
        if (dock == null) throw new ArgumentNullException(nameof(dock));
        if (dockable == null) throw new ArgumentNullException(nameof(dockable));

        factory.AddDockable(dock, dockable);
        factory.SetActiveDockable(dockable);
        factory.SetFocusedDockable(dock, dockable);
    }

    /// <summary>
    /// Closes the currently active dockable of the given dock.
    /// </summary>
    /// <param name="factory">The dock factory.</param>
    /// <param name="dock">The dock whose active dockable should be closed.</param>
    public static void CloseActiveDockable(this IFactory factory, IDock dock)
    {
        if (factory == null) throw new ArgumentNullException(nameof(factory));
        if (dock == null) throw new ArgumentNullException(nameof(dock));

        if (dock.ActiveDockable is { } active)
        {
            factory.CloseDockable(active);
        }
    }

    /// <summary>
    /// Floats the dockable into a new window and returns the created window.
    /// </summary>
    /// <param name="factory">The dock factory.</param>
    /// <param name="dockable">The dockable to float.</param>
    /// <returns>The window hosting the dockable or null.</returns>
    public static IDockWindow? FloatDockableWindow(this IFactory factory, IDockable dockable)
    {
        if (factory == null) throw new ArgumentNullException(nameof(factory));
        if (dockable == null) throw new ArgumentNullException(nameof(dockable));

        factory.FloatDockable(dockable);
        var root = factory.FindRoot(dockable);
        return root?.Window;
    }
}
