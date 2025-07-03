// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

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
    /// Hides document tabs in floating windows when there is only one document.
    /// </summary>
    public static bool HideSingleFloatingDocumentTabs = true;

}
