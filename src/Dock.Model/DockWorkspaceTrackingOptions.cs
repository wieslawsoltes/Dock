// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Options for tracking layout changes to mark workspaces as dirty.
/// </summary>
public sealed class DockWorkspaceTrackingOptions
{
    /// <summary>
    /// Gets or sets an optional filter for dockables that should trigger dirty tracking.
    /// </summary>
    public Func<IDockable?, bool>? DockableFilter { get; set; }

    /// <summary>
    /// Gets or sets whether window drag end events mark the workspace as dirty.
    /// </summary>
    public bool TrackWindowMoves { get; set; } = true;
}
