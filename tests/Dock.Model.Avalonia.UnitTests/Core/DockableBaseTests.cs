// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests
{
    public class DockableBaseTests
    {
        [Fact]
        public void TestDockableBase_Ctor()
        {
            var actual = new TestDockableBase();
            Assert.NotNull(actual);
            Assert.Equal(string.Empty, actual.Id);
            Assert.Equal(string.Empty, actual.Title);
        }
    }

    public class TestDockableBase : DockableBase
    {
        public override IDockable? Clone()
        {
            throw new NotImplementedException();
        }
    }
}
