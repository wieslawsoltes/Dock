using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls
{
    public class DockTargetTests
    {
        [Fact]
        public void DockTarget_Ctor()
        {
            var actual = new DockTarget();
            Assert.NotNull(actual);
        }
    }
}
