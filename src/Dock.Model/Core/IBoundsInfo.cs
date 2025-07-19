// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Provides bounds information for tracking dockables.
/// </summary>
public interface IBoundsInfo
{
    /// <summary>
    /// Gets dockable visible bounds information used for tracking.
    /// </summary>
    /// <param name="x">The dockable x axis position.</param>
    /// <param name="y">The dockable y axis position.</param>
    /// <param name="width">The dockable width.</param>
    /// <param name="height">The dockable height.</param>
    void GetVisibleBounds(out double x, out double y, out double width, out double height);

    /// <summary>
    /// Sets dockable visible bounds information used for tracking.
    /// </summary>
    /// <param name="x">The dock x axis position.</param>
    /// <param name="y">The dock y axis position.</param>
    /// <param name="width">The dockable width.</param>
    /// <param name="height">The dockable height.</param>
    void SetVisibleBounds(double x, double y, double width, double height);

    /// <summary>
    /// Called when dockable visible bounds changed.
    /// </summary>
    /// <param name="x">The dock x axis position.</param>
    /// <param name="y">The dock y axis position.</param>
    /// <param name="width">The dockable width.</param>
    /// <param name="height">The dockable height.</param>
    void OnVisibleBoundsChanged(double x, double y, double width, double height);

    /// <summary>
    /// Gets dockable pinned bounds information used for tracking.
    /// </summary>
    /// <param name="x">The dockable x axis position.</param>
    /// <param name="y">The dockable y axis position.</param>
    /// <param name="width">The dockable width.</param>
    /// <param name="height">The dockable height.</param>
    void GetPinnedBounds(out double x, out double y, out double width, out double height);

    /// <summary>
    /// Sets dockable pinned bounds information used for tracking.
    /// </summary>
    /// <param name="x">The dock x axis position.</param>
    /// <param name="y">The dock y axis position.</param>
    /// <param name="width">The dockable width.</param>
    /// <param name="height">The dockable height.</param>
    void SetPinnedBounds(double x, double y, double width, double height);

    /// <summary>
    /// Called when dockable pinned bounds changed.
    /// </summary>
    /// <param name="x">The dock x axis position.</param>
    /// <param name="y">The dock y axis position.</param>
    /// <param name="width">The dockable width.</param>
    /// <param name="height">The dockable height.</param>
    void OnPinnedBoundsChanged(double x, double y, double width, double height);

    /// <summary>
    /// Gets dockable tab bounds information used for tracking.
    /// </summary>
    /// <param name="x">The dockable x axis position.</param>
    /// <param name="y">The dockable y axis position.</param>
    /// <param name="width">The dockable width.</param>
    /// <param name="height">The dockable height.</param>
    void GetTabBounds(out double x, out double y, out double width, out double height);

    /// <summary>
    /// Sets dockable tab bounds information used for tracking.
    /// </summary>
    /// <param name="x">The dock x axis position.</param>
    /// <param name="y">The dock y axis position.</param>
    /// <param name="width">The dockable width.</param>
    /// <param name="height">The dockable height.</param>
    void SetTabBounds(double x, double y, double width, double height);

    /// <summary>
    /// Called when dockable tab bounds changed.
    /// </summary>
    /// <param name="x">The dock x axis position.</param>
    /// <param name="y">The dock y axis position.</param>
    /// <param name="width">The dockable width.</param>
    /// <param name="height">The dockable height.</param>
    void OnTabBoundsChanged(double x, double y, double width, double height);
}
