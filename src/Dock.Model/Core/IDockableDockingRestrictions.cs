// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace Dock.Model.Core;

/// <summary>
/// Optional docking restrictions for dockables.
/// </summary>
public interface IDockableDockingRestrictions
{
    /// <summary>
    /// Gets or sets allowed operations when this dockable is the drag source.
    /// </summary>
    DockOperationMask AllowedDockOperations { get; set; }

    /// <summary>
    /// Gets or sets allowed operations when this dockable is the drop target.
    /// </summary>
    DockOperationMask AllowedDropOperations { get; set; }
}
