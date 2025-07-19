// Copyright (c) Wiesław Šoltés. All rights reserved.
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
    /// Gets tracking adapter instance.
    /// </summary>
    ITrackingAdapter TrackingAdapter { get; }
}
