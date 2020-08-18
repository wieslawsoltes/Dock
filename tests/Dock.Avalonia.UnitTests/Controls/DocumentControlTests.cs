using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls
{
    public class DocumentControlTests
    {
        [Fact]
        public void DocumentControl_Ctor()
        {
            var actual = new DocumentControl();
            Assert.NotNull(actual);
        }
    }
}
