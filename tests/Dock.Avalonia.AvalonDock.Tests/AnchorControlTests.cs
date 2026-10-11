using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class AnchorControlTests
{
    [AvaloniaTheory]
    [InlineData(AnchorSide.Left)]
    [InlineData(AnchorSide.Top)]
    [InlineData(AnchorSide.Right)]
    [InlineData(AnchorSide.Bottom)]
    public void Public_Side_Group_And_Anchor_Controls_Render_The_Existing_Native_Models(AnchorSide side)
    {
        using Workspace workspace = new();
        LayoutAnchorSideControl control = workspace.Side(side);
        LayoutAnchorSide model = Assert.IsType<LayoutAnchorSide>(control.Model);
        Assert.Equal(side, model.Side);
        Assert.Equal(side == AnchorSide.Left, control.IsLeftSide);
        Assert.Equal(side == AnchorSide.Top, control.IsTopSide);
        Assert.Equal(side == AnchorSide.Right, control.IsRightSide);
        Assert.Equal(side == AnchorSide.Bottom, control.IsBottomSide);
        Assert.Same(model, control.Tag);
        LayoutAnchorGroupControl group = Assert.Single(control.Children);
        Assert.Same(model.Children[0], group.Model);
        Assert.Same(group.Model, group.Tag);
        Assert.Equal(model.Children[0].Children.Count, group.Children.Count);
        foreach (LayoutAnchorControl anchor in group.Children)
        {
            LayoutAnchorable tool = Assert.IsType<LayoutAnchorable>(anchor.Model);
            Assert.Same(tool, anchor.Tag);
            Assert.Same(tool.DockModel, anchor.DataContext);
            Assert.Contains(anchor, workspace.Manager.GetVisualDescendants());
            Assert.Contains(group, anchor.GetVisualAncestors());
            Assert.Contains(control, anchor.GetVisualAncestors());
            Assert.Equal(side, anchor.Side);
            Assert.True(anchor.Bounds.Width > 0 && anchor.Bounds.Height > 0);
            Assert.IsAssignableFrom<ToolPinItemControl>(anchor);
            Assert.Contains(tool.DockModel, workspace.Pinned(side));
        }
    }

    [AvaloniaFact]
    public void Group_And_Anchor_Collections_Follow_Add_Move_Replace_Remove_And_Reset()
    {
        using Workspace workspace = new();
        LayoutAnchorSideControl side = workspace.Side(AnchorSide.Left);
        LayoutAnchorSide model = (LayoutAnchorSide)side.Model;
        LayoutAnchorGroup first = model.Children[0];
        LayoutAnchorGroupControl firstView = side.Children[0];
        LayoutAnchorControl a = firstView.Children[0], b = firstView.Children[1];
        first.Children.Move(0, 1);
        Assert.Equal(new[] { b, a }, firstView.Children);
        Assert.Same(b.Model, first.Children[0]);
        LayoutAnchorGroup second = new() { Children = { new LayoutAnchorable { Title = "C" } } };
        model.Children.Add(second);
        LayoutAnchorGroupControl secondView = side.Children[1];
        model.Children.Move(1, 0);
        Assert.Equal(new[] { secondView, firstView }, side.Children);
        LayoutAnchorable replacement = new() { Title = "Replacement" };
        first.Children[0] = replacement;
        Assert.Same(replacement, firstView.Children[0].Model);
        Assert.Same(a, firstView.Children[1]);
        first.Children.RemoveAt(1);
        Assert.Single(firstView.Children);
        model.Children.Remove(second);
        Assert.Same(firstView, Assert.Single(side.Children));
        first.Children.Clear();
        Assert.Empty(firstView.Children);
        model.Children.Clear();
        Assert.Empty(side.Children);
        Assert.Empty(workspace.Pinned(AnchorSide.Left));
    }

    [AvaloniaFact]
    public void Native_Pinned_Reordering_Updates_The_Public_Controls_Without_Replacing_Them()
    {
        using Workspace workspace = new();
        LayoutAnchorGroupControl group = workspace.Side(AnchorSide.Left).Children[0];
        LayoutAnchorControl first = group.Children[0], second = group.Children[1];
        Assert.IsType<ToolPinnedControl>(first.Parent);
        Point start = workspace.Center(first), target = workspace.Center(second);
        workspace.Window.MouseMove(start);
        workspace.Window.MouseDown(start, MouseButton.Left);
        Assert.Equal(2, workspace.Pinned(AnchorSide.Left).Count);
        workspace.Window.MouseMove(target, RawInputModifiers.LeftMouseButton);
        Assert.True(workspace.Pinned(AnchorSide.Left).Count == 2,
            $"Move changed the pinned count to {workspace.Pinned(AnchorSide.Left).Count}; start={start}, target={target}, group={first.Parent?.GetType().Name}, floating={workspace.Manager.Layout.FloatingWindows.Count}.");
        workspace.Window.MouseUp(target, MouseButton.Left);
        Assert.True(workspace.Pinned(AnchorSide.Left).Count == 2,
            $"Release changed the pinned count to {workspace.Pinned(AnchorSide.Left).Count}; floating={workspace.Manager.Layout.FloatingWindows.Count}, firstParent={first.Model.Parent?.GetType().Name}, secondParent={second.Model.Parent?.GetType().Name}.");
        workspace.Update();
        Assert.Same(((LayoutAnchorable)second.Model).DockModel, workspace.Pinned(AnchorSide.Left)[0]);
        Assert.Equal(2, group.Children.Count);
        Assert.Same(second.Model, group.Children[0].Model);
        Assert.Same(first.Model, group.Children[1].Model);
        Assert.Same(second, group.Children[0]);
        Assert.Same(first, group.Children[1]);
        Assert.Empty(workspace.Manager.Layout.FloatingWindows);
    }

    [AvaloniaFact]
    public void Native_Pin_Unpin_And_Side_Collection_Replacement_Refresh_The_Public_Tree()
    {
        using Workspace workspace = new();
        LayoutAnchorable model = (LayoutAnchorable)workspace.Side(AnchorSide.Left).Children[0].Children[0].Model;
        model.ToggleAutoHide();
        workspace.Update();
        Assert.False(model.IsAutoHidden);
        Assert.DoesNotContain(workspace.Manager.GetVisualDescendants().OfType<LayoutAnchorControl>(), view => ReferenceEquals(view.Model, model));
        model.ToggleAutoHide();
        workspace.Update();
        LayoutAnchorControl anchor = workspace.Manager.GetVisualDescendants().OfType<LayoutAnchorControl>().Single(view => ReferenceEquals(view.Model, model));
        Assert.True(model.IsAutoHidden);
        Assert.Equal(model.FindParent<LayoutAnchorSide>()!.Side, anchor.Side);
        LayoutAnchorable replacement = new() { Title = "Native", Content = "Native content" };
        workspace.Manager.DockLayout.TopPinnedDockables = new global::Avalonia.Collections.AvaloniaList<global::Dock.Model.Core.IDockable> { replacement.DockModel };
        workspace.Manager.DockFactory.InitDockable(replacement.DockModel, workspace.Manager.DockLayout);
        workspace.Update();
        LayoutAnchorControl native = Assert.Single(Assert.Single(workspace.Side(AnchorSide.Top).Children).Children);
        Assert.Same(replacement.DockModel, ((LayoutAnchorable)native.Model).DockModel);
        Assert.Equal(AnchorSide.Top, native.Side);
    }

    [AvaloniaFact]
    public void Disabled_Anchors_And_Interactive_Template_Children_Do_Not_Open_Previews()
    {
        using Workspace workspace = new();
        LayoutAnchorControl anchor = workspace.Side(AnchorSide.Left).Children[0].Children[0];
        LayoutAnchorable model = (LayoutAnchorable)anchor.Model;
        model.IsEnabled = false;
        workspace.Update();
        Assert.False(anchor.IsEffectivelyEnabled);
        workspace.Click(anchor);
        Assert.Null(((IRootDock)workspace.Manager.DockLayout).PinnedDock);
        model.IsEnabled = true;
        workspace.Manager.AnchorTemplate = new FuncControlTemplate<LayoutAnchorControl>((_, _) => new Button { Content = "Action" });
        workspace.Update();
        workspace.Click(anchor.GetVisualDescendants().OfType<Button>().Single());
        Assert.Null(((IRootDock)workspace.Manager.DockLayout).PinnedDock);
        workspace.Manager.AnchorTemplate = null;
        workspace.Update();
        Assert.Empty(anchor.GetVisualDescendants());
    }

    [AvaloniaFact]
    public async Task Detaching_Cancels_A_Pending_Hover_And_Reattachment_Reuses_The_Controls()
    {
        using Workspace workspace = new();
        LayoutAnchorSideControl side = workspace.Side(AnchorSide.Left);
        LayoutAnchorControl anchor = side.Children[0].Children[0];
        workspace.Window.MouseMove(workspace.Center(anchor));
        workspace.Window.Content = null;
        workspace.Update();
        await Task.Delay(450); workspace.Update();
        Assert.Null(((IRootDock)workspace.Manager.DockLayout).PinnedDock);
        workspace.Window.Content = workspace.Manager;
        workspace.Update();
        Assert.Same(side, workspace.Side(AnchorSide.Left));
        Assert.Same(anchor, side.Children[0].Children[0]);
        workspace.Click(anchor);
        Assert.Same(((LayoutAnchorable)anchor.Model).DockModel, ((IRootDock)workspace.Manager.DockLayout).PinnedDock?.ActiveDockable);
    }

    [AvaloniaFact]
    public void Side_Setters_Invoke_Derived_Manager_Hooks_And_Preserve_Explicit_Nulls()
    {
        ManagerProbe manager = new();
        Window window = new() { Content = manager, Width = 500, Height = 400 };
        try
        {
            window.Show(); Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            Assert.Equal(new[] { 1, 1, 1, 1 }, manager.Changes);
            LayoutAnchorSideControl[] controls = { manager.LeftSidePanel!, manager.TopSidePanel!, manager.RightSidePanel!, manager.BottomSidePanel! };
            manager.LeftSidePanel = manager.TopSidePanel = manager.RightSidePanel = manager.BottomSidePanel = null;
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            Assert.Equal(new[] { 2, 2, 2, 2 }, manager.Changes);
            Assert.Null(manager.LeftSidePanel); Assert.Null(manager.TopSidePanel); Assert.Null(manager.RightSidePanel); Assert.Null(manager.BottomSidePanel);
            manager.LeftSidePanel = controls[0]; manager.TopSidePanel = controls[1]; manager.RightSidePanel = controls[2]; manager.BottomSidePanel = controls[3];
            Assert.Equal(new[] { 3, 3, 3, 3 }, manager.Changes);
            manager.Layout = new LayoutRoot();
            Assert.Equal(new[] { 4, 4, 4, 4 }, manager.Changes);
            Assert.Same(manager.Layout.LeftSide, manager.LeftSidePanel!.Model);
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [AvaloniaFact]
    public void Manager_Templates_Update_Existing_Controls_And_Clear_Back_To_Their_Styles()
    {
        using Workspace workspace = new();
        LayoutAnchorSideControl side = workspace.Side(AnchorSide.Left);
        LayoutAnchorGroupControl group = side.Children[0];
        LayoutAnchorControl anchor = group.Children[0];
        workspace.Manager.AnchorTemplate = (IControlTemplate)workspace.Window.Resources["CustomAnchor"]!;
        workspace.Manager.AnchorGroupTemplate = (IControlTemplate)workspace.Window.Resources["CustomGroup"]!;
        workspace.Manager.AnchorSideTemplate = (IControlTemplate)workspace.Window.Resources["CustomSide"]!;
        workspace.Update();
        Assert.Same(anchor, group.Children[0]);
        Assert.Contains(anchor.GetVisualDescendants().OfType<TextBlock>(), text => text.Name == "CustomAnchorHeader" && text.Text == "Left A");
        Assert.Contains(group.GetVisualDescendants().OfType<ItemsControl>(), items => items.Name == "CustomGroupItems");
        Assert.Contains(side.GetVisualDescendants().OfType<ItemsControl>(), items => items.Name == "CustomSideItems");
        ((LayoutAnchorable)anchor.Model).Title = "Changed";
        workspace.Update();
        Assert.Contains(anchor.GetVisualDescendants().OfType<TextBlock>(), text => text.Name == "CustomAnchorHeader" && text.Text == "Changed");
        workspace.Manager.ClearValue(DockingManager.AnchorSideTemplateProperty);
        workspace.Manager.ClearValue(DockingManager.AnchorGroupTemplateProperty);
        workspace.Manager.ClearValue(DockingManager.AnchorTemplateProperty);
        workspace.Update();
        Assert.Same(anchor, group.Children[0]);
        Assert.DoesNotContain(anchor.GetVisualDescendants().OfType<TextBlock>(), text => text.Name == "CustomAnchorHeader");
        Assert.True(anchor.Bounds.Height > 0);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Null_Side_And_Group_Templates_Release_Their_Items_And_Can_Be_Restored(bool sideTemplate)
    {
        using Workspace workspace = new();
        LayoutAnchorSideControl side = workspace.Side(AnchorSide.Left);
        LayoutAnchorControl anchor = side.Children[0].Children[0];
        var property = sideTemplate ? DockingManager.AnchorSideTemplateProperty : DockingManager.AnchorGroupTemplateProperty;
        workspace.Manager.SetValue(property, null);
        workspace.Update();
        Assert.DoesNotContain(anchor, workspace.Manager.GetVisualDescendants());
        workspace.Manager.ClearValue(property);
        workspace.Update();
        Assert.Contains(anchor, workspace.Manager.GetVisualDescendants());
        workspace.Click(anchor);
        Assert.Same(((LayoutAnchorable)anchor.Model).DockModel, ((IRootDock)workspace.Manager.DockLayout).PinnedDock?.ActiveDockable);
    }

    [AvaloniaFact]
    public void A_Retained_Side_Uses_Its_New_Managers_Templates_And_Preview_Factory()
    {
        using Workspace workspace = new();
        LayoutRoot layout = workspace.Manager.Layout;
        LayoutAnchorSideControl side = workspace.Side(AnchorSide.Left);
        LayoutAnchorControl anchor = side.Children[0].Children[0];
        workspace.Manager.Layout = new LayoutRoot();
        DockingManager destination = new()
        {
            Layout = layout,
            AnchorTemplate = (IControlTemplate)workspace.Window.Resources["CustomAnchor"]!
        };
        workspace.Window.Content = destination;
        workspace.Update();
        destination.LeftSidePanel = side;
        workspace.Update();
        Assert.Contains(anchor.GetVisualDescendants().OfType<TextBlock>(), text => text.Name == "CustomAnchorHeader");
        workspace.Click(anchor);
        Assert.Same(((LayoutAnchorable)anchor.Model).DockModel, ((IRootDock)destination.DockLayout).PinnedDock?.ActiveDockable);
        Assert.Null(((IRootDock)workspace.Manager.DockLayout).PinnedDock);
    }

    [AvaloniaTheory]
    [InlineData(AnchorSide.Left)]
    [InlineData(AnchorSide.Top)]
    [InlineData(AnchorSide.Right)]
    [InlineData(AnchorSide.Bottom)]
    public void Pointer_Press_Opens_Native_Preview_And_Activates_The_Anchor(AnchorSide side)
    {
        using Workspace workspace = new();
        LayoutAnchorControl anchor = workspace.Side(side).Children[0].Children[0];
        LayoutAnchorable model = (LayoutAnchorable)anchor.Model;
        workspace.Click(anchor);
        Assert.Same(model.DockModel, ((IRootDock)workspace.Manager.DockLayout).PinnedDock?.ActiveDockable);
        Assert.True(model.IsActive);
        Assert.Same(model, workspace.Manager.Layout.ActiveContent);
        Assert.Contains(workspace.Manager.GetVisualDescendants().OfType<LayoutAnchorableControl>(), control => ReferenceEquals(control.Model, model));
        workspace.Click(anchor);
        Assert.Same(model.DockModel, ((IRootDock)workspace.Manager.DockLayout).PinnedDock?.ActiveDockable);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Programmatic_Selection_And_Activation_Open_The_Native_Preview(bool activate)
    {
        using Workspace workspace = new();
        LayoutDocument document = workspace.Manager.Layout.Descendents().OfType<LayoutDocument>().Single();
        document.IsActive = true;
        LayoutAnchorable model = (LayoutAnchorable)workspace.Side(AnchorSide.Left).Children[0].Children[0].Model;
        Assert.False(model.IsSelected, "The pinned tool was already selected.");
        Assert.False(model.IsActive, "The pinned tool was already active.");
        if (activate) model.IsActive = true; else model.IsSelected = true;
        Assert.Same(model.DockModel, ((IRootDock)workspace.Manager.DockLayout).PinnedDock?.ActiveDockable);
        workspace.Update();
        Assert.Same(model.DockModel, ((IRootDock)workspace.Manager.DockLayout).PinnedDock?.ActiveDockable);
        Assert.Same(activate ? (LayoutContent)model : document, workspace.Manager.Layout.ActiveContent);
        if (!activate) Assert.False(model.IsSelected);
    }

    [AvaloniaFact]
    public async Task Hover_Opens_A_Passive_Preview_And_Leaving_Cancels_The_Delay()
    {
        using Workspace workspace = new();
        LayoutDocument document = workspace.Manager.Layout.Descendents().OfType<LayoutDocument>().Single();
        document.IsActive = true;
        LayoutAnchorControl anchor = workspace.Side(AnchorSide.Left).Children[0].Children[0];
        Point point = workspace.Center(anchor);
        workspace.Window.MouseMove(point);
        Assert.Null(((IRootDock)workspace.Manager.DockLayout).PinnedDock);
        workspace.Window.MouseMove(new Point(500, 400));
        await Task.Delay(450); workspace.Update();
        Assert.Null(((IRootDock)workspace.Manager.DockLayout).PinnedDock);
        workspace.Window.MouseMove(point);
        await Task.Delay(450); workspace.Update();
        Assert.Same(((LayoutAnchorable)anchor.Model).DockModel, ((IRootDock)workspace.Manager.DockLayout).PinnedDock?.ActiveDockable);
        Assert.Same(document, workspace.Manager.Layout.ActiveContent);
    }

    [AvaloniaFact]
    public void Side_Clear_Restore_Replacement_And_XML_Use_The_Current_Models()
    {
        using Workspace workspace = new();
        LayoutAnchorSideControl original = workspace.Side(AnchorSide.Left);
        workspace.Manager.LeftSidePanel = null;
        workspace.Update();
        Assert.DoesNotContain(original, workspace.Manager.GetVisualDescendants());
        Assert.Null(workspace.Manager.LeftSidePanel);
        workspace.Manager.LeftSidePanel = original;
        workspace.Update();
        Assert.Contains(original, workspace.Manager.GetVisualDescendants());
        LayoutAnchorSide replacement = new() { Children = { new LayoutAnchorGroup { Children = { new LayoutAnchorable { Title = "New", ContentId = "new", Content = "New content" } } } } };
        workspace.Manager.Layout.LeftSide = replacement;
        workspace.Update();
        Assert.NotSame(original, workspace.Manager.LeftSidePanel);
        Assert.Same(replacement, workspace.Manager.LeftSidePanel!.Model);
        using StringWriter xml = new();
        XmlLayoutSerializer serializer = new(workspace.Manager);
        serializer.Serialize(xml); serializer.Deserialize(new StringReader(xml.ToString()));
        workspace.Update();
        Assert.Same(workspace.Manager.Layout.LeftSide, workspace.Manager.LeftSidePanel!.Model);
        Assert.NotEmpty(workspace.Manager.Layout.LeftSide.Children);
        Assert.Contains(workspace.Manager.LeftSidePanel, workspace.Manager.GetVisualDescendants());
        workspace.Window.Content = null; workspace.Update();
        workspace.Window.Content = workspace.Manager; workspace.Update();
        Assert.Contains(workspace.Manager.LeftSidePanel, workspace.Manager.GetVisualDescendants());
    }

    [AvaloniaFact]
    public void Retained_Models_Do_Not_Retain_Replaced_Anchor_Views()
    {
        var result = ReplaceLayout();
        for (int i = 0; i < 3; i++)
        {
            Dispatcher.UIThread.RunJobs(); AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            GC.Collect(); GC.WaitForPendingFinalizers();
        }
        Assert.False(result.Side.TryGetTarget(out _));
        Assert.False(result.Anchor.TryGetTarget(out _));
        GC.KeepAlive(result.Model); GC.KeepAlive(result.Manager);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (DockingManager Manager, LayoutRoot Model, WeakReference<Control> Side, WeakReference<Control> Anchor) ReplaceLayout()
    {
        using Workspace workspace = new();
        var result = (workspace.Manager, workspace.Manager.Layout, new WeakReference<Control>(workspace.Manager.LeftSidePanel!),
            new WeakReference<Control>(workspace.Manager.LeftSidePanel!.Children[0].Children[0]));
        workspace.Manager.Layout = new LayoutRoot();
        workspace.Update(); workspace.Window.CaptureRenderedFrame()?.Dispose();
        return result;
    }

    private sealed class Workspace : IDisposable
    {
        public AnchorControlFixture Window { get; } = new();
        public DockingManager Manager { get; }
        public Workspace() { Manager = Window.FindControl<DockingManager>("Manager")!; Window.Show(); Update(); }
        public LayoutAnchorSideControl Side(AnchorSide side) => (side switch
        {
            AnchorSide.Left => Manager.LeftSidePanel, AnchorSide.Top => Manager.TopSidePanel,
            AnchorSide.Right => Manager.RightSidePanel, _ => Manager.BottomSidePanel
        })!;
        public System.Collections.Generic.IList<global::Dock.Model.Core.IDockable> Pinned(AnchorSide side) => (side switch
        {
            AnchorSide.Left => Manager.DockLayout.LeftPinnedDockables, AnchorSide.Top => Manager.DockLayout.TopPinnedDockables,
            AnchorSide.Right => Manager.DockLayout.RightPinnedDockables, _ => Manager.DockLayout.BottomPinnedDockables
        })!;
        public Point Center(Control control) => control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), Window)!.Value;
        public void Click(Control control)
        {
            Point point = Center(control);
            Window.MouseMove(point); Window.MouseDown(point, MouseButton.Left); Window.MouseUp(point, MouseButton.Left); Update();
        }
        public void Update() { Dispatcher.UIThread.RunJobs(); Window.UpdateLayout(); }
        public void Dispose() { Manager.Layout.FloatingWindows.Clear(); Window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    private sealed class ManagerProbe : DockingManager
    {
        public int[] Changes { get; } = new int[4];
        protected override void OnLeftSidePanelChanged(AvaloniaPropertyChangedEventArgs e) { Changes[0]++; base.OnLeftSidePanelChanged(e); }
        protected override void OnTopSidePanelChanged(AvaloniaPropertyChangedEventArgs e) { Changes[1]++; base.OnTopSidePanelChanged(e); }
        protected override void OnRightSidePanelChanged(AvaloniaPropertyChangedEventArgs e) { Changes[2]++; base.OnRightSidePanelChanged(e); }
        protected override void OnBottomSidePanelChanged(AvaloniaPropertyChangedEventArgs e) { Changes[3]++; base.OnBottomSidePanelChanged(e); }
    }
}
