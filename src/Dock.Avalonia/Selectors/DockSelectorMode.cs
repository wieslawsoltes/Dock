// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace Dock.Avalonia.Selectors;

/// <summary>
/// Defines selector scope for dockables.
/// </summary>
public enum DockSelectorMode
{
    /// <summary>
    /// Show documents only.
    /// </summary>
    Documents,

    /// <summary>
    /// Show tools only.
    /// </summary>
    Tools,

    /// <summary>
    /// Show documents and tools.
    /// </summary>
    All
}
