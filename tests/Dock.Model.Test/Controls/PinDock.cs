// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Dock.Model.Controls
{
    public class PinDock : DockBase, IPinDock
    {
        public Alignment Alignment { get; set; } = Alignment.Unset;

        public bool IsExpanded { get; set; } = false;

        public bool AutoHide { get; set; } = true;

        public override IDockable Clone()
        {
            throw new NotImplementedException();
        }
    }
}
