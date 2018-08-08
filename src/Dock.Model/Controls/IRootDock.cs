// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dock.Model.Controls
{
    /// <summary>
    /// Root dock contract.
    /// </summary>
    public interface IRootDock : IDock
    {
        /// <summary>
        /// Gets or sets owner window.
        /// </summary>
        IDockWindow Window { get; set; }

        /// <summary>
        /// Gets or sets top pin dock.
        /// </summary>
        IPinDock Top { get; set; }

        /// <summary>
        /// Gets or sets bottom pin dock.
        /// </summary>
        IPinDock Bottom { get; set; }

        /// <summary>
        /// Gets or sets left pin dock.
        /// </summary>
        IPinDock Left { get; set; }

        /// <summary>
        /// Gets or sets right pin dock.
        /// </summary>
        IPinDock Right { get; set; }
    }
}
