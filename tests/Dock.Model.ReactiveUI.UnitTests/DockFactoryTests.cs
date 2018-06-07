// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Xunit;

namespace Dock.Model.ReactiveUI.UnitTests
{
    public class DockFactoryTests
    {
        [Fact]
        public void TestDockFactory_Ctor()
        {
            var actual = new TestDockFactory();
            Assert.NotNull(actual);
        }
    }

    public class TestDockFactory : DockFactory
    {
        public override IDock CreateLayout()
        {
            throw new System.NotImplementedException();
        }
    }
}
