using Dock.Model.Controls;
using Xunit;

namespace Dock.Model.INPC.UnitTests.Controls
{
    public class DocumentDockTests
    {
        [Fact]
        public void DocumentDock_Ctor()
        {
            var actual = new DocumentDock();
            Assert.NotNull(actual);
        }
    }
}
