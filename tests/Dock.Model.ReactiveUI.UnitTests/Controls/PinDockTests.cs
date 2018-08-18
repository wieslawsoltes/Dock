// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Dock.Model.Controls;
using Xunit;

namespace Dock.Model.ReactiveUI.UnitTests.Controls
{
    public class PinDockTests
    {
        [Fact]
        public void PinDock_Ctor()
        {
            var actual = new PinDock();
            Assert.NotNull(actual);
        }
    }
}
