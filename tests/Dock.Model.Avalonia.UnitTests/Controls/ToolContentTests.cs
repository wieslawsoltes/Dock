using Dock.Model.Controls;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests.Controls
{
    public class ToolContentTests
    {
        [Fact]
        public void ToolContent_Ctor()
        {
            var actual = new ToolContent();
            Assert.NotNull(actual);
        }
    }
}
