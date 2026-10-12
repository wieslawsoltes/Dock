using System;
using System.Linq;
using System.Reactive.Concurrency;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.VisualTree;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using ReactiveUI.Reactive;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public partial class TabControlTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Custom_Close_Commands_Use_Explicit_Native_Policy_Precedence_For_Custom_And_Native_Tabs(bool native)
    {
        using Workspace workspace = new(false);
        workspace.First.CanClose = false;
        LayoutItem item = Item(workspace.Tab)!;
        System.Windows.Input.ICommand defaultClose = item.CloseCommand!;
        int calls = 0;
        using var command = ReactiveCommand.Create(() => calls++, outputScheduler: ImmediateScheduler.Instance);
        item.CloseCommand = command;
        IDock pane = (IDock)workspace.First.DockModel.Owner!;
        IRootDock root = (IRootDock)workspace.Manager.Layout.DockModel;
        Control tab = native ? NativeTab(workspace.Manager, workspace.First, false) : workspace.Tab;

        root.RootDockCapabilityPolicy = new DockCapabilityPolicy { CanClose = false };
        Click(tab, MouseButton.Middle);
        Assert.Equal(0, calls);
        pane.DockCapabilityPolicy = new DockCapabilityPolicy { CanClose = true };
        Click(tab, MouseButton.Middle);
        Assert.Equal(1, calls);
        workspace.First.DockModel.DockCapabilityOverrides = new DockCapabilityOverrides { CanClose = false };
        Click(tab, MouseButton.Middle);
        Assert.Equal(1, calls);
        workspace.First.DockModel.DockCapabilityOverrides = new DockCapabilityOverrides { CanClose = true };
        Click(tab, MouseButton.Middle);
        Assert.Equal(2, calls);

        root.RootDockCapabilityPolicy = null;
        pane.DockCapabilityPolicy = null;
        workspace.First.DockModel.DockCapabilityOverrides = null;
        Click(tab, MouseButton.Middle);
        Assert.Equal(3, calls);
        item.CloseCommand = defaultClose;
        Click(tab, MouseButton.Middle);
        Assert.Equal(3, calls);
        Assert.Same(workspace.Pane, workspace.First.Parent);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Preventing_Reordering_Still_Allows_Dragging_Outside_Custom_And_Native_Tab_Strips(bool tool, bool native)
    {
        using Workspace workspace = new(tool);
        if (tool) ((LayoutAnchorablePane)workspace.Pane).CanRepositionItems = false;
        else ((LayoutDocumentPane)workspace.Pane).CanRepositionItems = false;
        Control tab = native ? NativeTab(workspace.Manager, workspace.First, tool) : workspace.Tab;
        int began = 0;
        workspace.Manager.DockFactory.WindowMoveDragBegin += (_, args) => { began++; args.Cancel = true; };
        Point start = Center(tab);
        Point end = start + new Vector(0, tool && native ? -90 : 90);
        workspace.Window.MouseMove(start);
        workspace.Window.MouseDown(start, MouseButton.Left);
        workspace.Window.MouseMove(start + new Vector(12, 0), RawInputModifiers.LeftMouseButton);
        workspace.Window.MouseMove(end, RawInputModifiers.LeftMouseButton);
        workspace.Update();
        if (native)
        {
            // Native tabs hand the gesture to Dock's drag preview/drop state.
            Assert.True(workspace.Manager.GetVisualDescendants().OfType<DockControl>().Single().IsDraggingDock);
            Assert.Equal(new[] { workspace.First, workspace.Second }, workspace.Pane.Children.Cast<LayoutContent>());
        }
        else
        {
            Assert.True(workspace.First.IsFloating);
            Assert.False(workspace.Second.IsFloating);
            Assert.Equal(1, began);
            Assert.False(workspace.Manager.FloatingWindows.Single().IsDragging);
        }
        workspace.Window.MouseUp(end, MouseButton.Left);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Compiled_Custom_Tab_Row_Follows_Reordering_Without_Repeated_Pointer_Moves_Undoing_It(bool tool)
    {
        using Workspace workspace = new(tool);
        ItemsControl row = (ItemsControl)((IDataTemplate)workspace.Window.Resources[tool ? "BoundToolTabs" : "BoundDocumentTabs"]!).Build(workspace.Pane)!;
        row.DataContext = workspace.Pane;
        workspace.Tabs.Children.Clear();
        workspace.Tabs.Children.Add(row);
        workspace.Update();
        Control first = NativeTab(row, workspace.First, tool);
        Control second = NativeTab(row, workspace.Second, tool);
        LayoutItem? item = Item(first);
        Point start = Center(first);
        Point end = Center(second);
        workspace.Window.MouseMove(start);
        workspace.Window.MouseDown(start, MouseButton.Left);
        workspace.Window.MouseMove(start + new Vector(12, 0), RawInputModifiers.LeftMouseButton);
        workspace.Window.MouseMove(end, RawInputModifiers.LeftMouseButton);
        workspace.Window.MouseMove(end + new Vector(1, 0), RawInputModifiers.LeftMouseButton);
        workspace.Window.MouseUp(end + new Vector(1, 0), MouseButton.Left);
        workspace.Update();
        Assert.Equal(new[] { workspace.Second, workspace.First }, workspace.Pane.Children.Cast<LayoutContent>());
        Assert.Equal(new[] { workspace.Second, workspace.First }, row.GetVisualDescendants().OfType<Control>()
            .Where(control => tool ? control is LayoutAnchorableTabItem : control is LayoutDocumentTabItem).Select(Model));
        Assert.Same(item, Item(NativeTab(row, workspace.First, tool)));
    }

    [AvaloniaTheory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void Pane_And_Group_Reposition_Policies_Apply_To_Custom_And_Native_Tabs(bool tool, bool native, bool group)
    {
        using Workspace workspace = new(tool);
        Action<bool> allow;
        if (group)
        {
            workspace.Manager.Layout.RootPanel!.Children.Clear();
            if (tool)
            {
                LayoutAnchorablePaneGroup parent = new((LayoutAnchorablePane)workspace.Pane);
                workspace.Manager.Layout.RootPanel.Children.Add(parent);
                allow = value => parent.CanRepositionItems = value;
            }
            else
            {
                LayoutDocumentPaneGroup parent = new((LayoutDocumentPane)workspace.Pane);
                workspace.Manager.Layout.RootPanel.Children.Add(parent);
                allow = value => parent.CanRepositionItems = value;
            }
            workspace.Update();
        }
        else if (tool) allow = value => ((LayoutAnchorablePane)workspace.Pane).CanRepositionItems = value;
        else allow = value => ((LayoutDocumentPane)workspace.Pane).CanRepositionItems = value;
        allow(false);
        DragAcross(workspace, tool, native);
        Assert.Equal(new[] { workspace.First, workspace.Second }, workspace.Pane.Children.Cast<LayoutContent>());
        allow(true);
        DragAcross(workspace, tool, native);
        Assert.Equal(new[] { workspace.Second, workspace.First }, workspace.Pane.Children.Cast<LayoutContent>());
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Document_CanMove_Prevents_Custom_And_Native_Tab_Reordering(bool native)
    {
        using Workspace workspace = new(false);
        ((LayoutDocument)workspace.First).CanMove = false;
        DragAcross(workspace, false, native);
        Assert.Equal(new[] { workspace.First, workspace.Second }, workspace.Pane.Children.Cast<LayoutContent>());
        ((LayoutDocument)workspace.First).CanMove = true;
        DragAcross(workspace, false, native);
        Assert.Equal(new[] { workspace.Second, workspace.First }, workspace.Pane.Children.Cast<LayoutContent>());
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void A_Changed_Reposition_Policy_Prevents_A_Pending_Native_Reorder(bool tool, bool beforeRelease)
    {
        using Workspace workspace = new(tool);
        Control first = NativeTab(workspace.Manager, workspace.First, tool);
        Control second = NativeTab(workspace.Manager, workspace.Second, tool);
        Point start = Center(first);
        Point end = Center(second);
        workspace.Window.MouseMove(start);
        workspace.Window.MouseDown(start, MouseButton.Left);
        workspace.Window.MouseMove(start + new Vector(12, 0), RawInputModifiers.LeftMouseButton);
        if (beforeRelease) workspace.Window.MouseMove(end, RawInputModifiers.LeftMouseButton);
        if (tool) ((LayoutAnchorablePane)workspace.Pane).CanRepositionItems = false;
        else ((LayoutDocumentPane)workspace.Pane).CanRepositionItems = false;
        if (!beforeRelease) workspace.Window.MouseMove(end, RawInputModifiers.LeftMouseButton);
        workspace.Window.MouseUp(end, MouseButton.Left);
        Assert.Equal(new[] { workspace.First, workspace.Second }, workspace.Pane.Children.Cast<LayoutContent>());
    }

    [AvaloniaTheory]
    [InlineData(false, "model")]
    [InlineData(false, "detach")]
    [InlineData(false, "disabled")]
    [InlineData(false, "release")]
    [InlineData(true, "model")]
    [InlineData(true, "detach")]
    [InlineData(true, "disabled")]
    [InlineData(true, "release")]
    public void Interrupted_Custom_Drags_Do_Not_Float_Stale_Content(bool tool, string interruption)
    {
        using Workspace workspace = new(tool);
        Point start = Center(workspace.Tab);
        workspace.Window.MouseMove(start);
        workspace.Window.MouseDown(start, MouseButton.Left);
        workspace.Window.MouseMove(start + new Vector(12, 0), RawInputModifiers.LeftMouseButton);
        switch (interruption)
        {
            case "model": SetModel(workspace.Tab, workspace.Second); break;
            case "detach": workspace.Tabs.Children.Remove(workspace.Tab); break;
            case "disabled": workspace.First.IsEnabled = false; break;
            case "release": workspace.Window.MouseUp(start + new Vector(12, 0), MouseButton.Left); break;
        }
        workspace.Window.MouseMove(start + new Vector(0, 90), RawInputModifiers.LeftMouseButton);
        workspace.Window.MouseUp(start + new Vector(0, 90), MouseButton.Left);
        Assert.False(workspace.First.IsFloating);
        Assert.False(workspace.Second.IsFloating);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void CanFloat_False_Keeps_A_Custom_Tab_Docked(bool tool)
    {
        using Workspace workspace = new(tool);
        workspace.First.CanFloat = false;
        Point start = Center(workspace.Tab);
        workspace.Window.MouseMove(start);
        workspace.Window.MouseDown(start, MouseButton.Left);
        workspace.Window.MouseMove(start + new Vector(12, 0), RawInputModifiers.LeftMouseButton);
        workspace.Window.MouseMove(start + new Vector(0, 90), RawInputModifiers.LeftMouseButton);
        workspace.Window.MouseUp(start + new Vector(0, 90), MouseButton.Left);
        Assert.False(workspace.First.IsFloating);
        Assert.Empty(workspace.Manager.FloatingWindows);
    }

    private static void DragAcross(Workspace workspace, bool tool, bool native)
    {
        // Separate repeat attempts from Avalonia's double-click gesture, which
        // intentionally invokes the native tab's float command.
        Point background = new(799, 449);
        workspace.Window.MouseMove(background);
        workspace.Window.MouseDown(background, MouseButton.Left);
        workspace.Window.MouseUp(background, MouseButton.Left);
        Control first = native ? NativeTab(workspace.Manager, workspace.First, tool) : workspace.Tab;
        Control second = native ? NativeTab(workspace.Manager, workspace.Second, tool) : workspace.Other;
        Point start = Center(first);
        Point end = Center(second);
        workspace.Window.MouseMove(start);
        workspace.Window.MouseDown(start, MouseButton.Left);
        workspace.Window.MouseMove(start + new Vector(12, 0), RawInputModifiers.LeftMouseButton);
        workspace.Window.MouseMove(end, RawInputModifiers.LeftMouseButton);
        workspace.Window.MouseUp(end, MouseButton.Left);
        workspace.Update();
    }
}
