// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using Dock.Model.Controls;
using Xunit;

namespace Dock.Model.UnitTests
{
    public class BaseDockFactoryTests
    {
        [Fact]
        public void DockFactory_Ctor()
        {
            var actual = new DockFactory();
            Assert.NotNull(actual);
        }

        [Fact]
        public void CreateList_Creates_List()
        {
            var factory = new DockFactory();
            var actual = factory.CreateList<IView>();
            Assert.NotNull(actual);
            Assert.IsType<List<IView>>(actual);
        }

        [Fact]
        public void CreateRootDock_Creates_RootDock()
        {
            var factory = new DockFactory();
            var actual = factory.CreateRootDock();
            Assert.NotNull(actual);
            Assert.IsType<RootDock>(actual);
        }

        [Fact]
        public void CreatePinDock_Creates_PinDock()
        {
            var factory = new DockFactory();
            var actual = factory.CreatePinDock();
            Assert.NotNull(actual);
            Assert.IsType<PinDock>(actual);
        }

        [Fact]
        public void CreateLayoutDock_Creates_RootDock()
        {
            var factory = new DockFactory();
            var actual = factory.CreateLayoutDock();
            Assert.NotNull(actual);
            Assert.IsType<LayoutDock>(actual);
        }

        [Fact]
        public void CreateSplitterDock_Creates_SplitterDock()
        {
            var factory = new DockFactory();
            var actual = factory.CreateSplitterDock();
            Assert.NotNull(actual);
            Assert.IsType<SplitterDock>(actual);
        }

        [Fact]
        public void CreateToolDock_Creates_ToolDock()
        {
            var factory = new DockFactory();
            var actual = factory.CreateToolDock();
            Assert.NotNull(actual);
            Assert.IsType<ToolDock>(actual);
        }

        [Fact]
        public void CreateDocumentDock_Creates_DocumentDock()
        {
            var factory = new DockFactory();
            var actual = factory.CreateDocumentDock();
            Assert.NotNull(actual);
            Assert.IsType<DocumentDock>(actual);
        }

        [Fact]
        public void CreateDockWindow_Creates_DockWindow()
        {
            var factory = new DockFactory();
            var actual = factory.CreateDockWindow();
            Assert.NotNull(actual);
            Assert.IsType<DockWindow>(actual);
        }

        [Fact]
        public void CreateToolTab_Creates_ToolTab()
        {
            var factory = new DockFactory();
            var actual = factory.CreateToolTab();
            Assert.NotNull(actual);
            Assert.IsType<ToolTab>(actual);
        }

        [Fact]
        public void CreateDocumentTab_Creates_DocumentTab()
        {
            var factory = new DockFactory();
            var actual = factory.CreateDocumentTab();
            Assert.NotNull(actual);
            Assert.IsType<DocumentTab>(actual);
        }
    }
}
