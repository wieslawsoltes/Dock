using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls
{
    public class NavigationControlTests
    {
        [Fact]
        public void NavigationControl_Ctor()
        {
            var actual = new NavigationControl();
            Assert.NotNull(actual);
        }
    }
}
