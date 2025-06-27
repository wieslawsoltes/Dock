// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Docking panel contract.
/// </summary>
public interface IDockDock : IDock
{
    /// <summary>
    /// Gets or sets a value which indicates whether the last child of the fills the remaining space in the panel.
    /// </summary>
    bool LastChildFill { get; set; }
}
