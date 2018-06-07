// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Dock.Model.Controls;
using Xunit;

namespace Dock.Model.ReactiveUI.UnitTests.Controls
{
    public class ToolTabTests
    {
        [Fact]
        public void ToolTab_Ctor()
        {
            var actual = new ToolTab();
            Assert.NotNull(actual);
        }
    }
}
