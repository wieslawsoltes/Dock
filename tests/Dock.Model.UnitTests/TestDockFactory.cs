// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using Dock.Model.Controls;

namespace Dock.Model.UnitTests
{
    public class TestDockFactory : DockFactoryBase
    {
        public override IList<T> CreateList<T>(params T[] items)
        {
            throw new System.NotImplementedException();
        }

        public override IRootDock CreateRootDock()
        {
            throw new System.NotImplementedException();
        }

        public override ILayoutDock CreateLayoutDock()
        {
            throw new System.NotImplementedException();
        }

        public override ISplitterDock CreateSplitterDock()
        {
            throw new System.NotImplementedException();
        }

        public override IToolDock CreateToolDock()
        {
            throw new System.NotImplementedException();
        }

        public override IDocumentDock CreateDocumentDock()
        {
            throw new System.NotImplementedException();
        }

        public override IDockWindow CreateDockWindow()
        {
            throw new System.NotImplementedException();
        }

        public override IToolTab CreateToolTab()
        {
            throw new System.NotImplementedException();
        }

        public override IDocumentTab CreateDocumentTab()
        {
            throw new System.NotImplementedException();
        }

        public override IView CreateView()
        {
            throw new System.NotImplementedException();
        }

        public override IDock CreateDock()
        {
            throw new System.NotImplementedException();
        }

        public override IDock CreateLayout()
        {
            throw new System.NotImplementedException();
        }
    }
}
