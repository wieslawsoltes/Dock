// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls
{
    public class MetroWindowTests
    {
        [Fact(Skip = "Need to initialize Avalonia first")]
        public void MetroWindow_Ctor()
        {
            var actual = new MetroWindow();
            Assert.NotNull(actual);
        }
    }
}
