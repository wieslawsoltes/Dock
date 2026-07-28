// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Windows.Input;

namespace Dock.Avalonia.Commands;

/// <summary>
/// Provides presentation-layer commands that adapt dockable command parameters to typed factory operations.
/// </summary>
public static class DockCommands
{
    /// <summary>
    /// Pins or unpins the supplied dockable.
    /// </summary>
    public static readonly ICommand PinDockable =
        new DockableCommand(static (factory, dockable) => factory.PinDockable(dockable));

    /// <summary>
    /// Temporarily shows the supplied pinned dockable.
    /// </summary>
    public static readonly ICommand PreviewPinnedDockable =
        new DockableCommand(static (factory, dockable) => factory.PreviewPinnedDockable(dockable));

    /// <summary>
    /// Toggles the preview state of the supplied pinned dockable.
    /// </summary>
    public static readonly ICommand TogglePreviewPinnedDockable =
        new DockableCommand(static (factory, dockable) => factory.TogglePreviewPinnedDockable(dockable));

    /// <summary>
    /// Floats the supplied dockable.
    /// </summary>
    public static readonly ICommand FloatDockable =
        new DockableCommand(static (factory, dockable) => factory.FloatDockable(dockable));

    /// <summary>
    /// Floats the owner dock containing the supplied dockable.
    /// </summary>
    public static readonly ICommand FloatAllDockables =
        new DockableCommand(static (factory, dockable) => factory.FloatAllDockables(dockable));

    /// <summary>
    /// Docks the supplied dockable as a document.
    /// </summary>
    public static readonly ICommand DockAsDocument =
        new DockableCommand(static (factory, dockable) => factory.DockAsDocument(dockable));

    /// <summary>
    /// Closes the supplied dockable.
    /// </summary>
    public static readonly ICommand CloseDockable =
        new DockableCommand(static (factory, dockable) => factory.CloseDockable(dockable));

    /// <summary>
    /// Closes the other dockables alongside the supplied dockable.
    /// </summary>
    public static readonly ICommand CloseOtherDockables =
        new DockableCommand(static (factory, dockable) => factory.CloseOtherDockables(dockable));

    /// <summary>
    /// Closes all dockables alongside the supplied dockable.
    /// </summary>
    public static readonly ICommand CloseAllDockables =
        new DockableCommand(static (factory, dockable) => factory.CloseAllDockables(dockable));

    /// <summary>
    /// Closes the dockables to the left of the supplied dockable.
    /// </summary>
    public static readonly ICommand CloseLeftDockables =
        new DockableCommand(static (factory, dockable) => factory.CloseLeftDockables(dockable));

    /// <summary>
    /// Closes the dockables to the right of the supplied dockable.
    /// </summary>
    public static readonly ICommand CloseRightDockables =
        new DockableCommand(static (factory, dockable) => factory.CloseRightDockables(dockable));

    /// <summary>
    /// Moves document tabs to the left edge.
    /// </summary>
    public static readonly ICommand SetDocumentDockTabsLayoutLeft =
        new DockableCommand(static (factory, dockable) => factory.SetDocumentDockTabsLayoutLeft(dockable));

    /// <summary>
    /// Moves document tabs to the top edge.
    /// </summary>
    public static readonly ICommand SetDocumentDockTabsLayoutTop =
        new DockableCommand(static (factory, dockable) => factory.SetDocumentDockTabsLayoutTop(dockable));

    /// <summary>
    /// Moves document tabs to the right edge.
    /// </summary>
    public static readonly ICommand SetDocumentDockTabsLayoutRight =
        new DockableCommand(static (factory, dockable) => factory.SetDocumentDockTabsLayoutRight(dockable));

    /// <summary>
    /// Switches the document dock to tabbed layout mode.
    /// </summary>
    public static readonly ICommand SetDocumentDockLayoutModeTabbed =
        new DockableCommand(static (factory, dockable) => factory.SetDocumentDockLayoutModeTabbed(dockable));

    /// <summary>
    /// Switches the document dock to MDI layout mode.
    /// </summary>
    public static readonly ICommand SetDocumentDockLayoutModeMdi =
        new DockableCommand(static (factory, dockable) => factory.SetDocumentDockLayoutModeMdi(dockable));

    /// <summary>
    /// Creates a horizontal document dock for the supplied dockable.
    /// </summary>
    public static readonly ICommand NewHorizontalDocumentDock =
        new DockableCommand(static (factory, dockable) => factory.NewHorizontalDocumentDock(dockable));

    /// <summary>
    /// Creates a vertical document dock for the supplied dockable.
    /// </summary>
    public static readonly ICommand NewVerticalDocumentDock =
        new DockableCommand(static (factory, dockable) => factory.NewVerticalDocumentDock(dockable));
}
