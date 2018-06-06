// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dock.Model.Controls
{
    /// <summary>
    /// Layout dock.
    /// </summary>
    public class LayoutDock : DockBase, ILayoutDock
    {
        public IView FocusedView { get; set; }
    }
}
