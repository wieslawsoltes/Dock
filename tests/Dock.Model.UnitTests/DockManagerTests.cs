using Dock.Model;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using Xunit;

namespace Dock.Model.UnitTests;

public class DockManagerTests
{
    [Fact]
    public void IsDockTargetVisible_NonFill_AlwaysTrue()
    {
        var manager = new DockManager();
        var source = new Document();
        var target = new Document();
        Assert.True(manager.IsDockTargetVisible(source, target, DockOperation.Left));
    }

    [Fact]
    public void IsDockTargetVisible_SameDockable_ReturnsFalse()
    {
        var manager = new DockManager();
        var doc = new Document();
        Assert.False(manager.IsDockTargetVisible(doc, doc, DockOperation.Fill));
    }

    [Fact]
    public void IsDockTargetVisible_TargetIsOwner_ReturnsFalse()
    {
        var manager = new DockManager();
        var owner = new DocumentDock();
        var doc = new Document { Owner = owner };
        Assert.False(manager.IsDockTargetVisible(doc, owner, DockOperation.Fill));
    }

    [Fact]
    public void IsDockTargetVisible_Default_ReturnsTrue()
    {
        var manager = new DockManager();
        var owner = new DocumentDock();
        var doc1 = new Document { Owner = owner };
        var doc2 = new Document();
        Assert.True(manager.IsDockTargetVisible(doc1, doc2, DockOperation.Fill));
    }
}
