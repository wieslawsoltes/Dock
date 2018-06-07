// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Dock.Model.Controls.Editor;
using Xunit;

namespace Dock.Model.ReactiveUI.UnitTests.Controls.Editor
{
    public class DockStubTests
    {
        [Fact]
        public void DockStub_Ctor()
        {
            var actual = new DockStub();
            Assert.NotNull(actual);
        }
    }
}
