// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Xunit;

namespace Dock.Model.INPC.UnitTests
{
    public class ReactiveObjectTests
    {
        [Fact]
        public void TestReactiveObject_Ctor()
        {
            var actual = new TestReactiveObject();
            Assert.NotNull(actual);
        }
    }

    public class TestReactiveObject : ReactiveObject
    {
    }
}
