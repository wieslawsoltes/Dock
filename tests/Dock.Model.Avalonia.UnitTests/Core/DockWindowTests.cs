using Xunit;

namespace Dock.Model.Avalonia.UnitTests
{
    public class DockWindowTests
    {
        [Fact]
        public void DockWindow_Ctor()
        {
            var actual = new DockWindow();
            Assert.NotNull(actual);
        }
    }
}
