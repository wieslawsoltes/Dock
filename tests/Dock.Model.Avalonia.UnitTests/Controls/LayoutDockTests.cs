// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Dock.Model.Controls;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests.Controls
{
    public class LayoutDockTests
    {
        [Fact(Skip="Avalonia issue")]
        public void LayoutDock_Ctor()
        {
            var actual = new LayoutDock();
            Assert.NotNull(actual);
        }
    }
}
