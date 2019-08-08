// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls
{
    public class DockControlTests
    {
        [Fact]
        public void DockControl_Ctor()
        {
            var actual = new DockControl();
            Assert.NotNull(actual);
        }
    }
}
