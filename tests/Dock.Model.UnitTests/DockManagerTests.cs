// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Xunit;

namespace Dock.Model.UnitTests
{
    public class DockManagerTests
    {
        [Fact]
        public void DockManager_Ctor()
        {
            var actual = new DockManager();
            Assert.NotNull(actual);
        }

        [Fact]
        public void Validate_sourceDockable_Null()
        {
            var manager = new DockManager();
            var actual = manager.Validate(null, null, DragAction.Move, DockOperation.Fill, false);
            Assert.False(actual);
        }
    }
}
