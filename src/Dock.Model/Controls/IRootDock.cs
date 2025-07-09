// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Windows.Input;
using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Root dock contract.
/// </summary>
public interface IRootDock : IDock
{
    /// <summary>
    /// Gets or sets if root dock is focusable.
    /// </summary>
    bool IsFocusableRoot { get; set; }

    /// <summary>
    /// Gets or sets hidden dockables.
    /// </summary>
    IList<IDockable>? HiddenDockables { get; set; }


    /// <summary>
    /// Gets or sets left top pinned dockables.
    /// </summary>
    IList<IDockable>? LeftTopPinnedDockables { get; set; }

    /// <summary>
    /// Gets or sets left bottom pinned dockables.
    /// </summary>
    IList<IDockable>? LeftBottomPinnedDockables { get; set; }

    /// <summary>
    /// Gets or sets right top pinned dockables.
    /// </summary>
    IList<IDockable>? RightTopPinnedDockables { get; set; }

    /// <summary>
    /// Gets or sets right bottom pinned dockables.
    /// </summary>
    IList<IDockable>? RightBottomPinnedDockables { get; set; }

    /// <summary>
    /// Gets or sets top left pinned dockables.
    /// </summary>
    IList<IDockable>? TopLeftPinnedDockables { get; set; }

    /// <summary>
    /// Gets or sets top right pinned dockables.
    /// </summary>
    IList<IDockable>? TopRightPinnedDockables { get; set; }

    /// <summary>
    /// Gets or sets bottom left pinned dockables.
    /// </summary>
    IList<IDockable>? BottomLeftPinnedDockables { get; set; }

    /// <summary>
    /// Gets or sets bottom right pinned dockables.
    /// </summary>
    IList<IDockable>? BottomRightPinnedDockables { get; set; }

    /// <summary>
    /// Gets or sets left pinned dockables alignment.
    /// </summary>
    Alignment LeftPinnedDockablesAlignment { get; set; }

    /// <summary>
    /// Gets or sets right pinned dockables alignment.
    /// </summary>
    Alignment RightPinnedDockablesAlignment { get; set; }

    /// <summary>
    /// Gets or sets top pinned dockables alignment.
    /// </summary>
    Alignment TopPinnedDockablesAlignment { get; set; }

    /// <summary>
    /// Gets or sets bottom pinned dockables alignment.
    /// </summary>
    Alignment BottomPinnedDockablesAlignment { get; set; }

    /// <summary>
    /// Gets or sets pinned tool dock.
    /// </summary>
    IToolDock? PinnedDock { get; set; }

    /// <summary>
    /// Gets or sets owner window.
    /// </summary>
    IDockWindow? Window { get; set; }

    /// <summary>
    /// Gets or sets windows.
    /// </summary>
    IList<IDockWindow>? Windows { get; set; }

    /// <summary>
    /// Show windows.
    /// </summary>
    ICommand ShowWindows { get; }

    /// <summary>
    /// Exit windows.
    /// </summary>
    ICommand ExitWindows { get; }
}
