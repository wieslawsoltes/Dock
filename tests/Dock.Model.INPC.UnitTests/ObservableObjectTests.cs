using Xunit;

namespace Dock.Model.INPC.UnitTests
{
    public class ObservableObjectTests
    {
        [Fact]
        public void TestObservableObject_Ctor()
        {
            var actual = new TestObservableObject();
            Assert.NotNull(actual);
        }
    }

    public class TestObservableObject : ObservableObject
    {
    }
}
