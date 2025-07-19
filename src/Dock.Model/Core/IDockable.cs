﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls.Recycling.Model;

namespace Dock.Model.Core;

/// <summary>
/// Dockable contract.
/// </summary>
public interface IDockable : IControlRecyclingIdProvider
{
    /// <summary>
    /// Gets or sets dockable id.
    /// </summary>
    string Id { get; set; }

    /// <summary>
    /// Gets or sets dockable title.
    /// </summary>
    string Title { get; set; }

    /// <summary>
    /// Gets or sets dockable context.
    /// </summary>
    object? Context { get; set; }

    /// <summary>
    /// Gets or sets dockable owner.
    /// </summary>
    IDockable? Owner { get; set; }

    /// <summary>
    /// Gets or sets dockable original owner.
    /// </summary>
    IDockable? OriginalOwner { get; set; }

    /// <summary>
    /// Gets or sets dockable factory.
    /// </summary>
    IFactory? Factory { get; set; }

    /// <summary>
    /// Gets if the dockable is empty.
    /// </summary>
    bool IsEmpty { get; set; }

    /// <summary>
    /// Gets or sets if the dockable collapses when all its children are removed.
    /// </summary>
    bool IsCollapsable { get; set; }

    /// <summary>
    /// Gets or sets splitter proportion.
    /// </summary>
    double Proportion { get; set; }

    /// <summary> 
    /// Gets or sets docking mode. 
    /// </summary> 
    DockMode Dock { get; set; }

    /// <summary>
    /// Gets or sets grid column.
    /// </summary>
    int Column { get; set; }

    /// <summary>
    /// Gets or sets grid row.
    /// </summary>
    int Row { get; set; }

    /// <summary>
    /// Gets or sets grid column span.
    /// </summary>
    int ColumnSpan { get; set; }

    /// <summary>
    /// Gets or sets grid row span.
    /// </summary>
    int RowSpan { get; set; }

    /// <summary>
    /// Gets or sets whether this dock participates in shared size scope.
    /// </summary>
    bool IsSharedSizeScope { get; set; }

    /// <summary>
    /// Gets or sets last known proportion before collapse.
    /// </summary>
    double CollapsedProportion { get; set; }

    /// <summary>
    /// Gets or sets minimum width.
    /// </summary>
    double MinWidth { get; set; }

    /// <summary>
    /// Gets or sets maximum width.
    /// </summary>
    double MaxWidth { get; set; }

    /// <summary>
    /// Gets or sets minimum height.
    /// </summary>
    double MinHeight { get; set; }

    /// <summary>
    /// Gets or sets maximum height.
    /// </summary>
    double MaxHeight { get; set; }

    /// <summary>
    /// Gets or sets if the dockable can be closed.
    /// </summary>
    bool CanClose { get; set; }

    /// <summary>
    /// Gets or sets if the dockable can be pinned.
    /// </summary>
    bool CanPin { get; set; }

    /// <summary>
    /// Gets or sets if the dockable can be floated.
    /// </summary>
    bool CanFloat { get; set; }

    /// <summary>
    /// Gets or sets if the dockable can be dragged.
    /// </summary>
    bool CanDrag { get; set; }

    /// <summary>
    /// Gets or sets if the dockable can be dropped on.
    /// </summary>
    bool CanDrop { get; set; }

    /// <summary>
    /// Called when the dockable is closed.
    /// </summary>
    /// <returns>true to accept the close, and false to cancel the close.</returns>
    bool OnClose();

    /// <summary>
    /// Called when the dockable becomes the selected dockable.
    /// </summary>
    void OnSelected();

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

    /// <summary>
    /// Gets or sets tab index for MVVM based tab reordering.
    /// </summary>
    int TabIndex { get; set; }

    /// <summary>
    /// Called when tab index is about to change during drag reordering.
    /// </summary>
    /// <param name="newIndex">The target index.</param>
    void OnTabIndexChanging(int newIndex);

    /// <summary>
    /// Called after tab index changed due to drag reordering.
    /// </summary>
    /// <param name="oldIndex">The previous index.</param>
    /// <param name="newIndex">The new index.</param>
    void OnTabIndexChanged(int oldIndex, int newIndex);
}
