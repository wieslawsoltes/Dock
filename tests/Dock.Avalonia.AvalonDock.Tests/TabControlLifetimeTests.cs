using System;
using System.IO;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public partial class TabControlTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Retained_Model_Does_Not_Retain_An_Unused_Custom_Tab(bool tool)
    {
        using Workspace workspace = new(tool);
        WeakReference<Control> tab = CreateUnusedTab(workspace.First, tool);
        Collect();
        Assert.False(tab.TryGetTarget(out _));
        GC.KeepAlive(workspace.First);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Retaining_A_Removed_Native_Tab_Does_Not_Retain_Its_Content_Or_Manager(bool tool)
    {
        var (tab, model, manager) = CreateRemovedNativeTab(tool);
        Assert.Null(Model(tab));
        Assert.Null(Item(tab));
        Collect();
        Assert.False(model.TryGetTarget(out _));
        Assert.False(manager.TryGetTarget(out _));
        GC.KeepAlive(tab);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Xml_Replacement_Clears_The_Old_Native_Header_And_Binds_The_New_One(bool tool)
    {
        using Workspace workspace = new(tool);
        Control old = NativeTab(workspace.Manager, workspace.First, tool);
        using StringWriter writer = new();
        XmlLayoutSerializer serializer = new(workspace.Manager);
        serializer.Serialize(writer);
        serializer.Deserialize(new StringReader(writer.ToString()));
        workspace.Update();
        Assert.Null(Model(old));
        Assert.Null(Item(old));
        LayoutContent replacement = Assert.Single(workspace.Manager.Layout.Descendents(), node => node is LayoutContent { Title: "First" }) as LayoutContent ?? throw new InvalidOperationException();
        Assert.NotSame(workspace.First, replacement);
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(replacement), Item(NativeTab(workspace.Manager, replacement, tool)));
        Assert.Same(workspace.First, Model(workspace.Tab));
        Assert.Null(Item(workspace.Tab));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void An_Explicit_Custom_Tab_Follows_The_Model_Between_Managers(bool tool)
    {
        using Workspace workspace = new(tool);
        LayoutDocumentPane destination = new();
        DockingManager manager = new() { Layout = new() { RootPanel = new LayoutPanel(destination) } };
        LayoutItem? previous = Item(workspace.Tab);
        destination.Children.Add(workspace.First);
        Assert.Same(workspace.First, Model(workspace.Tab));
        Assert.NotSame(previous, Item(workspace.Tab));
        Assert.Same(manager.GetLayoutItemFromModel(workspace.First), Item(workspace.Tab));
    }

    [AvaloniaFact]
    public void A_Tool_Docked_As_A_Document_Uses_The_Document_Tab_Control_And_The_Same_Tool_Item()
    {
        using Workspace workspace = new(true);
        LayoutItem? item = Item(workspace.Tab);
        workspace.First.DockAsDocument();
        workspace.Update();
        LayoutDocumentTabItem tab = Assert.IsType<LayoutDocumentTabItem>(NativeTab(workspace.Manager, workspace.First, false));
        Assert.Same(item, tab.LayoutItem);
        Assert.IsType<LayoutAnchorableItem>(tab.LayoutItem);
        Click(tab);
        Assert.Same(workspace.First, workspace.Manager.Layout.ActiveContent);
    }

    private static void Collect()
    {
        for (int index = 0; index < 3; index++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            GC.Collect(); GC.WaitForPendingFinalizers();
        }
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<Control> CreateUnusedTab(LayoutContent model, bool tool) =>
        new(tool ? new LayoutAnchorableTabItem { Model = model } : new LayoutDocumentTabItem { Model = model });
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (Control, WeakReference<LayoutContent>, WeakReference<DockingManager>) CreateRemovedNativeTab(bool tool)
    {
        using Workspace workspace = new(tool);
        Control tab = NativeTab(workspace.Manager, workspace.First, tool);
        WeakReference<LayoutContent> model = new(workspace.First);
        WeakReference<DockingManager> manager = new(workspace.Manager);
        workspace.Manager.Layout = new();
        workspace.Update();
        return (tab, model, manager);
    }
}
