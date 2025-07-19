// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Provides pointer tracking information for dockables.
/// </summary>
public interface IPointerTracking
{
    /// <summary>
    /// Gets dockable pointer position used for tracking.
    /// </summary>
    /// <param name="x">The pointer x axis position.</param>
    /// <param name="y">The pointer y axis position.</param>
    void GetPointerPosition(out double x, out double y);

    /// <summary>
    /// Sets dockable pointer position used for tracking.
    /// </summary>
    /// <param name="x">The pointer x axis position.</param>
    /// <param name="y">The pointer y axis position.</param>
    void SetPointerPosition(double x, double y);

    /// <summary>
    /// Called when dockable pointer position changed.
    /// </summary>
    /// <param name="x">The pointer x axis position.</param>
    /// <param name="y">The pointer y axis position.</param>
    void OnPointerPositionChanged(double x, double y);

    /// <summary>
    /// Gets dockable pointer screen position used for tracking.
    /// </summary>
    /// <param name="x">The pointer x axis position.</param>
    /// <param name="y">The pointer y axis position.</param>
    void GetPointerScreenPosition(out double x, out double y);

    /// <summary>
    /// Sets dockable pointer screen position used for tracking.
    /// </summary>
    /// <param name="x">The pointer x axis position.</param>
    /// <param name="y">The pointer y axis position.</param>
    void SetPointerScreenPosition(double x, double y);

    /// <summary>
    /// Called when dockable pointer screen position changed.
    /// </summary>
    /// <param name="x">The pointer x axis position.</param>
    /// <param name="y">The pointer y axis position.</param>
    void OnPointerScreenPositionChanged(double x, double y);
}
