using Xunit;

namespace Dock.Model.UnitTests
{
    public class HostAdapterTests
    {
        [Fact]
        public void HostAdapter_Ctor()
        {
            var window = new TestWindow();
            var actual = new HostAdapter(window);
            Assert.NotNull(actual);
        }
    }
}
