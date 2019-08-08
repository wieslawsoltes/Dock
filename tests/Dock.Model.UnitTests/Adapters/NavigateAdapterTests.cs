// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Xunit;

namespace Dock.Model.UnitTests
{
    public class NavigateAdapterTests
    {
        [Fact]
        public void NavigateAdapter_Ctor()
        {
            var dock = new TestDock();
            var actual = new NavigateAdapter(dock);
            Assert.NotNull(actual);
        }
    }
}
