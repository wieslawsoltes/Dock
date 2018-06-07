// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Xunit;

namespace Dock.Model.ReactiveUI.UnitTests
{
    public class ViewBaseTests
    {
        [Fact]
        public void TestViewBase_Ctor()
        {
            var actual = new TestViewBase();
            Assert.NotNull(actual);
        }
    }

    public class TestViewBase : ViewBase
    {
    }
}
