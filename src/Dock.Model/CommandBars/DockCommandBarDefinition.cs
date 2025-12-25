// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;

namespace Dock.Model.CommandBars;

/// <summary>
/// Describes a command bar provided by a dockable.
/// </summary>
public sealed class DockCommandBarDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DockCommandBarDefinition"/> class.
    /// </summary>
    /// <param name="id">The command bar identifier.</param>
    /// <param name="kind">The command bar kind.</param>
    public DockCommandBarDefinition(string id, DockCommandBarKind kind)
    {
        Id = id;
        Kind = kind;
    }

    /// <summary>
    /// Gets the command bar identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the command bar kind.
    /// </summary>
    public DockCommandBarKind Kind { get; }

    /// <summary>
    /// Gets or sets the merge mode.
    /// </summary>
    public DockCommandBarMergeMode MergeMode { get; set; } = DockCommandBarMergeMode.Replace;

    /// <summary>
    /// Gets or sets the bar order.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the group identifier used for merge-by-group.
    /// </summary>
    public string? GroupId { get; set; }

    /// <summary>
    /// Gets or sets the bar items.
    /// </summary>
    public IReadOnlyList<DockCommandBarItem>? Items { get; set; }

    /// <summary>
    /// Gets or sets the bar content.
    /// </summary>
    public object? Content { get; set; }
}
