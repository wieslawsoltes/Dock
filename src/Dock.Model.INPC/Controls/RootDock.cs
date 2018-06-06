// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dock.Model.Controls
{
    /// <summary>
    /// Root dock.
    /// </summary>
    public class RootDock : DockBase, IRootDock
    {
        public IView FocusedView { get; set; }
    }
}
