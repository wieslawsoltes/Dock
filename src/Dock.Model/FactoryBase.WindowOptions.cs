// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Factory base class.
/// </summary>
public abstract partial class FactoryBase
{
    /// <summary>
    /// Applies window options to the given window.
    /// </summary>
    /// <param name="window">The window to configure.</param>
    /// <param name="options">The options to apply.</param>
    protected virtual void ApplyWindowOptions(IDockWindow window, DockWindowOptions? options)
    {
        if (options is null)
        {
            return;
        }

        options.ApplyTo(window);
    }

    /// <summary>
    /// Prepares window options using the source dockable before it is moved.
    /// </summary>
    /// <param name="dockable">The source dockable.</param>
    /// <param name="options">The options to update.</param>
    protected virtual void PrepareWindowOptionsForDockable(IDockable dockable, DockWindowOptions? options)
    {
        if (options is null)
        {
            return;
        }

        if (options.OwnerMode != DockWindowOwnerMode.DockableWindow)
        {
            return;
        }

        if (options.ParentWindow is not null)
        {
            return;
        }

        var root = FindRoot(dockable);
        if (root?.Window is { } window)
        {
            options.ParentWindow = window;
        }
    }
}
