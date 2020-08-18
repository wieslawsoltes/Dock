using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls
{
    public class MetroWindowTests
    {
        [Fact(Skip = "Need to initialize Avalonia first")]
        public void MetroWindow_Ctor()
        {
            var actual = new MetroWindow();
            Assert.NotNull(actual);
        }
    }
}
