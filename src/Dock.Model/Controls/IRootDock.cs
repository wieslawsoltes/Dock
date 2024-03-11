
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
    /// Gets or sets left pinned dockables.
    /// </summary>
    IList<IDockable>? LeftPinnedDockables { get; set; }

    /// <summary>
    /// Gets or sets right pinned dockables.
    /// </summary>
    IList<IDockable>? RightPinnedDockables { get; set; }

    /// <summary>
    /// Gets or sets top pinned dockables.
    /// </summary>
    IList<IDockable>? TopPinnedDockables { get; set; }

    /// <summary>
    /// Gets or sets bottom pinned dockables.
    /// </summary>
    IList<IDockable>? BottomPinnedDockables { get; set; }

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
