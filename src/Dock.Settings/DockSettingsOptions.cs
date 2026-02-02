// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Input;
using Dock.Model.Core;

namespace Dock.Settings;

/// <summary>
/// Configuration options for <see cref="DockSettings"/>.
/// </summary>
public class DockSettingsOptions
{
    /// <summary>
    /// Optional minimum horizontal drag threshold.
    /// </summary>
    public double? MinimumHorizontalDragDistance { get; set; }

    /// <summary>
    /// Optional minimum vertical drag threshold.
    /// </summary>
    public double? MinimumVerticalDragDistance { get; set; }

    /// <summary>
    /// Optional floating adorner flag.
    /// </summary>
    public bool? UseFloatingDockAdorner { get; set; }

    /// <summary>
    /// Optional floating pinned dock window flag.
    /// </summary>
    public bool? UsePinnedDockWindow { get; set; }

    /// <summary>
    /// Optional managed window hosting flag.
    /// </summary>
    public bool? UseManagedWindows { get; set; }

    /// <summary>
    /// Optional floating window host mode.
    /// </summary>
    public DockFloatingWindowHostMode? FloatingWindowHostMode { get; set; }

    /// <summary>
    /// Optional floating window owner flag.
    /// </summary>
    public bool? UseOwnerForFloatingWindows { get; set; }

    /// <summary>
    /// Optional floating window owner policy.
    /// </summary>
    public DockFloatingWindowOwnerPolicy? FloatingWindowOwnerPolicy { get; set; }

    /// <summary>
    /// Optional window magnetism flag.
    /// </summary>
    public bool? EnableWindowMagnetism { get; set; }

    /// <summary>
    /// Optional window magnet snap distance.
    /// </summary>
    public double? WindowMagnetDistance { get; set; }

    /// <summary>
    /// Optional bring windows to front on drag flag.
    /// </summary>
    public bool? BringWindowsToFrontOnDrag { get; set; }

    /// <summary>
    /// Optional dockable drag preview flag.
    /// </summary>
    public bool? ShowDockablePreviewOnDrag { get; set; }

    /// <summary>
    /// Optional drag preview opacity.
    /// </summary>
    public double? DragPreviewOpacity { get; set; }

    /// <summary>
    /// Optional selector enabled flag.
    /// </summary>
    public bool? SelectorEnabled { get; set; }

    /// <summary>
    /// Optional document selector key gesture.
    /// </summary>
    public KeyGesture? DocumentSelectorKeyGesture { get; set; }

    /// <summary>
    /// Optional tool selector key gesture.
    /// </summary>
    public KeyGesture? ToolSelectorKeyGesture { get; set; }

    /// <summary>
    /// Optional command bar merging enabled flag.
    /// </summary>
    public bool? CommandBarMergingEnabled { get; set; }

    /// <summary>
    /// Optional command bar merging scope.
    /// </summary>
    public DockCommandBarMergingScope? CommandBarMergingScope { get; set; }
}
