// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dock.Model
{
    /// <summary>
    /// Dock contract.
    /// </summary>
    public interface IDock : IView, IViewsHost, IWindowsHost
    {
        /// <summary>
        /// Gets or sets dock position.
        /// </summary>
        string Dock { get; set; }

        /// <summary>
        /// Gets or sets dock factory.
        /// </summary>
        IDockFactory Factory { get; set; }
    }
}
