// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// SplitView dock contract based on Avalonia's SplitView control.
/// Provides a container with a collapsible pane and main content area.
/// </summary>
[RequiresDataTemplate]
public interface ISplitViewDock : IDock
{
    /// <summary>
    /// Gets or sets the width of the pane when closed in compact display modes.
    /// </summary>
    /// <remarks>
    /// This value is used when <see cref="DisplayMode"/> is set to 
    /// <see cref="SplitViewDisplayMode.CompactOverlay"/> or 
    /// <see cref="SplitViewDisplayMode.CompactInline"/>.
    /// Default value is 48.
    /// </remarks>
    double CompactPaneLength { get; set; }

    /// <summary>
    /// Gets or sets how the pane is displayed in both open and closed states.
    /// </summary>
    SplitViewDisplayMode DisplayMode { get; set; }

    /// <summary>
    /// Gets or sets whether the pane is currently open.
    /// </summary>
    bool IsPaneOpen { get; set; }

    /// <summary>
    /// Gets or sets the width of the pane when it is open.
    /// </summary>
    /// <remarks>
    /// Default value is 320.
    /// </remarks>
    double OpenPaneLength { get; set; }

    /// <summary>
    /// Gets or sets the position of the pane (Left, Right, Top, or Bottom).
    /// </summary>
    SplitViewPanePlacement PanePlacement { get; set; }

    /// <summary>
    /// Gets or sets whether to use light dismiss overlay mode.
    /// When enabled in overlay modes, a light dismiss overlay is shown
    /// and clicking outside the pane can close it.
    /// </summary>
    bool UseLightDismissOverlayMode { get; set; }

    /// <summary>
    /// Gets or sets the dockable displayed in the pane area.
    /// This can be a single dockable or a container dock (like ToolDock).
    /// </summary>
    IDockable? PaneDockable { get; set; }

    /// <summary>
    /// Gets or sets the dockable displayed in the content area.
    /// This is typically the main content (like a DocumentDock).
    /// </summary>
    IDockable? ContentDockable { get; set; }

    /// <summary>
    /// Opens the pane.
    /// </summary>
    void OpenPane();

    /// <summary>
    /// Closes the pane.
    /// </summary>
    void ClosePane();

    /// <summary>
    /// Toggles the pane open/closed state.
    /// </summary>
    void TogglePane();
}
