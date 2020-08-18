using Dock.Model.Controls;
using Xunit;

namespace Dock.Model.INPC.UnitTests.Controls
{
    public class SplitterDockTests
    {
        [Fact]
        public void SplitterDock_Ctor()
        {
            var actual = new SplitterDock();
            Assert.NotNull(actual);
        }
    }
}
