// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Xunit;

namespace Dock.Model.UnitTests
{
    public class HostAdapterTests
    {
        [Fact]
        public void HostAdapter_Ctor()
        {
            var window = new TestWindow();
            var actual = new HostAdapter(window);
            Assert.NotNull(actual);
        }
    }
}
