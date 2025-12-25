// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;

namespace Dock.Model.CommandBars;

/// <summary>
/// Provides command bar definitions for a dockable.
/// </summary>
public interface IDockCommandBarProvider
{
    /// <summary>
    /// Gets the command bar definitions.
    /// </summary>
    /// <returns>The command bar definitions.</returns>
    IReadOnlyList<DockCommandBarDefinition> GetCommandBars();

    /// <summary>
    /// Raised when command bar definitions change.
    /// </summary>
    event EventHandler? CommandBarsChanged;
}
