// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dock.Model.Controls
{
    public class RootDock : DockBase, IRootDock
    {
        public IDockWindow Window { get; set; }

        public IPinDock Top { get; set; }

        public IPinDock Bottom { get; set; }

        public IPinDock Left { get; set; }

        public IPinDock Right { get; set; }
    }
}
