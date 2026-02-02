// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Defines how pinned dock previews are displayed.
/// </summary>
public enum PinnedDockDisplayMode
{
    /// <summary>
    /// Preview overlays the content when open.
    /// </summary>
    Overlay = 0,

    /// <summary>
    /// Preview takes layout space when open.
    /// </summary>
    Inline = 1
}
