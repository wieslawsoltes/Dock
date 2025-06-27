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
    /// Gets or sets minimum width.
    /// </summary>
    double MinWidth { get; set; }

    /// <summary>
    /// Gets or sets maximum width.
    /// </summary>
    double MaxWidth { get; set; }

    /// <summary>
    /// Gets or sets minimum height.
    /// </summary>
    double MinHeight { get; set; }

    /// <summary>
    /// Gets or sets maximum height.
    /// </summary>
    double MaxHeight { get; set; }
}
