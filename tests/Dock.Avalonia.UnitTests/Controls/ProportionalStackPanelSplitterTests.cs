using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls
{
    public class ProportionalStackPanelSplitterTests
    {
        [Fact]
        public void ProportionalStackPanelSplitter_Ctor()
        {
            var actual = new ProportionalStackPanelSplitter();
            Assert.NotNull(actual);
        }
    }
}
