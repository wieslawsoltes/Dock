// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace Dock.Model.Core;

/// <summary>
/// Dock manager options shared across docking surfaces.
/// </summary>
public sealed class DockManagerOptions
{
    /// <summary>
    /// Gets or sets whether docking interactions are enabled.
    /// </summary>
    public bool IsDockingEnabled { get; set; } = true;
}
