// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Dock.Model.Controls;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests.Controls
{
    public class RootDockTests
    {
        [Fact]
        public void RootDock_Ctor()
        {
            var actual = new RootDock();
            Assert.NotNull(actual);
        }
    }
}
