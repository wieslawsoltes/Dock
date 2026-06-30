// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using Avalonia.Layout;

namespace Dock.Controls.Flat;

/// <summary>
/// Describes a proportional container for flat layout.
/// </summary>
public interface IFlatProportionalDock : IFlatProportionalItem
{
    /// <summary>
    /// Gets the orientation used to arrange visible items.
    /// </summary>
    Orientation Orientation { get; }

    /// <summary>
    /// Gets the currently visible proportional children.
    /// </summary>
    IList<IFlatProportionalItem>? VisibleItems { get; }
}
