// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dock.Model.Controls
{
    /// <summary>
    /// Pin dock contract.
    /// </summary>
    public interface IPinDock : IDock
    {
        /// <summary>
        /// Gets or sets if the Dock is expanded.
        /// </summary>
        bool IsExpanded { get; set; }

        /// <summary>
        /// Gets or sets if the Dock auto hides view when pointer is not over.
        /// </summary>
        bool AutoHide { get; set; }
    }
}
