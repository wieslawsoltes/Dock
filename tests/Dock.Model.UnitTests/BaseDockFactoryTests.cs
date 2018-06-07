// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Xunit;

namespace Dock.Model.UnitTests
{
    public class BaseDockFactoryTests
    {
        [Fact]
        public void TestDockFactory_Ctor()
        {
            var actual = new TestDockFactory();
            Assert.NotNull(actual);
        }
    }
}
