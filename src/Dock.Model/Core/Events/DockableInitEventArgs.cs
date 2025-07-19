// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Dockable init event args.
/// </summary>
public record DockableInitEventArgs(IDockable? Dockable) : EventArgs
{
    /// <summary>
    /// Gets or sets dockable context.
    /// </summary>
    public object? Context { get; set; }
}
