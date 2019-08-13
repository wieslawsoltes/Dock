// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Dock.Model.Controls;
using Xunit;

namespace Dock.Model.INPC.UnitTests.Controls
{
    public class ProportionalDockTests
    {
        [Fact]
        public void ProportionalDock_Ctor()
        {
            var actual = new ProportionalDock();
            Assert.NotNull(actual);
        }
    }
}
