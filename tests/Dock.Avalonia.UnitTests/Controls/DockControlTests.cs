using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls
{
    public class DockControlTests
    {
        [Fact]
        public void DockControl_Ctor()
        {
            var actual = new DockControl();
            Assert.NotNull(actual);
        }
    }
}
