using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model.Core;
using Dock.Model.Controls;
using Dock.Model.Mvvm;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockDragPreviewPolicyTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Document_Tab_Drag_Uses_Content_Size_Only_For_Full_Previews(bool enabled)
    {
        bool oldPreview = DockSettings.ShowDockablePreviewOnDrag;
        bool oldManaged = DockSettings.UseManagedWindows;
        PreviewFactory factory = new() { Enabled = enabled };
        IDocument document = factory.CreateDocument();
        document.Title = "Document preview";
        IDocumentDock pane = factory.CreateDocumentDock();
        pane.VisibleDockables = factory.CreateList<IDockable>(document);
        pane.ActiveDockable = document;
        IRootDock root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>(pane);
        root.ActiveDockable = pane;
        factory.InitLayout(root);
        DockControl host = new() { Layout = root, Factory = factory };
        Window owner = new() { Width = 800, Height = 550, Content = host };
        DragPreviewWindow? window = null;
        using var subscription = Window.WindowOpenedEvent.AddClassHandler<DragPreviewWindow>((opened, _) => window = opened);
        DockControlState state = Assert.IsType<DockControlState>(host.DockControlState);
        try
        {
            DockSettings.ShowDockablePreviewOnDrag = !enabled;
            DockSettings.UseManagedWindows = false;
            owner.Show();
            Dispatcher.UIThread.RunJobs();
            owner.UpdateLayout();
            DocumentTabStripItem tab = Assert.Single(host.GetVisualDescendants().OfType<DocumentTabStripItem>());
            Size tabSize = tab.Bounds.Size;
            state.StartDrag(tab, new Point(5, 5), new Point(100, 100), host);
            Dispatcher.UIThread.RunJobs();
            Assert.NotNull(window);
            window.UpdateLayout();
            DragPreviewControl preview = Assert.IsType<DragPreviewControl>(window.Content);
            Assert.Equal(enabled, preview.ShowContent);
            if (enabled)
            {
                Assert.True(window.Bounds.Width > 400);
                Assert.True(window.Bounds.Height > 250);
                Assert.True(preview.PreviewContentWidth > tabSize.Width);
                Assert.True(preview.PreviewContentHeight > tabSize.Height);
            }
            else
            {
                Assert.Equal(tabSize.Width + tab.Margin.Left + tab.Margin.Right, window.Bounds.Width, 1);
                Assert.Equal(tabSize.Height + tab.Margin.Top + tab.Margin.Bottom, window.Bounds.Height, 1);
            }
        }
        finally
        {
            state.Process(default, default, EventType.CaptureLost, DragAction.None, host, [host]);
            owner.Close();
            Dispatcher.UIThread.RunJobs();
            DockSettings.ShowDockablePreviewOnDrag = oldPreview;
            DockSettings.UseManagedWindows = oldManaged;
        }
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Factory_Policy_Controls_Native_And_Managed_Previews_Without_Changing_Global_Settings(bool managed, bool enabled)
    {
        bool oldPreview = DockSettings.ShowDockablePreviewOnDrag;
        bool oldManaged = DockSettings.UseManagedWindows;
        PreviewFactory factory = new() { Enabled = enabled };
        IDockable document = factory.CreateDocument();
        document.Factory = factory;
        Border live = new() { Background = Brushes.Crimson };
        ManagedWindowLayer layer = new();
        Window owner = new() { Width = 500, Height = 350, Content = new Grid { Children = { live, layer } } };
        DragPreviewWindow? window = null;
        using var subscription = Window.WindowOpenedEvent.AddClassHandler<DragPreviewWindow>((opened, _) => window = opened);
        DragPreviewHelper helper = new();
        helper.Hide();
        try
        {
            DockSettings.ShowDockablePreviewOnDrag = !enabled;
            DockSettings.UseManagedWindows = managed;
            owner.Show();
            owner.UpdateLayout();
            factory.DocumentControls[document] = live;
            if (managed) ManagedWindowRegistry.RegisterLayer(factory, layer);
            helper.Show(document, new PixelPoint(50, 60), default, layer, new Size(300, 200));
            Dispatcher.UIThread.RunJobs();
            owner.UpdateLayout();
            DragPreviewControl preview = managed
                ? Assert.Single(layer.GetVisualDescendants().OfType<DragPreviewControl>())
                : Assert.IsType<DragPreviewControl>(Assert.IsType<DragPreviewWindow>(window).Content);
            Assert.Equal(enabled, preview.ShowContent);
            Assert.Same(document, factory.LastDockable);
            Assert.Equal(!enabled, DockSettings.ShowDockablePreviewOnDrag);
            if (enabled)
            {
                DragPreviewSnapshot snapshot = Assert.IsType<DragPreviewSnapshot>(preview.PreviewContent);
                Assert.NotNull(snapshot.Source);
            }
            else Assert.Null(preview.PreviewContent);
            Assert.Same(owner, TopLevel.GetTopLevel(live));
            Assert.Same(live, factory.DocumentControls[document]);

            // The same factory can change policy between drags, independently of
            // native or managed hosting and other factories' global defaults.
            helper.Hide();
            factory.Enabled = !enabled;
            helper.Show(document, new PixelPoint(60, 70), default, layer, new Size(300, 200));
            owner.UpdateLayout();
            preview = managed
                ? Assert.Single(layer.GetVisualDescendants().OfType<DragPreviewControl>())
                : Assert.IsType<DragPreviewControl>(Assert.IsType<DragPreviewWindow>(window).Content);
            Assert.Equal(!enabled, preview.ShowContent);
            Assert.Equal(!enabled, DockSettings.ShowDockablePreviewOnDrag);
        }
        finally
        {
            helper.Hide();
            ManagedWindowRegistry.UnregisterLayer(factory, layer);
            owner.Close();
            Dispatcher.UIThread.RunJobs();
            DockSettings.ShowDockablePreviewOnDrag = oldPreview;
            DockSettings.UseManagedWindows = oldManaged;
        }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Factories_Without_A_Policy_Keep_The_Global_Preview_Setting(bool enabled)
    {
        bool previous = DockSettings.ShowDockablePreviewOnDrag;
        try
        {
            DockSettings.ShowDockablePreviewOnDrag = enabled;
            Factory factory = new();
            IDockable document = factory.CreateDocument();
            document.Factory = factory;
            Assert.Equal(enabled, DragPreviewHelper.ShouldShowPreviewContent(document));
            document.Factory = null;
            Assert.Equal(enabled, DragPreviewHelper.ShouldShowPreviewContent(document));
        }
        finally { DockSettings.ShowDockablePreviewOnDrag = previous; }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Whole_Pane_Preview_Uses_The_Registered_Visual_Without_Rebuilding_Its_Content(bool tool)
    {
        Factory factory = new();
        IDock pane = tool ? factory.CreateToolDock() : factory.CreateDocumentDock();
        pane.Factory = factory;
        IDockable item = tool ? factory.CreateTool() : factory.CreateDocument();
        pane.VisibleDockables = factory.CreateList(item);
        item.Owner = pane;
        pane.ActiveDockable = item;
        TrackedPane live = new() { Child = new Button { Content = "Custom pane" } };
        Window owner = new() { Width = 500, Height = 350, Content = live };
        owner.Show();
        try
        {
            factory.VisibleDockableControls[pane] = live;
            owner.UpdateLayout();
            using DragPreviewSnapshot preview = Assert.IsType<DragPreviewSnapshot>(DragPreviewHelper.BuildDockablePreviewContent(pane, true));
            Assert.NotNull(preview.Source);
            Assert.Same(owner, TopLevel.GetTopLevel(live));
            Assert.Same(live, factory.VisibleDockableControls[pane]);
            Assert.Same(item, pane.ActiveDockable);
            Assert.Same(pane, item.Owner);
            Assert.Same(item, Assert.Single(pane.VisibleDockables));
            Assert.Null(DragPreviewHelper.BuildDockablePreviewContent(pane, false));
            owner.Content = null;
            Assert.Null(DragPreviewHelper.BuildDockablePreviewContent(pane, true));
        }
        finally { owner.Close(); }
    }

    private sealed class PreviewFactory : Factory, IDockDragPreviewPolicy
    {
        internal bool Enabled { get; set; }
        internal IDockable? LastDockable { get; private set; }
        public bool ShowDockablePreviewOnDrag(IDockable dockable)
        {
            LastDockable = dockable;
            return Enabled;
        }
    }

    private sealed class TrackedPane : Border, IDockableControl;
}
