using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class NativeWindowMessageTests
{
    [AvaloniaFact]
    public void System_Menu_Default_And_Styled_Values_Follow_The_Manager_Property()
    {
        DockingManager manager = new();
        Assert.True(manager.ShowSystemMenu);
        manager.SetValue(DockingManager.ShowSystemMenuProperty, false, BindingPriority.Style);
        Assert.False(manager.ShowSystemMenu);
        manager.ShowSystemMenu = true;
        Assert.True(manager.ShowSystemMenu);
        manager.ClearValue(DockingManager.ShowSystemMenuProperty);
        Assert.False(manager.ShowSystemMenu);
    }

    [AvaloniaTheory]
    [InlineData("DefaultNamespace")]
    [InlineData("HistoricalNamespace")]
    public void Both_XML_Namespaces_Compile_The_System_Menu_Property(string key) =>
        Assert.False(Assert.IsType<DockingManager>(new NativeWindowFixture().Resources[key]).ShowSystemMenu);

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Caption_Fallback_Reads_The_Current_Manager_Option(bool tool)
    {
        using Workspace workspace = new(tool);
        Assert.False(workspace.Send(0xA5));
        workspace.Manager.ShowSystemMenu = false;
        Assert.True(workspace.Send(0xA5));
        workspace.Manager.ShowSystemMenu = true;
        Assert.False(workspace.Send(0xA5));
        Assert.True(workspace.Host.IsVisible);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Custom_Caption_Menus_Precede_System_Fallback_At_Signed_Screen_Coordinates(bool tool, bool showSystemMenu)
    {
        using Workspace workspace = new(tool);
        workspace.Manager.ShowSystemMenu = showSystemMenu;
        ContextMenu menu = new() { Items = { new MenuItem { Header = "Custom action" } } };
        workspace.SetMenu(menu);
        PixelPoint position = new(-240, -90);

        Assert.True(workspace.Send(0xA5, position: position));

        Assert.True(menu.IsOpen);
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.Content), menu.DataContext);
        Assert.Same(workspace.Host, menu.PlacementTarget);
        Assert.Equal(PlacementMode.AnchorAndGravity, menu.Placement);
        Assert.Equal(workspace.Host.PointToClient(position), menu.PlacementRect!.Value.Position);
        menu.Close();
        Assert.Null(menu.DataContext);
        Assert.Null(menu.PlacementTarget);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Canceling_A_Custom_Menu_Does_Not_Open_The_System_Fallback(bool tool)
    {
        using Workspace workspace = new(tool);
        ContextMenu menu = new() { Items = { new MenuItem { Header = "Canceled" } } };
        menu.Opening += (_, e) => e.Cancel = true;
        workspace.SetMenu(menu);

        Assert.True(workspace.Send(0xA5));

        Assert.False(menu.IsOpen);
        Assert.Null(menu.DataContext);
        Assert.Null(menu.PlacementTarget);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Caption_Click_Activates_Content_And_Leaves_Movement_To_The_Native_Host(bool tool)
    {
        using Workspace workspace = new(tool);
        workspace.Other.IsActive = true;

        Assert.False(workspace.Send(0xA1));

        Assert.Same(workspace.Content, workspace.Manager.Layout.ActiveContent);
        Assert.True(workspace.Content.IsSelected);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Other_Hit_Areas_And_Previously_Handled_Messages_Do_Not_Run_Caption_Policy(bool tool)
    {
        using Workspace workspace = new(tool);
        workspace.Manager.ShowSystemMenu = false;
        workspace.Other.IsActive = true;

        Assert.False(workspace.Send(0xA5, hitTest: 1));
        Assert.False(workspace.Send(0xF));
        Assert.True(workspace.Send(0xA1, handled: true));
        Assert.Same(workspace.Other, workspace.Manager.Layout.ActiveContent);
    }

    [AvaloniaFact]
    public void A_Multiple_Pane_Tool_Window_Uses_System_Fallback_And_Activates_The_First_Selected_Tool()
    {
        using Workspace workspace = new(true);
        LayoutAnchorableFloatingWindow model = Assert.IsType<LayoutAnchorableFloatingWindow>(workspace.Host.Model);
        model.RootPanel!.Children.Add(new LayoutAnchorablePane(new LayoutAnchorable { Title = "Second pane" }));
        workspace.Update();
        Assert.Null(Assert.IsAssignableFrom<LayoutAnchorableFloatingWindowControl>(workspace.Host).SingleContentLayoutItem);
        ContextMenu menu = new() { Items = { new MenuItem { Header = "Single pane only" } } };
        workspace.SetMenu(menu);
        Assert.False(workspace.Send(0xA5));
        Assert.False(menu.IsOpen);
        workspace.Manager.ShowSystemMenu = false;
        Assert.True(workspace.Send(0xA5));
        workspace.Other.IsActive = true;
        Assert.False(workspace.Send(0xA1));
        Assert.Same(workspace.Content, workspace.Manager.Layout.ActiveContent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Reused_Menu_Changes_Its_Target_And_Clears_Invocation_State_When_The_Host_Closes(bool tool)
    {
        using Workspace workspace = new(tool);
        ContextMenu menu = new() { Items = { new MenuItem { Header = "Shared" } } };
        workspace.SetMenu(menu);
        Assert.True(workspace.Send(0xA5, position: new PixelPoint(-10, 20)));
        Assert.True(workspace.Send(0xA5, position: new PixelPoint(80, 120)));
        Assert.Equal(workspace.Host.PointToClient(new PixelPoint(80, 120)), menu.PlacementRect!.Value.Position);

        workspace.Host.Close();

        Assert.False(menu.IsOpen);
        Assert.Null(menu.DataContext);
        Assert.Null(menu.PlacementTarget);
    }

    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Overlay_Closing_Hooks_Share_Arguments_And_Preserve_Projection_When_Canceled(int veto)
    {
        using Workspace workspace = new(false);
        Control surface = workspace.Manager.GetVisualDescendants().OfType<LayoutDocumentPaneControl>().First();
        DockTarget target = new();
        NativeOverlay overlay = new(workspace.Manager, surface, target) { Content = target, Width = 200, Height = 200, CancelAtHook = veto == 0 };
        bool cancel = true;
        WindowClosingEventArgs? nativeArgs = null;
        overlay.Closing += (_, e) => { nativeArgs = e; overlay.Events.Add("event"); if (veto == 1 && cancel) e.Cancel = true; };
        overlay.CancelAfterBase = veto == 2;
        try
        {
            overlay.Show(workspace.Owner);
            overlay.Close();

            Assert.Equal(new[] { "native-before", "hook-before", "event", "hook-after", "native-after" }, overlay.Events);
            Assert.Same(nativeArgs, overlay.Arguments);
            Assert.True(nativeArgs!.Cancel);
            Assert.True(overlay.IsVisible);
            Assert.NotNull(overlay.DropArea);
            Assert.Same(target, overlay.Content);

            cancel = false; overlay.CancelAtHook = false; overlay.CancelAfterBase = false;
            overlay.Close();
            Assert.False(overlay.IsVisible);
            Assert.Null(overlay.DropArea);
            Assert.Null(overlay.Content);
        }
        finally { cancel = false; overlay.CancelAtHook = false; overlay.CancelAfterBase = false; overlay.Close(); }
    }

    private interface IMessageProbe
    {
        IntPtr Send(int message, IntPtr hitTest, IntPtr position, ref bool handled);
    }

    private sealed class DocumentProbe(LayoutDocumentFloatingWindow model) : LayoutDocumentFloatingWindowControl(model), IMessageProbe
    {
        public IntPtr Send(int message, IntPtr hitTest, IntPtr position, ref bool handled) => FilterMessage(IntPtr.Zero, message, hitTest, position, ref handled);
    }

    private sealed class ToolProbe(LayoutAnchorableFloatingWindow model) : LayoutAnchorableFloatingWindowControl(model), IMessageProbe
    {
        public IntPtr Send(int message, IntPtr hitTest, IntPtr position, ref bool handled) => FilterMessage(IntPtr.Zero, message, hitTest, position, ref handled);
    }

    private class BclOverlay(DockingManager manager, Control surface, Control adorner) : OverlayWindow(manager, surface, adorner)
    {
        public List<string> Events { get; } = new();
        public CancelEventArgs? Arguments { get; private set; }
        public bool CancelAtHook { get; set; }
        public bool CancelAfterBase { get; set; }
        protected override void OnClosing(CancelEventArgs e)
        {
            Arguments = e; Events.Add("hook-before"); e.Cancel |= CancelAtHook;
            base.OnClosing(e); e.Cancel |= CancelAfterBase; Events.Add("hook-after");
        }
    }

    private sealed class NativeOverlay(DockingManager manager, Control surface, Control adorner) : BclOverlay(manager, surface, adorner)
    {
        protected override void OnClosing(WindowClosingEventArgs e)
        {
            Events.Add("native-before"); base.OnClosing(e); Events.Add("native-after");
        }
    }

    private sealed class Workspace : IDisposable
    {
        public DockingManager Manager { get; } = new() { DocumentContextMenu = null, AnchorableContextMenu = null };
        public LayoutDocument Other { get; } = new() { Title = "Other", Content = "Other document" };
        public LayoutContent Content { get; }
        public Window Owner { get; }
        public LayoutFloatingWindowControl Host { get; }

        public Workspace(bool tool)
        {
            Content = tool ? new LayoutAnchorable { Title = "Tools", Content = "Properties" } : new LayoutDocument { Title = "Editor", Content = "Editor" };
            LayoutPanel panel = new(new LayoutDocumentPane(Other));
            if (Content is LayoutAnchorable anchorable) panel.Children.Add(new LayoutAnchorablePane(anchorable));
            else panel.Children.Add(new LayoutDocumentPane((LayoutDocument)Content));
            Manager.Layout = new() { RootPanel = panel };
            Owner = new() { Width = 800, Height = 600, Content = Manager };
            Owner.Show(); Update();
            Manager.DockFactory.HostWindowLocator![nameof(IDockWindow)] = () =>
            {
                LayoutFloatingWindow model = Manager.Layout.FloatingWindows.Single(window => window.DockWindow.Host is null);
                return model is LayoutDocumentFloatingWindow document ? new DocumentProbe(document) : new ToolProbe((LayoutAnchorableFloatingWindow)model);
            };
            Content.Float(); Update();
            Host = Assert.Single(Manager.FloatingWindows);
        }

        public void SetMenu(ContextMenu menu)
        {
            if (Content is LayoutAnchorable) Manager.AnchorableContextMenu = menu;
            else Manager.DocumentContextMenu = menu;
        }

        public bool Send(int message, int hitTest = 2, bool handled = false, PixelPoint position = default)
        {
            int packed = unchecked((ushort)position.X | ((ushort)position.Y << 16));
            Assert.Equal(IntPtr.Zero, ((IMessageProbe)Host).Send(message, (IntPtr)hitTest, (IntPtr)packed, ref handled));
            return handled;
        }

        public void Update() { Owner.UpdateLayout(); Dispatcher.UIThread.RunJobs(); }
        public void Dispose() { Manager.Layout.FloatingWindows.Clear(); Owner.Close(); Dispatcher.UIThread.RunJobs(); }
    }
}
