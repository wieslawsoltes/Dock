using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public partial class TabControlTests
{
    public static IEnumerable<object[]> FullDragPreviewCases()
    {
        foreach (bool tool in new[] { false, true })
            foreach (bool managed in new[] { false, true })
                foreach (bool enabled in new[] { false, true })
                    yield return new object[] { tool, managed, enabled };
    }

    [AvaloniaTheory]
    [MemberData(nameof(FullDragPreviewCases))]
    public void Native_Tab_Drag_Uses_The_Managers_Full_Content_Preview_And_Cancel_Restores_Selection(
        bool tool, bool managed, bool enabled)
    {
        bool oldManaged = DockSettings.UseManagedWindows;
        bool oldPreview = DockSettings.ShowDockablePreviewOnDrag;
        DragPreviewWindow? previewWindow = null;
        using IDisposable subscription = Window.WindowOpenedEvent.AddClassHandler<DragPreviewWindow>((window, _) => previewWindow = window);
        DockSettings.UseManagedWindows = managed;
        DockSettings.ShowDockablePreviewOnDrag = !enabled;
        try
        {
            using Workspace workspace = new(tool);
            workspace.Manager.ShowDockablePreviewOnDrag = enabled;
            TextBox editor = new() { Text = "Full preview editor", AcceptsReturn = true };
            workspace.First.Content = editor;
            workspace.First.IsActive = true;
            workspace.Update();
            LayoutContent[] children = workspace.Pane.Children.Cast<LayoutContent>().ToArray();
            Control tab = NativeTab(workspace.Manager, workspace.First, tool);
            Point start = Center(tab);
            workspace.Window.MouseMove(start);
            workspace.Window.MouseDown(start, MouseButton.Left);
            workspace.Window.MouseMove(start + new Vector(12, 0), RawInputModifiers.LeftMouseButton);
            workspace.Window.MouseMove(start + new Vector(0, tool ? -80 : 80), RawInputModifiers.LeftMouseButton);
            workspace.Update();
            DockControl native = Assert.Single(workspace.Manager.GetVisualDescendants().OfType<DockControl>());
            Assert.True(native.IsDraggingDock);
            DragPreviewControl preview = managed
                ? Assert.Single(workspace.Window.GetVisualDescendants().OfType<DragPreviewControl>())
                : Assert.IsType<DragPreviewControl>(Assert.IsType<DragPreviewWindow>(previewWindow).Content);
            Assert.Equal(enabled, preview.ShowContent);
            Assert.Equal(!enabled, DockSettings.ShowDockablePreviewOnDrag);
            Image? snapshot = null;
            if (enabled)
            {
                snapshot = Assert.IsAssignableFrom<Image>(preview.PreviewContent);
                Bitmap bitmap = Assert.IsAssignableFrom<Bitmap>(snapshot.Source);
                Assert.True(bitmap.PixelSize.Width > 100 && bitmap.PixelSize.Height > 100);
                Assert.True(preview.PreviewContentWidth > 100);
                Assert.True(preview.PreviewContentHeight > 100);
                Assert.True(preview.Bounds.Height > 100);
            }
            else Assert.Null(preview.PreviewContent);
            Assert.Equal(children, workspace.Pane.Children.Cast<LayoutContent>());
            Assert.Same(workspace.Pane, workspace.First.Parent);
            Assert.Empty(workspace.Manager.Layout.FloatingWindows);

            workspace.Window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
            workspace.Window.KeyRelease(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
            workspace.Window.MouseUp(start, MouseButton.Left);
            workspace.Update();
            Assert.False(native.IsDraggingDock);
            Assert.Same(workspace.First, ((ILayoutContentSelector)workspace.Pane).SelectedContent);
            Assert.Same(workspace.Window, TopLevel.GetTopLevel(editor));
            Assert.Equal(children, workspace.Pane.Children.Cast<LayoutContent>());
            if (snapshot is not null) Assert.Null(snapshot.Source);
            if (managed) Assert.Empty(workspace.Window.GetVisualDescendants().OfType<DragPreviewControl>());
            else Assert.False(previewWindow!.IsVisible);
        }
        finally
        {
            DockSettings.UseManagedWindows = oldManaged;
            DockSettings.ShowDockablePreviewOnDrag = oldPreview;
        }
    }

    [AvaloniaFact]
    public void Full_Previews_Default_To_Enabled_And_Are_Scoped_To_The_Current_Manager()
    {
        DockingManager first = new();
        DockingManager second = new() { ShowDockablePreviewOnDrag = false };
        LayoutDocument document = new();
        LayoutDocumentPane pane = new(document);
        first.Layout.RootPanel.Children.Add(pane);
        Assert.True(first.ShowDockablePreviewOnDrag);
        IDockDragPreviewPolicy firstPolicy = Assert.IsAssignableFrom<IDockDragPreviewPolicy>(first.DockFactory);
        IDockDragPreviewPolicy secondPolicy = Assert.IsAssignableFrom<IDockDragPreviewPolicy>(second.DockFactory);
        Assert.True(firstPolicy.ShowDockablePreviewOnDrag(document.DockModel));
        second.Layout.RootPanel.Children.Add(pane);
        Assert.False(secondPolicy.ShowDockablePreviewOnDrag(document.DockModel));
        second.ShowDockablePreviewOnDrag = true;
        Assert.True(secondPolicy.ShowDockablePreviewOnDrag(document.DockModel));
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Floating_Tabs_Use_Their_Current_Managers_Preview_Option(bool tool, bool enabled)
    {
        bool previous = DockSettings.UseManagedWindows;
        DockSettings.UseManagedWindows = false;
        DragPreviewWindow? previewWindow = null;
        using IDisposable subscription = Window.WindowOpenedEvent.AddClassHandler<DragPreviewWindow>((window, _) => previewWindow = window);
        try
        {
            using Workspace workspace = new(tool);
            if (tool) workspace.Manager.DockFactory.FloatAllDockables(workspace.First.DockModel);
            else workspace.First.Float();
            workspace.Update();
            workspace.Manager.ShowDockablePreviewOnDrag = enabled;
            LayoutFloatingWindowControl host = Assert.Single(workspace.Manager.FloatingWindows);
            ILayoutContainer? pane = workspace.First.Parent;
            Control tab = NativeTab(host, workspace.First, tool);
            Point start = Center(tab);
            host.MouseMove(start);
            host.MouseDown(start, MouseButton.Left);
            host.MouseMove(start + new Vector(12, 0), RawInputModifiers.LeftMouseButton);
            host.MouseMove(start + new Vector(0, tool ? -80 : 80), RawInputModifiers.LeftMouseButton);
            workspace.Update();
            DragPreviewControl preview = Assert.IsType<DragPreviewControl>(Assert.IsType<DragPreviewWindow>(previewWindow).Content);
            Assert.Equal(enabled, preview.ShowContent);
            if (enabled) Assert.NotNull(Assert.IsAssignableFrom<Image>(preview.PreviewContent).Source);
            else Assert.Null(preview.PreviewContent);
            host.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
            host.KeyRelease(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
            host.MouseUp(start, MouseButton.Left);
            workspace.Update();
            Assert.False(previewWindow!.IsVisible);
            Assert.True(host.IsVisible);
            Assert.Same(pane, workspace.First.Parent);
            Assert.True(workspace.First.IsFloating);
        }
        finally { DockSettings.UseManagedWindows = previous; }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Preview_Registration_Follows_Selection_Model_Removal_And_Visual_Detachment(bool tool)
    {
        using Workspace workspace = new(tool);
        workspace.First.IsActive = true;
        workspace.Update();
        IDictionary<IDockable, object> registry = tool ? workspace.Manager.DockFactory.ToolControls : workspace.Manager.DockFactory.DocumentControls;
        Control first = Assert.IsAssignableFrom<Control>(registry[workspace.First.DockModel]);
        Assert.Same(workspace.Window, TopLevel.GetTopLevel(first));
        if (tool) Assert.Same(workspace.First, Assert.IsType<LayoutAnchorableControl>(first).Model);
        else Assert.Same(workspace.First, Assert.IsType<LayoutDocumentControl>(first).Model);

        workspace.Second.IsActive = true;
        workspace.Update();
        Assert.False(registry.ContainsKey(workspace.First.DockModel));
        Assert.True(registry.ContainsKey(workspace.Second.DockModel));
        workspace.First.IsActive = true;
        workspace.Update();
        Assert.True(registry.ContainsKey(workspace.First.DockModel));
        Assert.False(registry.ContainsKey(workspace.Second.DockModel));

        workspace.First.Close();
        workspace.Update();
        Assert.False(registry.ContainsKey(workspace.First.DockModel));
        Assert.True(registry.ContainsKey(workspace.Second.DockModel));
        workspace.Window.Content = null;
        workspace.Update();
        Assert.False(registry.ContainsKey(workspace.Second.DockModel));
    }

    [AvaloniaTheory]
    [InlineData("CaptionPreview")]
    [InlineData("LegacyCaptionPreview")]
    public void The_Preview_Option_Compiles_In_Both_Xml_Namespaces(string key)
    {
        TabControlFixture fixture = new();
        Assert.False(Assert.IsType<DockingManager>(fixture.Resources[key]).ShowDockablePreviewOnDrag);
    }
}
