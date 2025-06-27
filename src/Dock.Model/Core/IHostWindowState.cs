// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Host window state contract.
/// </summary>
public interface IHostWindowState
{
    /// <summary>
    /// Gets or sets dock manager.
    /// </summary>
    IDockManager DockManager { get; set; }
}
