// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace Dock.Model.Core;

/// <summary>
/// Defines how floating dock windows are hosted.
/// </summary>
public enum DockFloatingWindowHostMode
{
    /// <summary>
    /// Uses the default host selection (falls back to DockSettings.UseManagedWindows).
    /// </summary>
    Default,

    /// <summary>
    /// Use native OS windows for floating docks.
    /// </summary>
    Native,

    /// <summary>
    /// Use managed in-app windows for floating docks.
    /// </summary>
    Managed
}
