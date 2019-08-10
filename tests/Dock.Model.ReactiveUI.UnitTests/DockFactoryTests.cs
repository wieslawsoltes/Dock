// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Dock.Model.Controls;
using ReactiveUI.Legacy;
using Xunit;

namespace Dock.Model.ReactiveUI.UnitTests
{
    public class DockFactoryTests
    {
        [Fact]
        public void TestDockFactory_Ctor()
        {
            var actual = new TestDockFactory();
            Assert.NotNull(actual);
        }

        [Fact]
        public void CreateList_Creates_ReactiveList_Empty()
        {
            var factory = new TestDockFactory();
            var actual = factory.CreateList<IView>();
            Assert.NotNull(actual);
#pragma warning disable CS0618 // Type or member is obsolete
            Assert.IsType<ReactiveList<IView>>(actual);
#pragma warning restore CS0618 // Type or member is obsolete
            Assert.Equal(0, actual.Count);
        }

        [Fact]
        public void CreateRootDock_Creates_RootDock_Type()
        {
            var factory = new TestDockFactory();
            var actual = factory.CreateRootDock();
            Assert.NotNull(actual);
            Assert.IsType<RootDock>(actual);
        }

        [Fact]
        public void CreatePinDock_Creates_PinDock()
        {
            var factory = new TestDockFactory();
            var actual = factory.CreatePinDock();
            Assert.NotNull(actual);
            Assert.IsType<PinDock>(actual);
        }

        [Fact]
        public void CreateLayoutDock_Creates_LayoutDock()
        {
            var factory = new TestDockFactory();
            var actual = factory.CreateLayoutDock();
            Assert.NotNull(actual);
            Assert.IsType<LayoutDock>(actual);
        }

        [Fact]
        public void CreateSplitterDock_Creates_SplitterDock()
        {
            var factory = new TestDockFactory();
            var actual = factory.CreateSplitterDock();
            Assert.NotNull(actual);
            Assert.IsType<SplitterDock>(actual);
        }

        [Fact]
        public void CreateToolDock_Creates_ToolDock()
        {
            var factory = new TestDockFactory();
            var actual = factory.CreateToolDock();
            Assert.NotNull(actual);
            Assert.IsType<ToolDock>(actual);
        }

        [Fact]
        public void CreateDocumentDock_Creates_DocumentDock()
        {
            var factory = new TestDockFactory();
            var actual = factory.CreateDocumentDock();
            Assert.NotNull(actual);
            Assert.IsType<DocumentDock>(actual);
        }

        [Fact]
        public void CreateDockWindow_Creates_DockWindow()
        {
            var factory = new TestDockFactory();
            var actual = factory.CreateDockWindow();
            Assert.NotNull(actual);
            Assert.IsType<DockWindow>(actual);
        }

        [Fact]
        public void CreateToolTab_Creates_ToolTab()
        {
            var factory = new TestDockFactory();
            var actual = factory.CreateToolTab();
            Assert.NotNull(actual);
            Assert.IsType<ToolTab>(actual);
        }

        [Fact]
        public void CreateDocumentTab_Creates_DocumentTab()
        {
            var factory = new TestDockFactory();
            var actual = factory.CreateDocumentTab();
            Assert.NotNull(actual);
            Assert.IsType<DocumentTab>(actual);
        }
    }

    public class TestDockFactory : DockFactory
    {
        public override IDock CreateLayout()
        {
            throw new NotImplementedException();
        }
    }
}
