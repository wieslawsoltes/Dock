using System;
using Xunit;

namespace Dock.Model.INPC.UnitTests
{
    public class DockableBaseTests
    {
        [Fact]
        public void TestDockableBase_Ctor()
        {
            var actual = new TestDockableBase();
            Assert.NotNull(actual);
            Assert.Equal(string.Empty, actual.Id);
            Assert.Equal(string.Empty, actual.Title);
        }
    }

    public class TestDockableBase : DockableBase
    {
        public override IDockable? Clone()
        {
            throw new NotImplementedException();
        }
    }
}
