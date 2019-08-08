// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls
{
    public class DockToolChromeTests
    {
        [Fact]
        public void DockToolChrome_Ctor()
        {
            var actual = new DockToolChrome();
            Assert.NotNull(actual);
        }
    }
}
