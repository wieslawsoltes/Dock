// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace Dock.Model.Core;

/// <summary>
/// Defines how a dock window resolves its host owner.
/// </summary>
public enum DockWindowOwnerMode
{
    /// <summary>
    /// Uses <see cref="IDockWindow.ParentWindow"/> when set; otherwise falls back to platform defaults.
    /// </summary>
    Default,

    /// <summary>
    /// Do not assign an owner.
    /// </summary>
    None,

    /// <summary>
    /// Use <see cref="IDockWindow.ParentWindow"/> as the owner when available.
    /// </summary>
    ParentWindow,

    /// <summary>
    /// Use the root dock window as the owner when available.
    /// </summary>
    RootWindow,

    /// <summary>
    /// Use the window that currently hosts the dockable being floated.
    /// </summary>
    DockableWindow
}
