// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Avalonia.Collections;
using Dock.Model.Controls;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests
{
    public class FactoryTests
    {
        [Fact]
        public void TestFactory_Ctor()
        {
            var actual = new TestFactory();
            Assert.NotNull(actual);
        }

        [Fact]
        public void CreateList_Creates_AvaloniaList_Empty()
        {
            var factory = new TestFactory();
            var actual = factory.CreateList<IDockable>();
            Assert.NotNull(actual);
            Assert.IsType<AvaloniaList<IDockable>>(actual);
            Assert.Equal(0, actual.Count);
        }

        [Fact]
        public void CreateRootDock_Creates_RootDock_Type()
        {
            var factory = new TestFactory();
            var actual = factory.CreateRootDock();
            Assert.NotNull(actual);
            Assert.IsType<RootDock>(actual);
        }

        [Fact]
        public void CreatePinDock_Creates_PinDock()
        {
            var factory = new TestFactory();
            var actual = factory.CreatePinDock();
            Assert.NotNull(actual);
            Assert.IsType<PinDock>(actual);
        }

        [Fact]
        public void CreateProportionalDock_Creates_ProportionalDock()
        {
            var factory = new TestFactory();
            var actual = factory.CreateProportionalDock();
            Assert.NotNull(actual);
            Assert.IsType<ProportionalDock>(actual);
        }

        [Fact]
        public void CreateSplitterDock_Creates_SplitterDock()
        {
            var factory = new TestFactory();
            var actual = factory.CreateSplitterDock();
            Assert.NotNull(actual);
            Assert.IsType<SplitterDock>(actual);
        }

        [Fact]
        public void CreateToolDock_Creates_ToolDock()
        {
            var factory = new TestFactory();
            var actual = factory.CreateToolDock();
            Assert.NotNull(actual);
            Assert.IsType<ToolDock>(actual);
        }

        [Fact]
        public void CreateDocumentDock_Creates_DocumentDock()
        {
            var factory = new TestFactory();
            var actual = factory.CreateDocumentDock();
            Assert.NotNull(actual);
            Assert.IsType<DocumentDock>(actual);
        }

        [Fact]
        public void CreateDockWindow_Creates_DockWindow()
        {
            var factory = new TestFactory();
            var actual = factory.CreateDockWindow();
            Assert.NotNull(actual);
            Assert.IsType<DockWindow>(actual);
        }

        [Fact]
        public void CreateTool_Creates_Tool()
        {
            var factory = new TestFactory();
            var actual = factory.CreateTool();
            Assert.NotNull(actual);
            Assert.IsType<Tool>(actual);
        }

        [Fact]
        public void CreateDocument_Creates_Document()
        {
            var factory = new TestFactory();
            var actual = factory.CreateDocument();
            Assert.NotNull(actual);
            Assert.IsType<Document>(actual);
        }
    }

    public class TestFactory : Factory
    {
        public override IDock CreateLayout()
        {
            throw new NotImplementedException();
        }
    }
}
