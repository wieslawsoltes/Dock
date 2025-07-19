// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Defines pinning capabilities for a dockable.
/// </summary>
public interface IPinnableDockable
{
    /// <summary>
    /// Gets or sets if the dockable can be pinned.
    /// </summary>
    bool CanPin { get; set; }
}
