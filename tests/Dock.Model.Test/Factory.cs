// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using Dock.Model.Controls;

namespace Dock.Model
{
    public abstract class Factory : FactoryBase
    {
        public override IList<T> CreateList<T>(params T[] items) => new List<T>(items);

        public override IRootDock CreateRootDock() => new RootDock();

        public override IPinDock CreatePinDock() => new PinDock();

        public override IProportionalDock CreateProportionalDock() => new ProportionalDock();

        public override ISplitterDock CreateSplitterDock() => new SplitterDock();

        public override IToolDock CreateToolDock() => new ToolDock();

        public override IDocumentDock CreateDocumentDock() => new DocumentDock();

        public override IDockWindow CreateDockWindow() => new DockWindow();
    }
}
