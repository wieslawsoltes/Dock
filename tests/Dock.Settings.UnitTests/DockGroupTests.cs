using Avalonia.Controls;
using Dock.Settings;
using Xunit;

namespace Dock.Settings.UnitTests;

/// <summary>
/// Unit tests for DockGroup attached property functionality.
/// </summary>
public class DockGroupTests
{
    [Fact]
    public void DockGroup_Property_Should_Default_To_Null()
    {
        var control = new Border();
        var dockGroup = DockProperties.GetDockGroup(control);
        
        Assert.Null(dockGroup);
    }

    [Fact]
    public void DockGroup_Property_Should_Set_And_Get_Value()
    {
        var control = new Border();
        const string groupName = "TestGroup";
        
        DockProperties.SetDockGroup(control, groupName);
        var result = DockProperties.GetDockGroup(control);
        
        Assert.Equal(groupName, result);
    }

    [Fact]
    public void DockGroup_Property_Should_Accept_Null_Value()
    {
        var control = new Border();
        
        DockProperties.SetDockGroup(control, "TestGroup");
        DockProperties.SetDockGroup(control, null);
        var result = DockProperties.GetDockGroup(control);
        
        Assert.Null(result);
    }

    [Fact]
    public void DockGroup_Property_Should_Accept_Empty_String()
    {
        var control = new Border();
        
        DockProperties.SetDockGroup(control, string.Empty);
        var result = DockProperties.GetDockGroup(control);
        
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void DockGroup_Property_Should_Inherit_From_Parent()
    {
        var parent = new StackPanel();
        var child = new Border();
        const string groupName = "InheritedGroup";
        
        parent.Children.Add(child);
        DockProperties.SetDockGroup(parent, groupName);
        
        var childGroup = DockProperties.GetDockGroup(child);
        
        Assert.Equal(groupName, childGroup);
    }

    [Fact]
    public void DockGroup_Property_Should_Override_Inherited_Value()
    {
        var parent = new StackPanel();
        var child = new Border();
        const string parentGroup = "ParentGroup";
        const string childGroup = "ChildGroup";
        
        parent.Children.Add(child);
        DockProperties.SetDockGroup(parent, parentGroup);
        DockProperties.SetDockGroup(child, childGroup);
        
        var result = DockProperties.GetDockGroup(child);
        
        Assert.Equal(childGroup, result);
    }

    [Fact]
    public void DockGroup_Property_Should_Inherit_Through_Multiple_Levels()
    {
        var grandparent = new StackPanel();
        var parent = new StackPanel();
        var child = new Border();
        const string groupName = "MultiLevelGroup";
        
        grandparent.Children.Add(parent);
        parent.Children.Add(child);
        DockProperties.SetDockGroup(grandparent, groupName);
        
        var childGroup = DockProperties.GetDockGroup(child);
        
        Assert.Equal(groupName, childGroup);
    }

    [Fact]
    public void DockGroup_Property_Should_Use_Closest_Ancestor_Value()
    {
        var grandparent = new StackPanel();
        var parent = new StackPanel();
        var child = new Border();
        const string grandparentGroup = "GrandparentGroup";
        const string parentGroup = "ParentGroup";
        
        grandparent.Children.Add(parent);
        parent.Children.Add(child);
        DockProperties.SetDockGroup(grandparent, grandparentGroup);
        DockProperties.SetDockGroup(parent, parentGroup);
        
        var childGroup = DockProperties.GetDockGroup(child);
        
        Assert.Equal(parentGroup, childGroup);
    }

    [Fact]
    public void DockGroup_Property_Should_Be_Case_Sensitive()
    {
        var control = new Border();
        const string groupName = "TestGroup";
        const string differentCase = "testgroup";
        
        DockProperties.SetDockGroup(control, groupName);
        var result = DockProperties.GetDockGroup(control);
        
        Assert.Equal(groupName, result);
        Assert.NotEqual(differentCase, result);
    }

    [Fact]
    public void DockGroup_Property_Should_Handle_Whitespace_Values()
    {
        var control = new Border();
        const string whitespaceGroup = "  Test Group  ";
        
        DockProperties.SetDockGroup(control, whitespaceGroup);
        var result = DockProperties.GetDockGroup(control);
        
        Assert.Equal(whitespaceGroup, result);
    }
}
