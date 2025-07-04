// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Windows.Input;

namespace Dock.Model.Core;

/// <summary>
/// Dock contract.
/// </summary>
public interface IDock : IDockable
{
    /// <summary>
    /// Gets or sets visible dockables.
    /// </summary>
    IList<IDockable>? VisibleDockables { get; set; }

    /// <summary>
    /// Gets or sets active dockable.
    /// </summary>
    IDockable? ActiveDockable { get; set; }

    /// <summary>
    /// Gets or sets default dockable.
    /// </summary>
    IDockable? DefaultDockable { get; set; }

    /// <summary>
    /// Gets or sets the focused dockable.
    /// </summary>
    IDockable? FocusedDockable { get; set; }

    /// <summary>
    /// Gets or sets if the dockable is the currently active.
    /// </summary>
    bool IsActive { get; set; }

    /// <summary>
    /// Gets the number of currently opened and visible dockables
    /// </summary>
    int OpenedDockablesCount { get; set; }

    /// <summary>
    /// Gets or sets whether the last remaining dockable can be closed.
    /// </summary>
    bool CanCloseLastDockable { get; set; }

    /// <summary>
    /// Gets a value that indicates whether there is at least one entry in back navigation history.
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Gets a value that indicates whether there is at least one entry in forward navigation history.
    /// </summary>
    bool CanGoForward { get; }

    /// <summary>
    /// Navigates to the most recent entry in back navigation history, if there is one.
    /// </summary>
    ICommand GoBack { get; }

    /// <summary>
    /// Navigate to the most recent entry in forward navigation history, if there is one.
    /// </summary>
    ICommand GoForward { get; }

    /// <summary>
    /// Navigate to content that is contained by an object.
    /// </summary>
    ICommand Navigate { get; }

    /// <summary>
    /// Close layout.
    /// </summary>
    ICommand Close { get; }
}
