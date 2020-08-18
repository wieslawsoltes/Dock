using Xunit;

namespace Dock.Model.UnitTests
{
    public class NavigateAdapterTests
    {
        [Fact]
        public void NavigateAdapter_Ctor()
        {
            var dock = new TestDock();
            var actual = new NavigateAdapter(dock);
            Assert.NotNull(actual);
        }
    }
}
