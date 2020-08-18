using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls
{
    public class HostWindowTests
    {
        [Fact(Skip = "Need to initialize Avalonia first")]
        public void HostWindow_Ctor()
        {
            var actual = new HostWindow();
            Assert.NotNull(actual);
        }
    }
}
