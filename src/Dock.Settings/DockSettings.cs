// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using Avalonia;
using Avalonia.Input;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Settings;

/// <summary>
/// Dock settings.
/// </summary>
public static class DockSettings
{
    /// <summary>
    /// Minimum horizontal drag distance to initiate drag operation.
    /// </summary>
    public static double MinimumHorizontalDragDistance = 4;

    /// <summary>
    /// Minimum vertical drag distance to initiate drag operation.
    /// </summary>
    public static double MinimumVerticalDragDistance = 4;

    /// <summary>
    /// Show dock adorners using floating transparent window.
    /// </summary>
    public static bool UseFloatingDockAdorner = false;

    /// <summary>
    /// Show auto-hidden dockables inside a floating window.
    /// </summary>
    public static bool UsePinnedDockWindow = false;

    /// <summary>
    /// Use managed (in-app) windows instead of native OS windows for floating windows.
    /// </summary>
    public static bool UseManagedWindows = false;

    /// <summary>
    /// Sets the global floating window host mode.
    /// </summary>
    public static DockFloatingWindowHostMode FloatingWindowHostMode = DockFloatingWindowHostMode.Default;
    
    /// <summary>
    /// Floating windows use the main window as their owner so they stay in front.
    /// </summary>
    public static bool UseOwnerForFloatingWindows = true;

    /// <summary>
    /// Sets the global floating window owner policy.
    /// </summary>
    public static DockFloatingWindowOwnerPolicy FloatingWindowOwnerPolicy = DockFloatingWindowOwnerPolicy.Default;

    /// <summary>
    /// Sets the default owner mode for floating windows when <see cref="IDockWindow.OwnerMode"/> is <see cref="DockWindowOwnerMode.Default"/>.
    /// </summary>
    public static DockWindowOwnerMode DefaultFloatingWindowOwnerMode = DockWindowOwnerMode.Default;

    /// <summary>
    /// Snap floating windows to nearby windows when dragging.
    /// </summary>
    public static bool EnableWindowMagnetism = false;

    /// <summary>
    /// Distance in pixels within which windows snap together.
    /// </summary>
    public static double WindowMagnetDistance = 16;

    /// <summary>
    /// Bring all windows to the front when a window starts being dragged.
    /// </summary>
    public static bool BringWindowsToFrontOnDrag = true;

    /// <summary>
    /// Close all floating windows when the main (non-host) window closes.
    /// </summary>
    public static bool CloseFloatingWindowsOnMainWindowClose = false;

    /// <summary>
    /// Show dockable content inside the drag preview window while dragging.
    /// </summary>
    public static bool ShowDockablePreviewOnDrag = false;

    /// <summary>
    /// Opacity for the drag preview window.
    /// </summary>
    public static double DragPreviewOpacity = 1.0;

    /// <summary>
    /// When true, closing an ItemsSource-generated document or tool attempts to remove its item from
    /// the backing <see cref="System.Collections.IList"/> source collection.
    /// </summary>
    public static bool UpdateItemsSourceOnUnregister = true;
    
    /// <summary>
    /// Configures the proportion used for global docking split operations.
    /// </summary>
    public static double GlobalDockingProportion = 0.5;

    /// <summary>
    /// Configures global docking behavior as a combined preset.
    /// </summary>
    public static DockGlobalDockingPreset GlobalDockingPreset = DockGlobalDockingPreset.GlobalFirst;

    /// <summary>
    /// Enables verbose diagnostics logging for docking workflows.
    /// </summary>
    public static bool EnableDiagnosticsLogging = false;

    /// <summary>
    /// Optional handler that is invoked when diagnostics logging emits a message.
    /// </summary>
    public static Action<string>? DiagnosticsLogHandler = null;

    private static bool s_selectorEnabled = true;
    private static KeyGesture s_documentSelectorKeyGesture = new(Key.Tab, KeyModifiers.Control);
    private static KeyGesture s_toolSelectorKeyGesture = new(Key.Tab, KeyModifiers.Control | KeyModifiers.Alt);
    private static bool s_commandBarMergingEnabled = false;
    private static DockCommandBarMergingScope s_commandBarMergingScope = DockCommandBarMergingScope.ActiveDocument;

    /// <summary>
    /// Gets or sets whether the document/panel selector is enabled.
    /// </summary>
    public static bool SelectorEnabled
    {
        get => s_selectorEnabled;
        set => s_selectorEnabled = value;
    }

