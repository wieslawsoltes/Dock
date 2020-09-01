using System.Collections.Generic;
using Xunit;
using Dock.Model.Controls;

namespace Dock.Model.UnitTests
{
    public class FactoryBaseTests
    {
        [Fact]
        public void CreateList_Creates_List()
        {
            var factory = new TestFactory();
            var actual = factory.CreateList<IDockable>();
            Assert.NotNull(actual);
            Assert.IsType<List<IDockable>>(actual);
        }

        [Fact]
        public void CreateRootDock_Creates_RootDock()
        {
            var factory = new TestFactory();
            var actual = factory.CreateRootDock();
            Assert.NotNull(actual);
            Assert.IsType<RootDock>(actual);
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
}
