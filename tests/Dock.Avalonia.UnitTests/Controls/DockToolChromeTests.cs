using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls
{
    public class DockToolChromeTests
    {
        [Fact]
        public void DockToolChrome_Ctor()
        {
            var actual = new DockToolChrome();
            Assert.NotNull(actual);
        }
    }
}
