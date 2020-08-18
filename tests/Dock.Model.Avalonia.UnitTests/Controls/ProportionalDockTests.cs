using Dock.Model.Controls;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests.Controls
{
    public class ProportionalDockTests
    {
        [Fact]
        public void ProportionalDock_Ctor()
        {
            var actual = new ProportionalDock();
            Assert.NotNull(actual);
        }
    }
}
