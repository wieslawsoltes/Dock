// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Xunit;

namespace Dock.Model.ReactiveUI.UnitTests
{
    public class DockBaseTests
    {
        [Fact]
        public void TestDockBase_Ctor()
        {
            var actual = new TestDockBase();
            Assert.NotNull(actual);
        }
    }

    public class TestDockBase : DockBase
    {
    }
}
