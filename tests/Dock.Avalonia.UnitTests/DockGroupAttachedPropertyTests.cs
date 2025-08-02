using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.UnitTests;

/// <summary>
/// Unit tests for DockGroup attached property functionality in Avalonia UI controls.
/// </summary>
public class DockGroupAttachedPropertyTests
{
    [AvaloniaFact]
    public void DockGroup_Property_Should_Default_To_Null()
    {
        var control = new Border();
        var dockGroup = DockProperties.GetDockGroup(control);
        
        Assert.Null(dockGroup);
    }

    [AvaloniaFact]
    public void DockGroup_Property_Should_Set_And_Get_Value()
    {
        var control = new Border();
        const string groupName = "TestGroup";
        
        DockProperties.SetDockGroup(control, groupName);
        var result = DockProperties.GetDockGroup(control);
        
        Assert.Equal(groupName, result);
    }

    [AvaloniaFact]
    public void DockGroup_Property_Should_Accept_Null_Value()
    {
        var control = new Border();
        
        DockProperties.SetDockGroup(control, "TestGroup");
        DockProperties.SetDockGroup(control, null);
        var result = DockProperties.GetDockGroup(control);
        
        Assert.Null(result);
    }

    [AvaloniaFact]
    public void DockGroup_Property_Should_Accept_Empty_String()
    {
        var control = new Border();
        
        DockProperties.SetDockGroup(control, string.Empty);
        var result = DockProperties.GetDockGroup(control);
        
        Assert.Equal(string.Empty, result);
    }

    [AvaloniaFact]
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

    [AvaloniaFact]
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

    [AvaloniaFact]
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

    [AvaloniaFact]
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

    [AvaloniaFact]
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

    [AvaloniaFact]
    public void DockGroup_Property_Should_Handle_Whitespace_Values()
    {
        var control = new Border();
        const string whitespaceGroup = "  Test Group  ";
        
        DockProperties.SetDockGroup(control, whitespaceGroup);
        var result = DockProperties.GetDockGroup(control);
        
        Assert.Equal(whitespaceGroup, result);
    }

    [AvaloniaFact]
    public void DockGroup_Property_Should_Work_With_Different_Control_Types()
    {
        const string groupName = "ControlTypeGroup";
        
        var border = new Border();
        var button = new Button();
        var textBlock = new TextBlock();
        var panel = new StackPanel();
        
        DockProperties.SetDockGroup(border, groupName);
        DockProperties.SetDockGroup(button, groupName);
        DockProperties.SetDockGroup(textBlock, groupName);
        DockProperties.SetDockGroup(panel, groupName);
        
        Assert.Equal(groupName, DockProperties.GetDockGroup(border));
        Assert.Equal(groupName, DockProperties.GetDockGroup(button));
        Assert.Equal(groupName, DockProperties.GetDockGroup(textBlock));
        Assert.Equal(groupName, DockProperties.GetDockGroup(panel));
    }

    [AvaloniaFact]
    public void DockGroup_Property_Should_Support_Property_Changed_Notifications()
    {
        var control = new Border();
        const string initialGroup = "InitialGroup";
        const string newGroup = "NewGroup";
        
        string? changedValue = null;
        bool propertyChanged = false;
        
        control.PropertyChanged += (sender, e) =>
        {
            if (e.Property == DockProperties.DockGroupProperty)
            {
                propertyChanged = true;
                changedValue = e.NewValue as string;
            }
        };
        
        DockProperties.SetDockGroup(control, initialGroup);
        
        Assert.True(propertyChanged);
        Assert.Equal(initialGroup, changedValue);
        
        propertyChanged = false;
        DockProperties.SetDockGroup(control, newGroup);
        
        Assert.True(propertyChanged);
        Assert.Equal(newGroup, changedValue);
    }

    [AvaloniaFact]
    public void DockGroup_Property_Should_Inherit_When_Parent_Added_After_Child()
    {
        var parent = new StackPanel();
        var child = new Border();
        const string groupName = "LateInheritGroup";
        
        // Set group on parent first
        DockProperties.SetDockGroup(parent, groupName);
        
        // Add child after setting parent group
        parent.Children.Add(child);
        
        var childGroup = DockProperties.GetDockGroup(child);
        
        Assert.Equal(groupName, childGroup);
    }

    [AvaloniaFact]
    public void DockGroup_Property_Should_Update_When_Parent_Group_Changes()
    {
        var parent = new StackPanel();
        var child = new Border();
        const string initialGroup = "InitialGroup";
        const string newGroup = "NewGroup";
        
        parent.Children.Add(child);
        DockProperties.SetDockGroup(parent, initialGroup);
        
        // Verify initial inheritance
        Assert.Equal(initialGroup, DockProperties.GetDockGroup(child));
        
        // Change parent group
        DockProperties.SetDockGroup(parent, newGroup);
        
        // Child should inherit the new group
        Assert.Equal(newGroup, DockProperties.GetDockGroup(child));
    }
}
