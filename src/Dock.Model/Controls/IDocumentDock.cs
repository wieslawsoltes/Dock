// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dock.Model.Controls
{
    /// <summary>
    /// Document dock contract.
    /// </summary>
    public interface IDocumentDock : ITabDock
    {
        /// <summary>
        /// Gets or sets splitter proportion.
        /// </summary>
        double Proportion { get; set; }
    }
}
