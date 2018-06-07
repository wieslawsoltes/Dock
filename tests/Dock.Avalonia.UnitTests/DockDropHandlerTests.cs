// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Xunit;

namespace Dock.Avalonia.UnitTests
{
    public class DockDropHandlerTests
    {
        [Fact]
        public void DockDropHandler_Ctor()
        {
            var actual = new DockDropHandler();
            Assert.NotNull(actual);
        }
    }
}
