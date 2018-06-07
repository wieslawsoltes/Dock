// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Dock.Model.Controls;
using Xunit;

namespace Dock.Model.INPC.UnitTests.Controls
{
    public class DockStubTests
    {
        [Fact]
        public void DocumentDock_Ctor()
        {
            var actual = new DocumentDock();
            Assert.NotNull(actual);
        }
    }
}
