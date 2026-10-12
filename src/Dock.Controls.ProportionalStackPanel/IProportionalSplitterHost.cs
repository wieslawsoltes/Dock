// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Dock.Controls.ProportionalStackPanel;

/// <summary>Allows a panel with its own sizing policy to use Dock's splitter input and preview.</summary>
/// <remarks>Implement this interface on the panel containing the panes and splitters.
/// Splitters can be direct children or the content of a child content presenter.
/// Collapsed panes are identified by <see cref="ProportionalStackPanel.IsCollapsedProperty"/>.</remarks>
public interface IProportionalSplitterHost
{
    /// <summary>Gets the panel whose child bounds define the resize gesture.</summary>
    Panel Panel { get; }

    /// <summary>Gets the axis along which panes are arranged.</summary>
    Orientation Orientation { get; }

    /// <summary>Raised when the split axis changes, so splitters update their size and cursor.</summary>
    event EventHandler? OrientationChanged;

    /// <summary>Applies a resize to the nearest noncollapsed panes on each side of a splitter.</summary>
    /// <param name="target">The pane before the splitter.</param>
    /// <param name="neighbor">The pane after the splitter.</param>
    /// <param name="dragDelta">Movement along the split axis in device-independent pixels.</param>
    /// <param name="minimumSize">The minimum pane size requested by the splitter.</param>
    /// <remarks>The host applies its own constraints and persists the resulting dimensions.
    /// Preview gestures invoke this method only on release; live gestures invoke it on movement.</remarks>
    void Resize(Control target, Control neighbor, double dragDelta, double minimumSize);
}
