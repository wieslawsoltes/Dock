using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class GlobalSplitLayoutTests
{
    [AvaloniaFact]
    public void Splitting_A_Constrained_Root_Region_Preserves_Its_Minimum_Width()
    {
        Factory factory = new();
        Document document = new() { Title = "Document" };
        DocumentDock documents = new()
        {
            VisibleDockables = factory.CreateList<IDockable>(document), ActiveDockable = document
        };
        ProportionalDock region = Assert.IsType<ProportionalDock>(factory.CreateSplitLayout(documents, new ToolDock(), DockOperation.Right));
        region.VisibleDockables = factory.CreateList<IDockable>(documents);
        region.ActiveDockable = documents;
        region.DefaultDockable = documents;
        region.MinWidth = 500;
        RootDock root = new() { VisibleDockables = factory.CreateList<IDockable>(region), ActiveDockable = region };
        factory.InitLayout(root);
        Tool tool = new() { Title = "Incoming" };
        ToolDock incoming = new() { VisibleDockables = factory.CreateList<IDockable>(tool), ActiveDockable = tool };
        DockControl control = new() { Layout = root };
        Window window = new() { Width = 800, Height = 600, Content = control };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            // A different orientation requires the constrained region to become a pane.
            region.Orientation = Orientation.Vertical;
            factory.SplitToDock(region, incoming, DockOperation.Right);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            Control pane = Assert.IsAssignableFrom<Control>(factory.VisibleDockableControls[documents]);
            Assert.True(pane.Bounds.Width >= 500, $"The constrained region shrank to {pane.Bounds.Width} pixels.");
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Splitting_An_Inactive_Root_Region_Shows_The_New_Pane()
    {
        Factory factory = new();
        Document oldDocument = new() { Title = "Previous" };
        DocumentDock previous = new()
        {
            VisibleDockables = factory.CreateList<IDockable>(oldDocument), ActiveDockable = oldDocument
        };
        Document document = new() { Title = "Target" };
        DocumentDock documents = new()
        {
            VisibleDockables = factory.CreateList<IDockable>(document), ActiveDockable = document
        };
        ProportionalDock target = new()
        {
            IsCollapsable = false, Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>(documents), ActiveDockable = documents
        };
        RootDock root = new()
        {
            VisibleDockables = factory.CreateList<IDockable>(previous, target),
            ActiveDockable = previous, DefaultDockable = previous
        };
        factory.InitLayout(root);
        Tool tool = new() { Title = "Incoming" };
        ToolDock incoming = new() { VisibleDockables = factory.CreateList<IDockable>(tool), ActiveDockable = tool };
        DockControl control = new() { Layout = root };
        Window window = new() { Width = 800, Height = 600, Content = control };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            factory.SplitToDock(target, incoming, DockOperation.Right);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            Assert.Same(target, root.ActiveDockable);
            Assert.Same(previous, root.DefaultDockable);
            Control pane = Assert.IsAssignableFrom<Control>(factory.VisibleDockableControls[incoming]);
            Assert.Same(window, pane.GetVisualRoot());
            Assert.True(pane.Bounds.Width > 0 && pane.Bounds.Height > 0);
            Assert.Contains(control.GetVisualDescendants().OfType<ToolControl>(), item => ReferenceEquals(item.DataContext, incoming));
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Alternating_Window_Edges_Keep_Rendered_Regions_And_Resizable_Splitters()
    {
        Factory factory = new();
        Document document = new() { Title = "Document" };
        DocumentDock documents = new() { VisibleDockables = factory.CreateList<IDockable>(document), ActiveDockable = document };
        Tool tool = new() { Title = "Tool" };
        ToolDock bottom = new()
        {
            Alignment = Alignment.Bottom, VisibleDockables = factory.CreateList<IDockable>(tool), ActiveDockable = tool
        };
        ProportionalDock main = new()
        {
            Id = "MainLayout", IsCollapsable = false, Orientation = Orientation.Vertical,
            VisibleDockables = factory.CreateList<IDockable>(documents, new ProportionalDockSplitter(), bottom),
            ActiveDockable = documents, DefaultDockable = documents
        };
        RootDock root = new()
        {
            IsCollapsable = false, VisibleDockables = factory.CreateList<IDockable>(main),
            ActiveDockable = main, DefaultDockable = main
        };
        factory.InitLayout(root);
        DockControl control = new() { Layout = root };
        Window window = new() { Width = 1000, Height = 700, Content = control };
        DockManager manager = new(new DockService());
        DockOperation[] operations = { DockOperation.Right, DockOperation.Bottom, DockOperation.Left, DockOperation.Top };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            for (int iteration = 0; iteration < 20; iteration++)
            {
                Control dropControl = Assert.IsAssignableFrom<Control>(factory.VisibleDockableControls[documents]);
                IDock target = Assert.IsAssignableFrom<IDock>(GlobalDockingService.Instance.ResolveGlobalTargetDock(dropControl));
                Assert.Same(Assert.Single(root.VisibleDockables!), target);
                DockOperation operation = operations[iteration % operations.Length];
                Assert.True(manager.ValidateDockable(tool, target, DragAction.Move, operation, true));
                IDock inserted = Assert.IsAssignableFrom<IDock>(tool.Owner);
                Assert.True(GlobalDockingService.Instance.TryApplyGlobalDockingProportion(tool, root, root, 0.3));
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();

                Assert.Same(main, documents.Owner);
                Assert.Same(root.ActiveDockable, root.DefaultDockable);
                Assert.InRange(control.GetVisualDescendants().OfType<ProportionalStackPanel>().Count(), 1, 2);
                Control paneControl = Assert.IsAssignableFrom<Control>(factory.VisibleDockableControls[inserted]);
                Control documentControl = Assert.IsAssignableFrom<Control>(factory.VisibleDockableControls[documents]);
                Rect paneBounds = new(paneControl.TranslatePoint(default, control)!.Value, paneControl.Bounds.Size);
                Rect documentBounds = new(documentControl.TranslatePoint(default, control)!.Value, documentControl.Bounds.Size);
                Assert.True(paneBounds.Width > 0 && paneBounds.Height > 0);
                Assert.True(documentBounds.Width > 0 && documentBounds.Height > 0);
                switch (operation)
                {
                    case DockOperation.Left: Assert.True(paneBounds.Right <= documentBounds.Left); break;
                    case DockOperation.Right: Assert.True(documentBounds.Right <= paneBounds.Left); break;
                    case DockOperation.Top: Assert.True(paneBounds.Bottom <= documentBounds.Top); break;
                    case DockOperation.Bottom: Assert.True(documentBounds.Bottom <= paneBounds.Top); break;
                }

                ProportionalStackPanelSplitter splitter = Assert.Single(control.GetVisualDescendants().OfType<ProportionalStackPanelSplitter>());
                Point start = splitter.TranslatePoint(new Point(splitter.Bounds.Width / 2, splitter.Bounds.Height / 2), window)!.Value;
                Vector movement = operation is DockOperation.Left or DockOperation.Right ? new Vector(30, 0) : new Vector(0, 30);
                double oldShare = inserted.Proportion;
                window.MouseDown(start, MouseButton.Left);
                window.MouseMove(start + movement, RawInputModifiers.LeftMouseButton);
                window.MouseUp(start + movement, MouseButton.Left);
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                Assert.NotEqual(oldShare, inserted.Proportion);
                Assert.InRange(inserted.Proportion, 0.01, 0.99);
            }
        }
        finally
        {
            window.Close();
        }
    }
}
