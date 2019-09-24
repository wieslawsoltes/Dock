// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Dock.Model.Controls
{
    public class SplitterDock : DockBase, ISplitterDock
    {
        public override IDockable? Clone()
        {
            throw new NotImplementedException();
        }
    }
}
