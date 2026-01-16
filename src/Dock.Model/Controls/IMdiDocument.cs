// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// MDI document contract.
/// </summary>
public interface IMdiDocument : IDocument
{
    /// <summary>
    /// Gets or sets MDI window bounds in dock coordinates.
    /// </summary>
    DockRect MdiBounds { get; set; }

    /// <summary>
    /// Gets or sets MDI window state.
    /// </summary>
    MdiWindowState MdiState { get; set; }

    /// <summary>
    /// Gets or sets MDI window Z index.
    /// </summary>
    int MdiZIndex { get; set; }
}
