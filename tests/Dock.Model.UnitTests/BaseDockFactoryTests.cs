// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using Dock.Model.Controls;
using Xunit;

namespace Dock.Model.UnitTests
{
    public class BaseDockFactoryTests
    {
        [Fact]
        public void TestDockFactory_Ctor()
        {
            var actual = new TestDockFactory();
            Assert.NotNull(actual);
        }
    }

    public class TestDockFactory : DockFactoryBase
    {
        public override IList<T> CreateList<T>(params T[] items)
        {
            return new List<T>(items);
        }

        public override IRootDock CreateRootDock()
        {
            throw new NotImplementedException();
        }

        public override IPinDock CreatePinDock()
        {
            throw new NotImplementedException();
        }

        public override ILayoutDock CreateLayoutDock()
        {
            throw new NotImplementedException();
        }

        public override ISplitterDock CreateSplitterDock()
        {
            throw new NotImplementedException();
        }

        public override IToolDock CreateToolDock()
        {
            throw new NotImplementedException();
        }

        public override IDocumentDock CreateDocumentDock()
        {
            throw new NotImplementedException();
        }

        public override IDockWindow CreateDockWindow()
        {
            throw new NotImplementedException();
        }

        public override IToolTab CreateToolTab()
        {
            throw new NotImplementedException();
        }

        public override IDocumentTab CreateDocumentTab()
        {
            throw new NotImplementedException();
        }

        public override IDock CreateLayout()
        {
            throw new NotImplementedException();
        }
    }
}
