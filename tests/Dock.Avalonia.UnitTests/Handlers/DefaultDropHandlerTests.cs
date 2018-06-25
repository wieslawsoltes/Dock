// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Xunit;

namespace Dock.Avalonia.UnitTests
{
    public class DefaultDropHandlerTests
    {
        [Fact]
        public void TestDropHandler_Ctor()
        {
            var actual = new DockDragHandler();
            Assert.NotNull(actual);
        }
    }

    public class TestDropHandler : DefaultDropHandler
    {
    }
}
