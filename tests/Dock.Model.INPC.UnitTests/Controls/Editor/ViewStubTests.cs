// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Dock.Model.Controls.Editor;
using Xunit;

namespace Dock.Model.INPC.UnitTests.Controls.Editor
{
    public class ViewStubTests
    {
        [Fact]
        public void ViewStub_Ctor()
        {
            var actual = new ViewStub();
            Assert.NotNull(actual);
        }
    }
}
