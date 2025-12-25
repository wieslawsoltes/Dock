// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Windows.Input;

namespace Dock.Model.CommandBars;

/// <summary>
/// Describes a command bar item for menus and toolbars.
/// </summary>
public sealed class DockCommandBarItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DockCommandBarItem"/> class.
    /// </summary>
    /// <param name="header">The item header content.</param>
    public DockCommandBarItem(object? header)
    {
        Header = header;
    }

    /// <summary>
    /// Gets or sets the header content.
    /// </summary>
    public object? Header { get; set; }

    /// <summary>
    /// Gets or sets the icon content.
    /// </summary>
    public object? Icon { get; set; }

    /// <summary>
    /// Gets or sets the command executed by this item.
    /// </summary>
    public ICommand? Command { get; set; }

    /// <summary>
    /// Gets or sets the command parameter.
    /// </summary>
    public object? CommandParameter { get; set; }

    /// <summary>
    /// Gets or sets child items (for menus or split buttons).
    /// </summary>
    public IReadOnlyList<DockCommandBarItem>? Items { get; set; }

    /// <summary>
    /// Gets or sets the group identifier used for merge-by-group.
    /// </summary>
    public string? GroupId { get; set; }

    /// <summary>
    /// Gets or sets the item order within a group.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this item is a separator.
    /// </summary>
    public bool IsSeparator { get; set; }
}
