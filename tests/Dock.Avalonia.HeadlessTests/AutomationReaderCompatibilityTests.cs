using System.Collections.Generic;
using System.Linq;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Selectors;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class AutomationReaderCompatibilityTests
{
    [AvaloniaFact]
    public void AutomationReader_Can_Traverse_DockControl_PeerTree_And_Read_Metadata()
    {
        var context = CreateLayoutContext();
        var dockControl = new DockControl
        {
            Factory = context.Factory,
            Layout = context.Root
        };

        var window = new Window { Width = 960, Height = 600, Content = dockControl };
        window.Show();
        dockControl.ApplyTemplate();
        window.UpdateLayout();
        dockControl.UpdateLayout();

        if (dockControl is IDockSelectorService selectorService)
        {
            selectorService.ShowSelector(DockSelectorMode.Documents);
        }

        window.UpdateLayout();

        try
        {
            var windowPeer = ControlAutomationPeer.CreatePeerForElement(window);
            Assert.NotNull(windowPeer);

            var peers = EnumeratePeerTree(windowPeer!);
            Assert.NotEmpty(peers);

            Assert.Contains(peers, peer => peer.GetClassName() == nameof(DockControl));
            Assert.Contains(peers, peer => peer.GetClassName() == nameof(RootDockControl));
            Assert.Contains(
                peers,
                peer => peer.GetClassName() == nameof(DocumentControl) || peer.GetClassName() == nameof(MdiDocumentControl));
            Assert.Contains(peers, peer => peer.GetClassName() == nameof(DockSelectorOverlay));

            foreach (var peer in peers)
            {
                _ = peer.GetClassName();
                _ = peer.GetName();
                _ = peer.GetAutomationId();
                _ = peer.GetAutomationControlType();
                _ = peer.GetHelpText();
                _ = peer.IsControlElement();
                _ = peer.IsContentElement();
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void AutomationReader_Can_Use_Patterns_From_Discovered_Peers()
    {
        var context = CreateLayoutContext();

        var documentStrip = new DocumentTabStrip
        {
            DataContext = context.DocumentDock,
            ItemsSource = context.DocumentDock.VisibleDockables
        };

        var overlayItemA = new DockSelectorItem(context.DocumentA, 1, true, false, false, false, false);
        var overlayItemB = new DockSelectorItem(context.DocumentB, 2, true, false, false, false, false);
        var overlay = new DockSelectorOverlay
        {
            Mode = DockSelectorMode.Documents,
            Items = new[] { overlayItemA, overlayItemB },
            SelectedItem = overlayItemA,
            IsOpen = true
        };

        var host = new StackPanel
        {
            Children =
            {
                documentStrip,
                overlay
            }
        };

        var window = new Window { Width = 720, Height = 420, Content = host };
        window.Show();
        documentStrip.ApplyTemplate();
        overlay.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            var windowPeer = ControlAutomationPeer.CreatePeerForElement(window);
            Assert.NotNull(windowPeer);

            var peers = EnumeratePeerTree(windowPeer!);
            var documentStripPeer = peers.FirstOrDefault(peer => peer.GetClassName() == nameof(DocumentTabStrip));
            Assert.NotNull(documentStripPeer);

            var tabSelectionProvider = documentStripPeer.GetProvider<ISelectionProvider>();
            var tabScrollProvider = documentStripPeer.GetProvider<IScrollProvider>();
            Assert.NotNull(tabSelectionProvider);
            Assert.NotNull(tabScrollProvider);
            Assert.True(tabSelectionProvider.IsSelectionRequired);
            Assert.Single(tabSelectionProvider.GetSelection());

            var documentItemPeer = peers.FirstOrDefault(peer => peer.GetClassName() == nameof(DocumentTabStripItem));
            Assert.NotNull(documentItemPeer);

            var selectionItemProvider = documentItemPeer.GetProvider<ISelectionItemProvider>();
            var invokeProvider = documentItemPeer.GetProvider<IInvokeProvider>();
            Assert.NotNull(selectionItemProvider);
            Assert.NotNull(invokeProvider);
            Assert.NotNull(selectionItemProvider.SelectionContainer);

            context.DocumentDock.ActiveDockable = null;
            selectionItemProvider.Select();
            Assert.NotNull(context.DocumentDock.ActiveDockable);

            var overlayPeer = peers.FirstOrDefault(peer => peer.GetClassName() == nameof(DockSelectorOverlay));
            Assert.NotNull(overlayPeer);

            var overlaySelectionProvider = overlayPeer.GetProvider<ISelectionProvider>();
            var overlayExpandProvider = overlayPeer.GetProvider<IExpandCollapseProvider>();
            var overlayScrollProvider = overlayPeer.GetProvider<IScrollProvider>();
            var overlayValueProvider = overlayPeer.GetProvider<IValueProvider>();

            Assert.NotNull(overlaySelectionProvider);
            Assert.NotNull(overlayExpandProvider);
            Assert.NotNull(overlayScrollProvider);
            Assert.NotNull(overlayValueProvider);
            Assert.True(overlayValueProvider.IsReadOnly);
            Assert.Equal("Document A", overlayValueProvider.Value);

            overlayExpandProvider.Collapse();
            overlayExpandProvider.Expand();
            overlay.SelectedItem = overlayItemB;
            window.UpdateLayout();

            Assert.Equal("Document B", overlayValueProvider.Value);
            Assert.Single(overlaySelectionProvider.GetSelection());
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void AutomationReader_Receives_Overlay_Change_Events()
    {
        var context = CreateLayoutContext();
        var itemA = new DockSelectorItem(context.DocumentA, 1, true, false, false, false, false);
        var itemB = new DockSelectorItem(context.DocumentB, 2, true, false, false, false, false);
        var overlay = new DockSelectorOverlay
        {
            Mode = DockSelectorMode.Documents,
            Items = new[] { itemA, itemB },
            SelectedItem = itemA,
            IsOpen = true
        };

        var window = new Window { Width = 420, Height = 280, Content = overlay };
        window.Show();
        overlay.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            var overlayPeer = ControlAutomationPeer.CreatePeerForElement(overlay);
            Assert.NotNull(overlayPeer);

            var changedProperties = new List<AutomationProperty>();
            var childrenChangedCount = 0;
            overlayPeer.PropertyChanged += (_, args) => changedProperties.Add(args.Property);
            overlayPeer.ChildrenChanged += (_, _) => childrenChangedCount++;

            overlay.SelectedItem = itemB;
            overlay.Mode = DockSelectorMode.Tools;
            overlay.Items = new[] { itemB };
            overlay.IsOpen = false;
            overlay.IsOpen = true;

            Assert.Contains(SelectionPatternIdentifiers.SelectionProperty, changedProperties);
            Assert.Contains(ValuePatternIdentifiers.ValueProperty, changedProperties);
            Assert.Contains(AutomationElementIdentifiers.NameProperty, changedProperties);
            Assert.Contains(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, changedProperties);
            Assert.True(childrenChangedCount > 0);
        }
        finally
        {
            window.Close();
        }
    }

    private static List<AutomationPeer> EnumeratePeerTree(AutomationPeer root)
    {
        var result = new List<AutomationPeer>();
        var queue = new Queue<AutomationPeer>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var peer = queue.Dequeue();
            result.Add(peer);

            var children = peer.GetChildren();
            if (children is null)
            {
                continue;
            }

            foreach (var child in children)
            {
                queue.Enqueue(child);
            }
        }

        return result;
    }

    private static LayoutContext CreateLayoutContext()
    {
        var factory = new Factory();

        var root = factory.CreateRootDock();
        root.Id = "Root";
        root.Factory = factory;

        var documentDock = factory.CreateDocumentDock();
        documentDock.Id = "Documents";
        documentDock.Factory = factory;
        documentDock.Owner = root;

        var toolDock = factory.CreateToolDock();
        toolDock.Id = "Tools";
        toolDock.Factory = factory;
        toolDock.Owner = root;

        var documentA = new Document { Id = "DocA", Title = "Document A", Factory = factory, Owner = documentDock };
        var documentB = new Document { Id = "DocB", Title = "Document B", Factory = factory, Owner = documentDock };
        var toolA = new Tool { Id = "ToolA", Title = "Tool A", Factory = factory, Owner = toolDock };
        var toolB = new Tool { Id = "ToolB", Title = "Tool B", Factory = factory, Owner = toolDock };

        documentDock.VisibleDockables = factory.CreateList<IDockable>(documentA, documentB);
        documentDock.ActiveDockable = documentA;
        documentDock.FocusedDockable = documentA;

        toolDock.VisibleDockables = factory.CreateList<IDockable>(toolA, toolB);
        toolDock.ActiveDockable = toolA;
        toolDock.FocusedDockable = toolA;

        root.VisibleDockables = factory.CreateList<IDockable>(toolDock, documentDock);
        root.ActiveDockable = documentDock;
        root.FocusedDockable = documentA;

        return new LayoutContext(factory, root, documentDock, toolDock, documentA, documentB, toolA, toolB);
    }

    private sealed record LayoutContext(
        Factory Factory,
        IRootDock Root,
        IDocumentDock DocumentDock,
        IToolDock ToolDock,
        Document DocumentA,
        Document DocumentB,
        Tool ToolA,
        Tool ToolB);
}
