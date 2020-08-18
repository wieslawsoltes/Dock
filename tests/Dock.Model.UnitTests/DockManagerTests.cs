using Xunit;

namespace Dock.Model.UnitTests
{
    public class DockManagerTests
    {
        [Fact]
        public void DockManager_Ctor()
        {
            var actual = new DockManager();
            Assert.NotNull(actual);
        }

        [Fact]
        public void Validate_sourceDockable_Null()
        {
            var manager = new DockManager();
#pragma warning disable CS8625
            var actual = manager.ValidateDockable(null, null, DragAction.Move, DockOperation.Fill, false);
#pragma warning restore CS8625
            Assert.False(actual);
        }
    }
}
