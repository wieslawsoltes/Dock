using System;
using Avalonia.Controls.Recycling.Model;
using Xunit;

namespace Dock.Controls.Recycling.Model.UnitTests;

public class ControlRecyclingVersionTests
{
    [Fact]
    public void Version_Requires_A_Source_Identity()
    {
        Assert.Throws<ArgumentNullException>(() => new ControlRecyclingVersion(null!, 0));
    }

    [Fact]
    public void Advancing_A_Revision_Preserves_The_Source_And_Previous_Snapshot()
    {
        var source = new object();
        var previous = new ControlRecyclingVersion(source, 7);
        var current = new ControlRecyclingVersion(previous.Source, previous.Revision + 1);

        Assert.Same(source, previous.Source);
        Assert.Same(previous.Source, current.Source);
        Assert.Equal(7, previous.Revision);
        Assert.Equal(8, current.Revision);
    }
}
