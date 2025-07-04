// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Stack dock contract.
/// </summary>
public interface IStackDock : IDock
{
    /// <summary>
    /// Gets or sets layout orientation.
    /// </summary>
    Orientation Orientation { get; set; }

    /// <summary>
    /// Gets or sets spacing between items.
    /// </summary>
    double Spacing { get; set; }
}
