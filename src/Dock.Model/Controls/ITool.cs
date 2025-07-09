// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Tool contract.
/// </summary>
public interface ITool : IDockable
{
    /// <summary>
    /// Gets or sets pinned alignment used when tool is pinned.
    /// </summary>
    Alignment PinnedAlignment { get; set; }
}
