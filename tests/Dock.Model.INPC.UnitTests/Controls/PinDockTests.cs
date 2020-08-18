using Dock.Model.Controls;
using Xunit;

namespace Dock.Model.INPC.UnitTests.Controls
{
    public class PinDockDockTests
    {
        [Fact]
        public void PinDock_Ctor()
        {
            var actual = new PinDock();
            Assert.NotNull(actual);
        }
    }
}
