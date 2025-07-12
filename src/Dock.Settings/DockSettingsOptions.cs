// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

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
    /// Optional global docking flag.
    /// </summary>
    public bool? EnableGlobalDocking { get; set; }

    /// <summary>
    /// Optional floating window owner flag.
    /// </summary>
    public bool? UseOwnerForFloatingWindows { get; set; }

    /// <summary>
    /// Optional window magnetism flag.
    /// </summary>
    public bool? EnableWindowMagnetism { get; set; }

    /// <summary>
    /// Optional window magnet snap distance.
    /// </summary>
    public double? WindowMagnetDistance { get; set; }

    /// <summary>
    /// Optional animation flag.
    /// </summary>
    public bool? EnableAnimations { get; set; }
}

