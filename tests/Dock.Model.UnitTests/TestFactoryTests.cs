using System;
using Dock.Model.Controls;
using Xunit;

namespace Dock.Model.UnitTests
{
    public class TestFactoryTests
    {
        [Fact]
        public void TestFactory_Ctor()
        {
            var actual = new TestFactory();
            Assert.NotNull(actual);
        }

        [Fact]
        public void CreateLayout_Creates_Layout()
        {
            var factory = new TestFactory();
            var actual = factory.CreateLayout();
            Assert.NotNull(actual);
            Assert.IsType<RootDock>(actual);
        }

        [Fact]
        public void InitLayout_Initializes_Layout()
        {
            var actual = new TestFactory();
            var layout = actual.CreateLayout();
            Assert.NotNull(layout);
            if (layout == null)
            {
                throw new Exception("The CreateLayout method returned null.");
            }
            actual.InitLayout(layout);
            Assert.NotNull(actual);
            Assert.NotNull(actual.ContextLocator);
            Assert.NotNull(actual.HostWindowLocator);
        }
    }
}
