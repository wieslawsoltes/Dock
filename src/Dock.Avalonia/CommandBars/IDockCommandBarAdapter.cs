// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using Avalonia.Controls;
using Dock.Model.CommandBars;

namespace Dock.Avalonia.CommandBars;

/// <summary>
/// Converts command bar definitions into Avalonia controls.
/// </summary>
public interface IDockCommandBarAdapter
{
    /// <summary>
    /// Builds controls for the specified command bar kind.
    /// </summary>
    /// <param name="kind">The command bar kind.</param>
    /// <param name="definitions">The command bar definitions.</param>
    /// <returns>The built controls.</returns>
    IReadOnlyList<Control> BuildBars(DockCommandBarKind kind, IReadOnlyList<DockCommandBarDefinition> definitions);
}
