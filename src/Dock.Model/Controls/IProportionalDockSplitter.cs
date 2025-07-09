// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Proportional dock splitter contract.
/// </summary>
public interface IProportionalDockSplitter : IDockable
{
    /// <summary>
    /// Gets or sets whether the splitter allows resizing.
    /// </summary>
    bool CanResize { get; set; }

    /// <summary>
    /// Resets the proportions of the elements adjacent to the splitter.
    /// </summary>
    void ResetProportion();

    /// <summary>
    /// Sets the proportion of the element before the splitter.
    /// </summary>
    /// <param name="proportion">The target proportion value.</param>
    void SetProportion(double proportion);
}
