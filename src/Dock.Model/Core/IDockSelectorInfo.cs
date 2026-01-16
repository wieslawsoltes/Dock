// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Provides customization options for the document/panel selector overlay.
/// </summary>
public interface IDockSelectorInfo
{
    /// <summary>
    /// Gets or sets whether this dockable appears in the selector.
    /// </summary>
    bool ShowInSelector { get; set; }

    /// <summary>
    /// Gets or sets the text displayed in the selector.
    /// </summary>
    string? SelectorTitle { get; set; }
}
