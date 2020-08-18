using System;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests
{
    public class DockBaseTests
    {
        [Fact]
        public void TestDockBase_Ctor()
        {
            var actual = new TestDockBase();
            Assert.NotNull(actual);
        }
    }

    public class TestDockBase : DockBase
    {
        public override IDockable? Clone()
        {
            throw new NotImplementedException();
        }
    }
}
