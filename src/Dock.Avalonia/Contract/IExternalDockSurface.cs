// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Contract;

/// <summary>
/// Represents an external surface that participates in DockControl drag and drop.
/// </summary>
public interface IExternalDockSurface
{
    /// <summary>
    /// Gets or sets the owning <see cref="DockControl"/>.
    /// </summary>
    DockControl? DockControl { get; set; }

    /// <summary>
    /// Gets the control root used for hit testing this external surface.
    /// </summary>
    Control SurfaceControl { get; }
}
