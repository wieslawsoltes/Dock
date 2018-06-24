// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Xunit;

namespace Dock.Avalonia.UnitTests
{
    public class DockDragHandlerTests
    {
        [Fact]
        public void DockDragHandler_Ctor()
        {
            var actual = new DockDragHandler();
            Assert.NotNull(actual);
        }
    }
}
