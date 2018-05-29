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
        None = 0,
        Copy = 1,
        Move = 2,
        Link = 4
    }
}
