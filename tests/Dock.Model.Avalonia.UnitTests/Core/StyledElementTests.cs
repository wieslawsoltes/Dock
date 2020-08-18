using Xunit;
using Avalonia;

namespace Dock.Model.Avalonia.UnitTests
{
    public class StyledElementTests
    {
        [Fact]
        public void TestStyledElement_Ctor()
        {
            var actual = new TestStyledElement();
            Assert.NotNull(actual);
        }
    }

    public class TestStyledElement : StyledElement
    {
        public static readonly StyledProperty<object> TestProperty =
            AvaloniaProperty.Register<TestStyledElement, object>(nameof(TestProperty));

        public object Test
        {
            get => GetValue(TestProperty);
            set => SetValue(TestProperty, value);
        }
    }
}
