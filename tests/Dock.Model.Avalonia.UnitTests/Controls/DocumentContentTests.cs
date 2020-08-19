using Dock.Model.Controls;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests.Controls
{
    public class DocumentContentTests
    {
        [Fact]
        public void DocumentContent_Ctor()
        {
            var actual = new DocumentContent();
            Assert.NotNull(actual);
        }
    }
}
