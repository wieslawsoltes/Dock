// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
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
