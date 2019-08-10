// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using Dock.Model.Controls;

namespace Dock.Model
{
    public class DockFactory : DockFactoryBase
    {
        public override IList<T> CreateList<T>(params T[] items) => new List<T>(items);

        public override IRootDock CreateRootDock() => new RootDock();

        public override IPinDock CreatePinDock() => new PinDock();

        public override ILayoutDock CreateLayoutDock() => new LayoutDock();

        public override ISplitterDock CreateSplitterDock() => new SplitterDock();

        public override IToolDock CreateToolDock() => new ToolDock();

        public override IDocumentDock CreateDocumentDock() => new DocumentDock();

        public override IDockWindow CreateDockWindow() => new DockWindow();

        public override IToolTab CreateToolTab() => new ToolTab();

        public override IDocumentTab CreateDocumentTab() => new DocumentTab();

        public override IDock CreateLayout()
        {
            throw new NotImplementedException();
        }

        public override void InitLayout(IView layout)
        {
            var context = "Test";

            this.ContextLocator = new Dictionary<string, Func<object>>
            {
                [nameof(IRootDock)] = () => context,
                [nameof(IPinDock)] = () => context,
                [nameof(ILayoutDock)] = () => context,
                [nameof(IDocumentDock)] = () => context,
                [nameof(IToolDock)] = () => context,
                [nameof(ISplitterDock)] = () => context,
                [nameof(IDockWindow)] = () => context,
                [nameof(IDocumentTab)] = () => context,
                [nameof(IToolTab)] = () => context,
            };

            this.HostLocator = new Dictionary<string, Func<IDockHost>>
            {
                [nameof(IDockWindow)] = () => new HostWindow()
            };

            base.InitLayout(layout);
        }
    }
}
