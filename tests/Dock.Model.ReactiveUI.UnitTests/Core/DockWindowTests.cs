using Xunit;

namespace Dock.Model.ReactiveUI.UnitTests
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
