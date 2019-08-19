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
    }

    public class CloneHelperTests
    {
        [Fact]
        public void CloneRootDock_Clone_Returns_RootDock()
        {
            var factory = new TestFactory();
            var source = factory.CreateRootDock();
            factory.UpdateDockable(source, null);
            var actual = source.Clone();
            Assert.NotNull(actual);
            Assert.IsType<RootDock>(actual);
        }

        [Fact]
        public void ClonePinDock_Clone_Returns_PinDock()
        {
            var factory = new TestFactory();
            var source = factory.CreatePinDock();
            factory.UpdateDockable(source, null);
            var actual = source.Clone();
            Assert.NotNull(actual);
            Assert.IsType<PinDock>(actual);
        }

        [Fact]
        public void CloneProportionalDock_Clone_Returns_ProportionalDock()
        {
            var factory = new TestFactory();
            var source = factory.CreateProportionalDock();
            factory.UpdateDockable(source, null);
            var actual = source.Clone();
            Assert.NotNull(actual);
            Assert.IsType<ProportionalDock>(actual);
        }

        [Fact]
        public void CloneSplitterDock_Clone_Returns_SplitterDock()
        {
            var factory = new TestFactory();
            var source = factory.CreateSplitterDock();
            factory.UpdateDockable(source, null);
            var actual = source.Clone();
            Assert.NotNull(actual);
            Assert.IsType<SplitterDock>(actual);
        }

        [Fact]
        public void CloneToolDock_Clones_ToolDock()
        {
            var factory = new TestFactory();
            var source = factory.CreateToolDock();
            factory.UpdateDockable(source, null);
            var actual = source.Clone();
            Assert.NotNull(actual);
            Assert.IsType<ToolDock>(actual);
        }

        [Fact]
        public void CloneDocumentDock_Clone_Returns_DocumentDock()
        {
            var factory = new TestFactory();
            var source = factory.CreateDocumentDock();
            factory.UpdateDockable(source, null);
            var actual = source.Clone();
            Assert.NotNull(actual);
            Assert.IsType<DocumentDock>(actual);
        }

        [Fact]
        public void CloneDockWindow_Clone_Returns_DockWindow()
        {
            var factory = new TestFactory();
            var source = factory.CreateDockWindow();
            factory.UpdateDockable(source, null);
            var actual = source.Clone();
            Assert.NotNull(actual);
            Assert.IsType<DockWindow>(actual);
        }

        [Fact]
        public void Document_Clone_Returns_this()
        {
            var factory = new TestFactory();
            var source = new TestDocument();
            factory.UpdateDockable(source, null);
            var actual = source.Clone();
            Assert.NotNull(actual);
            Assert.Equal(source, actual);
        }

        [Fact]
        public void Tool_Clone_Returns_this()
        {
            var factory = new TestFactory();
            var source = new TestTool();
            factory.UpdateDockable(source, null);
            var actual = source.Clone();
            Assert.NotNull(actual);
            Assert.Equal(source, actual);
        }
    }

    public class TestDocument : Document
    {
    }

    public class TestTool : Tool
    {
    }

    public class TestFactory : Factory
    {
        public override IDock CreateLayout()
        {
            throw new NotImplementedException();
        }
    }
}
