using Dock.Model.Controls;
using Xunit;

namespace Dock.Model.ReactiveUI.UnitTests.Controls
{
    public class PinDockTests
    {
        [Fact]
        public void PinDock_Ctor()
        {
            var actual = new PinDock();
            Assert.NotNull(actual);
        }
    }
}
