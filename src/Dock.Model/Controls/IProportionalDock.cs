// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Proportional dock contract.
/// </summary>
public interface IProportionalDock : IDock, IGlobalTarget
{
    /// <summary>
    /// Gets or sets layout orientation.
    /// </summary>
    Orientation Orientation { get; set; }
}
