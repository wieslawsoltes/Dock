// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dock.Model.Controls
{
    /// <summary>
    /// Tool dock.
    /// </summary>
    public class ToolDock : DockBase, IToolDock
    {
        public IView FocusedView { get; set; }
    }
}
