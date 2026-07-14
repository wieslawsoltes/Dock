// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Avalonia;
using Xunit;

namespace Dock.Model.UnitTests;

public class FactoryCloseDockableTests
{
    [Fact]
    public void CloseDockable_DoesNothing_WhenDockableIsNull()
    {
        var factory = new Factory();

        var exception = Record.Exception(() => factory.CloseDockable(null));

        Assert.Null(exception);
    }
}
