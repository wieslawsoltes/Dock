// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dock.Model.Controls
{
    /// <summary>
    /// Layout dock contract.
    /// </summary>
    public interface ILayoutDock : IDock
    {
        /// <summary>
        /// Gets or sets splitter proportion.
        /// </summary>
        double Proportion { get; set; }

        /// <summary>
        /// Gets or sets layout orientation.
        /// </summary>
        Orientation Orientation { get; set; }
    }
}
