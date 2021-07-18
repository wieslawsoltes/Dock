using System.Collections.ObjectModel;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using Dock.Model.ReactiveUI.Core;
using Xunit;

namespace Dock.Model.ReactiveUI.UnitTests
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
        public void CreateList_Creates_ReactiveList_Empty()
        {
            var factory = new TestFactory();
            var actual = factory.CreateList<IDockable>();
            Assert.NotNull(actual);
            Assert.IsType<ObservableCollection<IDockable>>(actual);
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
        public void CreateProportionalDock_Creates_ProportionalDock()
        {
            var factory = new TestFactory();
            var actual = factory.CreateProportionalDock();
            Assert.NotNull(actual);
            Assert.IsType<ProportionalDock>(actual);
        }

        [Fact]
        public void CreateDockDock_Creates_DockDockk()
        {
            var factory = new TestFactory();
            var actual = factory.CreateDockDock();
            Assert.NotNull(actual);
            Assert.IsType<DockDock>(actual);
            Assert.True(actual.LastChildFill);
        }

        [Fact]
        public void CreateSplitterDock_Creates_SplitterDock()
        {
            var factory = new TestFactory();
            var actual = factory.CreateSplitterDockable();
            Assert.NotNull(actual);
            Assert.IsType<SplitterDockable>(actual);
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
        public void CreateLayout_Creates_RootDock()
        {
            var factory = new TestFactory();
            var actual = factory.CreateLayout();
            Assert.NotNull(actual);
            Assert.IsType<RootDock>(actual);
        }
    }

    public class TestFactory : Factory
    {
    }
}
