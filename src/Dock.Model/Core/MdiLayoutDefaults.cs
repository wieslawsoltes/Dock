// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace Dock.Model.Core;

/// <summary>
/// Defaults used by MDI layout helpers and controls.
/// </summary>
public static class MdiLayoutDefaults
{
    /// <summary>
    /// Default cascade offset between windows.
    /// </summary>
    public const double CascadeOffset = 24.0;

    /// <summary>
    /// Default minimized window height.
    /// </summary>
    public const double MinimizedHeight = 28.0;

    /// <summary>
    /// Default minimized window width.
    /// </summary>
    public const double MinimizedWidth = 220.0;

    /// <summary>
    /// Default spacing between minimized windows.
    /// </summary>
    public const double MinimizedSpacing = 4.0;

    /// <summary>
    /// Default width ratio for cascaded windows.
    /// </summary>
    public const double DefaultWidthRatio = 0.6;

    /// <summary>
    /// Default height ratio for cascaded windows.
    /// </summary>
    public const double DefaultHeightRatio = 0.6;

    /// <summary>
    /// Minimum width for cascaded windows.
    /// </summary>
    public const double MinimumWidth = 200.0;

    /// <summary>
    /// Minimum height for cascaded windows.
    /// </summary>
    public const double MinimumHeight = 120.0;
}
