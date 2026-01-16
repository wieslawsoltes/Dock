// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace Dock.Model.CommandBars;

/// <summary>
/// Defines how a command bar merges with the host.
/// </summary>
public enum DockCommandBarMergeMode
{
    /// <summary>
    /// Replace the host command bar.
    /// </summary>
    Replace,

    /// <summary>
    /// Append items or bars to the host.
    /// </summary>
    Append,

    /// <summary>
    /// Merge items into a matching group.
    /// </summary>
    MergeByGroup
}
