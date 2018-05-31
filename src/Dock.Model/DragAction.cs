// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;

namespace Dock.Model
{
    /// <summary>
    /// Defines the available drag actions.
    /// </summary>
    [Flags]
    public enum DragAction
    {
        /// <summary>
        /// No action.
        /// </summary>
        None = 0,

        /// <summary>
        /// Copy action.
        /// </summary>
        Copy = 1,

        /// <summary>
        /// Move action.
        /// </summary>
        Move = 2,

        /// <summary>
        /// Link action.
        /// </summary>
        Link = 4
    }
}
