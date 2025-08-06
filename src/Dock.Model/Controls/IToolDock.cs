// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Tool dock contract.
/// </summary>
[RequiresDataTemplate]
public interface IToolDock : IDock, ILocalTarget
{
    /// <summary>
    /// Gets or sets dock auto hide alignment.
    /// </summary>
    Alignment Alignment { get; set; }

    /// <summary>
    /// Gets or sets if the Dock is expanded.
    /// </summary>
    bool IsExpanded { get; set; }

    /// <summary>
    /// Gets or sets if the Dock auto hides dockable when pointer is not over.
    /// </summary>
    bool AutoHide { get; set; }

    /// <summary>
    /// Gets or sets if the tool Dock grip mode.
    /// </summary>
    GripMode GripMode { get; set; }

    /// <summary>
    /// Adds the specified tool to this dock and activates it.
    /// </summary>
    /// <param name="tool">The tool to add.</param>
    void AddTool(IDockable tool);
}
