// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls.Recycling.Model;

namespace Dock.Model.Core;

/// <summary>
/// Dockable contract.
/// </summary>
public interface IDockable : IControlRecyclingIdProvider, IBoundsInfo, IPointerTracking, IPinnableDockable
{
    string Id { get; set; }
    string Title { get; set; }
    object? Context { get; set; }
    IDockable? Owner { get; set; }
    IDockable? OriginalOwner { get; set; }
    IFactory? Factory { get; set; }
    bool IsEmpty { get; set; }
    bool IsCollapsable { get; set; }
    double Proportion { get; set; }
    DockMode Dock { get; set; }
    int Column { get; set; }
    int Row { get; set; }
    int ColumnSpan { get; set; }
    int RowSpan { get; set; }
    bool IsSharedSizeScope { get; set; }
    double CollapsedProportion { get; set; }
    double MinWidth { get; set; }
    double MaxWidth { get; set; }
    double MinHeight { get; set; }
    double MaxHeight { get; set; }
    bool CanClose { get; set; }
    bool CanFloat { get; set; }
    bool CanDrag { get; set; }
    bool CanDrop { get; set; }
    bool OnClose();
    void OnSelected();
}
