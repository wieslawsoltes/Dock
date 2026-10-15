using System.Collections.Generic;
using Avalonia.Headless.XUnit;
using AvalonDock;
using AvalonDock.Layout;
using Dock.Model.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class HiddenActivationContractTests
{
    [AvaloniaTheory]
    [InlineData("content")]
    [InlineData("root")]
    [InlineData("manager")]
    [InlineData("hide")]
    public void Explicit_Activation_Of_Hidden_Content_Retains_Its_Flag_Without_Showing_It(string api)
    {
        DockingManager manager = new();
        LayoutAnchorable tool = new();
        manager.Layout.Hidden.Add(tool);
        List<bool> changes = new();
        tool.IsActiveChanged += (_, _) => changes.Add(tool.IsActive);

        if (api == "content") tool.IsActive = true;
        else if (api == "root") manager.Layout.ActiveContent = tool;
        else if (api == "manager") manager.ActiveContent = tool;
        else tool.Hide();

        Assert.True(tool.IsActive);
        Assert.True(tool.IsSelected);
        Assert.True(tool.IsHidden);
        Assert.False(tool.IsVisible);
        Assert.Same(tool, manager.Layout.ActiveContent);
        Assert.Same(tool, Assert.Single(manager.Layout.Hidden));
        Assert.Same(tool.DockModel, Assert.Single(((RootDock)manager.Layout.DockModel).HiddenDockables!));
        Assert.False(((Tool)tool.DockModel).IsOpen);
        Assert.Equal(new[] { true }, changes);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Explicit_Deactivation_And_Focus_Changes_Can_Clear_A_Hidden_Active_Flag(bool focus)
    {
        LayoutDocument document = new() { Content = new object() };
        DockingManager manager = new() { Layout = new() { RootPanel = new(new LayoutDocumentPane(document)) } };
        LayoutAnchorable tool = new() { Content = new object() };
        manager.Layout.Hidden.Add(tool);
        tool.IsActive = true;
        Assert.True(tool.IsActive);

        if (focus) document.IsActive = true;
        else tool.IsActive = false;

        Assert.False(tool.IsActive);
        Assert.Same(focus ? (LayoutContent)document : tool, manager.Layout.ActiveContent);
        Assert.True(tool.IsHidden);
        if (!focus)
        {
            manager.Layout.Hidden[0] = tool;
            Assert.False(tool.IsActive);
            Assert.Same(tool, manager.Layout.ActiveContent);
        }
    }
}
