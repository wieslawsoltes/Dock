using Xunit;

namespace Dock.Model.UnitTests
{
    public class DockPointTests
    {
        [Fact]
        public void DockPoint_Ctor()
        {
            var actual = new DockPoint(50, 100);
            Assert.Equal(50, actual.X);
            Assert.Equal(100, actual.Y);
        }

        [Fact]
        public void DockPoint_ToString()
        {
            var actual = new DockPoint(50, 100).ToString();
            Assert.Equal("50, 100", actual);
        }
    }
}
