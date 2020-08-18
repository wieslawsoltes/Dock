using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls
{
    public class DockPanelSplitterTests
    {
        [Fact]
        public void DockPanelSplitter_Ctor()
        {
            var actual = new DockPanelSplitter();
            Assert.NotNull(actual);
        }
    }
}
