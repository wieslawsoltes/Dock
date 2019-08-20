// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Dock.Model.Controls;
using Xunit;

namespace Dock.Model.INPC.UnitTests
{
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
}
