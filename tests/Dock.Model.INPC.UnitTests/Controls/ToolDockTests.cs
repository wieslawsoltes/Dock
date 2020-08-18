using Dock.Model.Controls;
using Xunit;

namespace Dock.Model.INPC.UnitTests.Controls
{
    public class ToolDockTests
    {
        [Fact]
        public void ToolDock_Ctor()
        {
            var actual = new ToolDock();
            Assert.NotNull(actual);
        }
    }
}
