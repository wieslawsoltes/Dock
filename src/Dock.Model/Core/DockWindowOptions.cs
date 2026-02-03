// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core;

/// <summary>
/// Options used to configure dock window relationships and presentation.
/// </summary>
public sealed class DockWindowOptions
{
    /// <summary>
    /// Gets or sets the owner resolution mode.
    /// </summary>
    public DockWindowOwnerMode OwnerMode { get; set; } = DockWindowOwnerMode.Default;

    /// <summary>
    /// Gets or sets the parent window for owner relationships.
    /// </summary>
    public IDockWindow? ParentWindow { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the window should be presented modally.
    /// </summary>
    public bool IsModal { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the window should appear in the taskbar.
    /// </summary>
    public bool? ShowInTaskbar { get; set; }

    /// <summary>
    /// Applies the options to the specified window.
    /// </summary>
    /// <param name="window">The window to configure.</param>
    public void ApplyTo(IDockWindow window)
    {
        if (window is null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        window.OwnerMode = OwnerMode;
        window.ParentWindow = ParentWindow;
        window.IsModal = IsModal;
        if (ShowInTaskbar is not null)
        {
            window.ShowInTaskbar = ShowInTaskbar;
        }
    }
}
