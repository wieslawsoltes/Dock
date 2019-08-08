// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls
{
    public class HostWindowTests
    {
        [Fact(Skip = "Need to initialize Avalonia first")]
        public void HostWindow_Ctor()
        {
            var actual = new HostWindow();
            Assert.NotNull(actual);
        }
    }
}
