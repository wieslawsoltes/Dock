// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia;
using Xunit;

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
    }
}
