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
    /// Resets proportions of neighbouring dockables.
    /// </summary>
    void ResetProportion();

    /// <summary>
    /// Sets proportion of the dockable preceding the splitter.
    /// </summary>
    /// <param name="proportion">Desired proportion value.</param>
    void SetProportion(object proportion);
}