    /// <summary>
    /// Gets or sets the key gesture for the document selector.
    /// </summary>
    public static KeyGesture DocumentSelectorKeyGesture
    {
        get => s_documentSelectorKeyGesture;
        set => s_documentSelectorKeyGesture = value;
    }

    /// <summary>
    /// Gets or sets the key gesture for the tool selector.
    /// </summary>
    public static KeyGesture ToolSelectorKeyGesture
    {
        get => s_toolSelectorKeyGesture;
        set => s_toolSelectorKeyGesture = value;
    }

    /// <summary>
    /// Gets or sets whether command bar merging is enabled.
    /// </summary>
    public static bool CommandBarMergingEnabled
    {
        get => s_commandBarMergingEnabled;
        set => s_commandBarMergingEnabled = value;
    }

    /// <summary>
    /// Gets or sets the command bar merging scope.
    /// </summary>
    public static DockCommandBarMergingScope CommandBarMergingScope
    {
        get => s_commandBarMergingScope;
        set => s_commandBarMergingScope = value;
    }


    /// <summary>
    /// Checks if the drag distance is greater than the minimum required distance to initiate a drag operation.
    /// </summary>
    /// <param name="diff">The drag delta.</param>
    /// <returns>True if drag delta is above minimum drag distance threshold.</returns>
    public static bool IsMinimumDragDistance(Vector diff)
    {
        return (Math.Abs(diff.X) > DockSettings.MinimumHorizontalDragDistance
                || Math.Abs(diff.Y) > DockSettings.MinimumVerticalDragDistance);
    }

    /// <summary>
    /// Checks if the drag distance is greater than the minimum required distance to initiate a drag operation.
    /// </summary>
    /// <param name="diff">The drag delta.</param>
    /// <returns>True if drag delta is above minimum drag distance threshold.</returns>
    public static bool IsMinimumDragDistance(PixelPoint diff)
    {
        return (Math.Abs(diff.X) > DockSettings.MinimumHorizontalDragDistance
                || Math.Abs(diff.Y) > DockSettings.MinimumVerticalDragDistance);
    }

    /// <summary>
    /// Resolves the effective floating window host mode.
    /// </summary>
    /// <param name="root">Optional root dock override.</param>
    public static DockFloatingWindowHostMode ResolveFloatingWindowHostMode(IRootDock? root = null)
    {
        if (root is { FloatingWindowHostMode: var hostMode } && hostMode != DockFloatingWindowHostMode.Default)
        {
            return hostMode;
        }

        if (FloatingWindowHostMode != DockFloatingWindowHostMode.Default)
        {
            return FloatingWindowHostMode;
        }

        return UseManagedWindows ? DockFloatingWindowHostMode.Managed : DockFloatingWindowHostMode.Native;
    }

    /// <summary>
    /// Returns true when managed window hosting is enabled.
    /// </summary>
    /// <param name="root">Optional root dock override.</param>
    public static bool IsManagedWindowHostingEnabled(IRootDock? root = null)
    {
        return ResolveFloatingWindowHostMode(root) == DockFloatingWindowHostMode.Managed;
    }

    /// <summary>
    /// Returns true when floating windows should use an owner by default.
    /// </summary>
    public static bool ShouldUseOwnerForFloatingWindows()
    {
        return FloatingWindowOwnerPolicy switch
        {
            DockFloatingWindowOwnerPolicy.AlwaysOwned => true,
            DockFloatingWindowOwnerPolicy.NeverOwned => false,
            _ => UseOwnerForFloatingWindows
        };
    }
}

/// <summary>
/// Defines combined global docking behavior presets.
/// </summary>
public enum DockGlobalDockingPreset
{
    /// <summary>
    /// Local-first behavior with drop-context global target resolution.
    /// </summary>
    LocalFirst,

    /// <summary>
    /// Global-first behavior with outermost global target resolution.
    /// </summary>
    GlobalFirst
}

/// <summary>
/// Defines which dockable contributes command bars to a host window.
/// </summary>
public enum DockCommandBarMergingScope
{
    /// <summary>
    /// Merge command bars from the active document only.
    /// </summary>
    ActiveDocument,

    /// <summary>
    /// Merge command bars from the active dockable (documents and tools).
    /// </summary>
    ActiveDockable
}
