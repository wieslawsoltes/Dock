using System.Collections.Generic;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using Xunit;

namespace Dock.Model.UnitTests;

public class AlignmentHelperTests
{
    [Fact]
    public void AddToAlignmentList_AddsDockableToRightList()
    {
        var root = new RootDock();
        var tool = new Tool();

        AlignmentHelper.AddToAlignmentList(root, Alignment.Right, tool);

        Assert.Single(root.RightPinnedDockables!);
        Assert.Contains(tool, root.RightPinnedDockables!);
    }

    [Fact]
    public void RemoveFromAlignmentList_RemovesDockableFromList()
    {
        var root = new RootDock { RightPinnedDockables = new List<IDockable>() };
        var tool = new Tool();
        root.RightPinnedDockables!.Add(tool);

        AlignmentHelper.RemoveFromAlignmentList(root, Alignment.Right, tool);

        Assert.Empty(root.RightPinnedDockables);
    }
}

