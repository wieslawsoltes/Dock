// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Dock.Avalonia.Internal;
using Dock.Controls.Flat;
using Dock.Model.Controls;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Splitter used by <see cref="FlatProportionalDockPanel"/> to resize flattened proportional dock regions.
/// </summary>
public class FlatProportionalDockSplitter : FlatProportionalSplitter
{
    /// <summary>
    /// Gets the Dock model splitter represented by this control.
    /// </summary>
    public new IProportionalDockSplitter? Splitter =>
        base.Splitter is DockFlatProportionalAdapter.DockFlatSplitterAdapter adapter ? adapter.Splitter : null;

    /// <summary>
    /// Gets the Dock model proportional dock that owns <see cref="Splitter"/>.
    /// </summary>
    public new IProportionalDock? OwnerDock =>
        base.OwnerDock is DockFlatProportionalAdapter.DockFlatDockAdapter adapter ? adapter.Dock : null;
}
