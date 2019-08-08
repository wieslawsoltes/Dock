// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Xunit;
using Avalonia;

namespace Dock.Model.Avalonia.UnitTests
{
    public class AvaloniaObjectTests
    {
        [Fact]
        public void TestAvaloniaObject_Ctor()
        {
            var actual = new TestAvaloniaObject();
            Assert.NotNull(actual);
        }
    }

    public class TestAvaloniaObject : AvaloniaObject
    {
        public static readonly StyledProperty<object> TestProperty =
            AvaloniaProperty.Register<TestAvaloniaObject, object>(nameof(TestProperty));

        public object Test
        {
            get { return GetValue(TestProperty); }
            set { SetValue(TestProperty, value); }
        }
    }
}
