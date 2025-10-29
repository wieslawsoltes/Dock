// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model;

/// <summary>
/// Presentation mode for documents inside an <see cref="Controls.IDocumentDock"/>.
/// </summary>
public enum DocumentPresentation
{
    /// <summary>
    /// Traditional tabbed document interface.
    /// </summary>
    Tabs = 0,

    /// <summary>
    /// Standard MDI where each document is a window-like control within the client area.
    /// </summary>
    Mdi = 1
}


