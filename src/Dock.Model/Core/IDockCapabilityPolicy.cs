// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace Dock.Model.Core;

/// <summary>
/// Defines optional capability values used by root and dock capability policies.
/// </summary>
public interface IDockCapabilityPolicy
{
    /// <summary>
    /// Gets or sets an optional close capability value.
    /// </summary>
    bool? CanClose { get; set; }

    /// <summary>
    /// Gets or sets an optional pin capability value.
    /// </summary>
    bool? CanPin { get; set; }

    /// <summary>
    /// Gets or sets an optional float capability value.
    /// </summary>
    bool? CanFloat { get; set; }

    /// <summary>
    /// Gets or sets an optional drag capability value.
    /// </summary>
    bool? CanDrag { get; set; }

    /// <summary>
    /// Gets or sets an optional drop capability value.
    /// </summary>
    bool? CanDrop { get; set; }

    /// <summary>
    /// Gets or sets an optional dock-as-document capability value.
    /// </summary>
    bool? CanDockAsDocument { get; set; }
}
