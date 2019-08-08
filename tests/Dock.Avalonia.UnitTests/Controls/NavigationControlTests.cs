// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls
{
    public class NavigationControlTests
    {
        [Fact]
        public void NavigationControl_Ctor()
        {
            var actual = new NavigationControl();
            Assert.NotNull(actual);
        }
    }
}
