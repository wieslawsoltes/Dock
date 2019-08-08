// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls
{
    public class DocumentControlTests
    {
        [Fact]
        public void DocumentControl_Ctor()
        {
            var actual = new DocumentControl();
            Assert.NotNull(actual);
        }
    }
}
