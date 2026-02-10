using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Selectors;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using System.Collections.Generic;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class AutomationPeersTests
{
    [AvaloniaFact]
    public void DockControl_Exposes_Dock_Host_Peer_Metadata()
    {
        var context = CreateLayoutContext();
        var control = new DockControl
        {
            Layout = context.Root
        };

        var window = new Window { Width = 640, Height = 360, Content = control };
        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            var peer = ControlAutomationPeer.CreatePeerForElement(control);
            Assert.NotNull(peer);
            Assert.Equal(AutomationControlType.Pane, peer.GetAutomationControlType());
            Assert.Equal(nameof(DockControl), peer.GetClassName());
            Assert.Equal(context.Root.Id, peer.GetAutomationId());
            Assert.Contains("DockingEnabled=true", peer.GetHelpText());
            Assert.Contains("SelectorOpen=false", peer.GetHelpText());
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void RootDockControl_Exposes_Root_State_Metadata()
    {
        var context = CreateLayoutContext();
        var pinnedDock = context.Factory.CreateToolDock();
        pinnedDock.Id = "Pinned";
        pinnedDock.Factory = context.Factory;
        pinnedDock.Owner = context.Root;
        pinnedDock.VisibleDockables = context.Factory.CreateList<IDockable>(context.ToolA);
        pinnedDock.ActiveDockable = context.ToolA;
        context.Root.PinnedDock = pinnedDock;

        var control = new RootDockControl
        {
            DataContext = context.Root
        };

        var window = new Window { Width = 640, Height = 360, Content = control };
        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            var peer = ControlAutomationPeer.CreatePeerForElement(control);
            Assert.NotNull(peer);
            Assert.Equal(AutomationControlType.Pane, peer.GetAutomationControlType());
            Assert.Equal(nameof(RootDockControl), peer.GetClassName());
            Assert.Equal(context.Root.Id, peer.GetAutomationId());
            Assert.Contains("HasRoot=true", peer.GetHelpText());
            Assert.Contains("PinnedVisible=1", peer.GetHelpText());
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DockCommandBarHost_Exposes_ToolBar_Role_And_State()
    {
        var host = new DockCommandBarHost
        {
            MenuBars = new Control[] { new TextBlock { Text = "menu" } },
            ToolBars = new Control[] { new TextBlock { Text = "toolbar" } },
            RibbonBars = new Control[] { new TextBlock { Text = "ribbon" } }
        };

        var window = new Window { Width = 480, Height = 240, Content = host };
        window.Show();
        host.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            var peer = ControlAutomationPeer.CreatePeerForElement(host);
            Assert.NotNull(peer);
            Assert.Equal(AutomationControlType.ToolBar, peer.GetAutomationControlType());
            Assert.Equal(nameof(DockCommandBarHost), peer.GetClassName());
            Assert.Contains("MenuBars=1", peer.GetHelpText());
            Assert.Contains("ToolBars=1", peer.GetHelpText());
            Assert.Contains("RibbonBars=1", peer.GetHelpText());
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DockTarget_Creates_Dedicated_Peer_With_Pane_Role()
    {
        var target = new DockTarget
        {
            Focusable = true,
            ShowHorizontalTargets = true,
            ShowVerticalTargets = false,
            IsGlobalDockAvailable = true,
            IsGlobalDockActive = false
        };

        var window = new Window { Width = 320, Height = 240, Content = target };
        window.Show();
        target.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            var peer = ControlAutomationPeer.CreatePeerForElement(target);
            Assert.NotNull(peer);
            Assert.Equal(AutomationControlType.Pane, peer.GetAutomationControlType());
            Assert.Equal(nameof(DockTarget), peer.GetClassName());
            Assert.Contains("Horizontal=true", peer.GetHelpText());
            Assert.Contains("GlobalAvailable=true", peer.GetHelpText());

            var invokeProvider = peer.GetProvider<IInvokeProvider>();
            Assert.NotNull(invokeProvider);
            invokeProvider.Invoke();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DocumentTabStripItem_Invoke_Activates_Document_And_Exposes_TabItem_Role()
    {
        var context = CreateLayoutContext();
        var item = new DocumentTabStripItem { DataContext = context.DocumentA };
        var window = new Window { Width = 320, Height = 120, Content = item };
        window.Show();
        item.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            context.DocumentDock.ActiveDockable = null;
            var peer = ControlAutomationPeer.CreatePeerForElement(item);
            Assert.NotNull(peer);
            Assert.Equal(AutomationControlType.TabItem, peer.GetAutomationControlType());
            Assert.Equal(context.DocumentA.Title, peer.GetName());
            Assert.Equal(context.DocumentA.Id, peer.GetAutomationId());

            var selectionItemProvider = peer.GetProvider<ISelectionItemProvider>();
            Assert.NotNull(selectionItemProvider);
            Assert.False(selectionItemProvider.IsSelected);

            var selectionStateChanged = false;
            peer.PropertyChanged += (_, args) =>
            {
                if (args.Property == SelectionItemPatternIdentifiers.IsSelectedProperty)
                {
                    selectionStateChanged = true;
                }
            };

            var invokeProvider = peer.GetProvider<IInvokeProvider>();
            Assert.NotNull(invokeProvider);
            invokeProvider.Invoke();

            Assert.Same(context.DocumentA, context.DocumentDock.ActiveDockable);
            Assert.True(item.IsSelected);
            Assert.True(selectionItemProvider.IsSelected);
            Assert.True(selectionStateChanged);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ToolTabStripItem_Invoke_Activates_Tool_And_Exposes_TabItem_Role()
    {
        var context = CreateLayoutContext();
        var item = new ToolTabStripItem { DataContext = context.ToolA };
        var window = new Window { Width = 320, Height = 120, Content = item };
        window.Show();
        item.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            context.ToolDock.ActiveDockable = null;
            var peer = ControlAutomationPeer.CreatePeerForElement(item);
            Assert.NotNull(peer);
            Assert.Equal(AutomationControlType.TabItem, peer.GetAutomationControlType());
            Assert.Equal(context.ToolA.Title, peer.GetName());
            Assert.Equal(context.ToolA.Id, peer.GetAutomationId());

            var selectionItemProvider = peer.GetProvider<ISelectionItemProvider>();
            Assert.NotNull(selectionItemProvider);
            Assert.False(selectionItemProvider.IsSelected);

            var selectionStateChanged = false;
            peer.PropertyChanged += (_, args) =>
            {
                if (args.Property == SelectionItemPatternIdentifiers.IsSelectedProperty)
                {
                    selectionStateChanged = true;
                }
            };

            var invokeProvider = peer.GetProvider<IInvokeProvider>();
            Assert.NotNull(invokeProvider);
            invokeProvider.Invoke();

            Assert.Same(context.ToolA, context.ToolDock.ActiveDockable);
            Assert.True(item.IsSelected);
            Assert.True(selectionItemProvider.IsSelected);
            Assert.True(selectionStateChanged);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DocumentControl_Invoke_Activates_Document_And_Exposes_Document_Host_Role()
    {
        var context = CreateLayoutContext();
        var control = new DocumentControl { DataContext = context.DocumentDock };
        var window = new Window { Width = 480, Height = 240, Content = control };
        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            context.DocumentDock.ActiveDockable = null;

            var peer = ControlAutomationPeer.CreatePeerForElement(control);
            Assert.NotNull(peer);
            Assert.Equal(AutomationControlType.Pane, peer.GetAutomationControlType());
            Assert.Equal(nameof(DocumentControl), peer.GetClassName());
            Assert.Equal(context.DocumentDock.Id, peer.GetAutomationId());
            Assert.Contains("HasVisibleDockables=", peer.GetHelpText());

            var invokeProvider = peer.GetProvider<IInvokeProvider>();
            Assert.NotNull(invokeProvider);
            invokeProvider.Invoke();
            Assert.Same(context.DocumentA, context.DocumentDock.ActiveDockable);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ToolControl_Invoke_Activates_Tool_And_Exposes_Tool_Host_Role()
    {
        var context = CreateLayoutContext();
        var control = new ToolControl { DataContext = context.ToolDock };
        var window = new Window { Width = 480, Height = 240, Content = control };
        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            context.ToolDock.ActiveDockable = null;

            var peer = ControlAutomationPeer.CreatePeerForElement(control);
            Assert.NotNull(peer);
            Assert.Equal(AutomationControlType.Pane, peer.GetAutomationControlType());
            Assert.Equal(nameof(ToolControl), peer.GetClassName());
            Assert.Equal(context.ToolDock.Id, peer.GetAutomationId());
            Assert.Contains("VisibleDockables=2", peer.GetHelpText());

            var invokeProvider = peer.GetProvider<IInvokeProvider>();
            Assert.NotNull(invokeProvider);
            invokeProvider.Invoke();
            Assert.Same(context.ToolA, context.ToolDock.ActiveDockable);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void MdiDocumentControl_Invoke_Activates_Document_And_Exposes_Mdi_Host_Role()
    {
        var context = CreateLayoutContext();
        var control = new MdiDocumentControl { DataContext = context.DocumentDock };
        var window = new Window { Width = 480, Height = 240, Content = control };
        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            context.DocumentDock.ActiveDockable = null;

            var peer = ControlAutomationPeer.CreatePeerForElement(control);
            Assert.NotNull(peer);
            Assert.Equal(AutomationControlType.Pane, peer.GetAutomationControlType());
            Assert.Equal(nameof(MdiDocumentControl), peer.GetClassName());
            Assert.Equal(context.DocumentDock.Id, peer.GetAutomationId());
            Assert.Contains("HasVisibleDocuments=", peer.GetHelpText());

            var invokeProvider = peer.GetProvider<IInvokeProvider>();
            Assert.NotNull(invokeProvider);
            invokeProvider.Invoke();
            Assert.Same(context.DocumentA, context.DocumentDock.ActiveDockable);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DocumentTabStrip_Exposes_Tab_Role_And_State()
    {
        var context = CreateLayoutContext();
        context.DocumentDock.CanCreateDocument = true;

        var strip = new DocumentTabStrip
        {
            DataContext = context.DocumentDock,
            ItemsSource = context.DocumentDock.VisibleDockables,
            CanCreateItem = true,
            IsActive = true
        };

        var window = new Window { Width = 480, Height = 240, Content = strip };
        window.Show();
        strip.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            var peer = ControlAutomationPeer.CreatePeerForElement(strip);
            Assert.NotNull(peer);
            Assert.Equal(AutomationControlType.Tab, peer.GetAutomationControlType());
            Assert.Equal(nameof(DocumentTabStrip), peer.GetClassName());
            Assert.Equal(context.DocumentDock.Id, peer.GetAutomationId());
            Assert.Contains("CanCreate=true", peer.GetHelpText());
            Assert.Contains("Items=2", peer.GetHelpText());

            var selectionProvider = peer.GetProvider<ISelectionProvider>();
            Assert.NotNull(selectionProvider);
            Assert.False(selectionProvider.CanSelectMultiple);
            Assert.True(selectionProvider.IsSelectionRequired);
            Assert.Single(selectionProvider.GetSelection());

            var scrollProvider = peer.GetProvider<IScrollProvider>();
            Assert.NotNull(scrollProvider);
            scrollProvider.Scroll(ScrollAmount.SmallIncrement, ScrollAmount.NoAmount);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ToolTabStrip_Exposes_Tab_Role_And_State()
    {
        var context = CreateLayoutContext();
        var strip = new ToolTabStrip
        {
            DataContext = context.ToolDock,
            ItemsSource = context.ToolDock.VisibleDockables,
            CanCreateItem = true
        };

        var window = new Window { Width = 480, Height = 240, Content = strip };
        window.Show();
        strip.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            var peer = ControlAutomationPeer.CreatePeerForElement(strip);
            Assert.NotNull(peer);
            Assert.Equal(AutomationControlType.Tab, peer.GetAutomationControlType());
            Assert.Equal(nameof(ToolTabStrip), peer.GetClassName());
            Assert.Equal(context.ToolDock.Id, peer.GetAutomationId());
            Assert.Contains("CanCreate=true", peer.GetHelpText());
            Assert.Contains("Items=2", peer.GetHelpText());

            var selectionProvider = peer.GetProvider<ISelectionProvider>();
            Assert.NotNull(selectionProvider);
            Assert.False(selectionProvider.CanSelectMultiple);
            Assert.True(selectionProvider.IsSelectionRequired);
            Assert.Single(selectionProvider.GetSelection());

            var scrollProvider = peer.GetProvider<IScrollProvider>();
            Assert.NotNull(scrollProvider);
            scrollProvider.Scroll(ScrollAmount.SmallIncrement, ScrollAmount.NoAmount);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ToolChromeControl_Exposes_TitleBar_Role_And_Flyout_ExpandCollapse()
    {
        var context = CreateLayoutContext();
        var flyout = new Flyout { Content = new TextBlock { Text = "menu" } };
        var chrome = new ToolChromeControl
        {
            DataContext = context.ToolDock,
            ToolFlyout = flyout
        };

        var window = new Window { Width = 480, Height = 320, Content = chrome };
        window.Show();
        chrome.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            context.ToolDock.ActiveDockable = null;
            context.Root.ActiveDockable = context.ToolDock;

            var peer = ControlAutomationPeer.CreatePeerForElement(chrome);
            Assert.NotNull(peer);
            Assert.Equal(AutomationControlType.TitleBar, peer.GetAutomationControlType());
            Assert.Contains("HasMenu=true", peer.GetHelpText());

            var invokeProvider = peer.GetProvider<IInvokeProvider>();
            Assert.NotNull(invokeProvider);
            context.ToolDock.ActiveDockable = context.ToolA;
            context.Root.ActiveDockable = context.ToolDock;
            invokeProvider.Invoke();
            Assert.Same(context.ToolA, context.ToolDock.ActiveDockable);

            var expandCollapse = peer.GetProvider<IExpandCollapseProvider>();
            Assert.NotNull(expandCollapse);
            Assert.Equal(ExpandCollapseState.Collapsed, expandCollapse.ExpandCollapseState);
            expandCollapse.Expand();
            Assert.Equal(ExpandCollapseState.Expanded, expandCollapse.ExpandCollapseState);
            expandCollapse.Collapse();
            Assert.Equal(ExpandCollapseState.Collapsed, expandCollapse.ExpandCollapseState);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void MdiDocumentWindow_Invoke_Activates_Document_And_Exposes_Window_Role()
    {
        var context = CreateLayoutContext();
        context.DocumentA.MdiState = MdiWindowState.Maximized;

        var windowControl = new MdiDocumentWindow { DataContext = context.DocumentA };
        var window = new Window { Width = 480, Height = 320, Content = windowControl };
        window.Show();
        windowControl.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            context.DocumentDock.ActiveDockable = null;
            var peer = ControlAutomationPeer.CreatePeerForElement(windowControl);
            Assert.NotNull(peer);
            Assert.Equal(AutomationControlType.Window, peer.GetAutomationControlType());
            Assert.Equal(context.DocumentA.Title, peer.GetName());
            Assert.Contains("MdiState=Maximized", peer.GetHelpText());

            var invokeProvider = peer.GetProvider<IInvokeProvider>();
            Assert.NotNull(invokeProvider);
            invokeProvider.Invoke();
            Assert.Same(context.DocumentA, context.DocumentDock.ActiveDockable);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DockSelectorOverlay_Exposes_List_Role_And_ExpandCollapse()
    {
        var context = CreateLayoutContext();
        var itemA = new DockSelectorItem(context.DocumentA, 1, true, false, false, false, false);
        var itemB = new DockSelectorItem(context.DocumentB, 2, true, false, false, false, false);
        var overlay = new DockSelectorOverlay
        {
            Mode = DockSelectorMode.Documents,
            Items = new[] { itemA, itemB },
            SelectedItem = itemA,
            IsOpen = false
        };

        var window = new Window { Width = 480, Height = 320, Content = overlay };
        window.Show();
        overlay.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            var peer = ControlAutomationPeer.CreatePeerForElement(overlay);
            Assert.NotNull(peer);
            Assert.Equal(AutomationControlType.List, peer.GetAutomationControlType());
            Assert.Equal("Documents selector", peer.GetName());
            Assert.Contains("Mode=Documents", peer.GetHelpText());
            Assert.Contains("ItemCount=2", peer.GetHelpText());

            var selectionProvider = peer.GetProvider<ISelectionProvider>();
            Assert.NotNull(selectionProvider);
            Assert.False(selectionProvider.CanSelectMultiple);

            var valueProvider = peer.GetProvider<IValueProvider>();
            Assert.NotNull(valueProvider);
            Assert.True(valueProvider.IsReadOnly);
            Assert.Equal("Document A", valueProvider.Value);

            var scrollProvider = peer.GetProvider<IScrollProvider>();
            Assert.NotNull(scrollProvider);

            var raisedProperties = new List<AutomationProperty>();
            peer.PropertyChanged += (_, args) => raisedProperties.Add(args.Property);

            var expandCollapse = peer.GetProvider<IExpandCollapseProvider>();
            Assert.NotNull(expandCollapse);
            Assert.Equal(ExpandCollapseState.Collapsed, expandCollapse.ExpandCollapseState);
            expandCollapse.Expand();
            Assert.True(overlay.IsOpen);
            Assert.Equal(ExpandCollapseState.Expanded, expandCollapse.ExpandCollapseState);
            window.UpdateLayout();
            Assert.Single(selectionProvider.GetSelection());

            overlay.SelectedItem = itemB;
            Assert.Equal("Document B", valueProvider.Value);

            expandCollapse.Collapse();
            Assert.False(overlay.IsOpen);
            Assert.Equal(ExpandCollapseState.Collapsed, expandCollapse.ExpandCollapseState);

            Assert.Contains(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, raisedProperties);
            Assert.Contains(SelectionPatternIdentifiers.SelectionProperty, raisedProperties);
            Assert.Contains(ValuePatternIdentifiers.ValueProperty, raisedProperties);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void PinnedDockControl_Exposes_Pinned_State_And_ExpandCollapse()
    {
        var context = CreateLayoutContext();
        var pinnedDock = context.Factory.CreateToolDock();
        pinnedDock.Id = "Pinned";
        pinnedDock.Factory = context.Factory;
        pinnedDock.Owner = context.Root;
        pinnedDock.VisibleDockables = context.Factory.CreateList<IDockable>(context.ToolA);
        pinnedDock.ActiveDockable = context.ToolA;
        pinnedDock.IsExpanded = false;
        context.Root.PinnedDock = pinnedDock;

        var control = new PinnedDockControl
        {
            DataContext = context.Root
        };

        var window = new Window { Width = 640, Height = 360, Content = control };
        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            var peer = ControlAutomationPeer.CreatePeerForElement(control);
            Assert.NotNull(peer);
            Assert.Equal(AutomationControlType.Pane, peer.GetAutomationControlType());
            Assert.Equal(nameof(PinnedDockControl), peer.GetClassName());
            Assert.True(
                peer.GetAutomationId() == pinnedDock.Id || peer.GetAutomationId() == context.Root.Id,
                $"Unexpected automation id '{peer.GetAutomationId()}'.");
            Assert.Contains("HasPinnedDock=true", peer.GetHelpText());
            Assert.Contains("VisibleDockables=1", peer.GetHelpText());

            var expandCollapse = peer.GetProvider<IExpandCollapseProvider>();
            Assert.NotNull(expandCollapse);
            Assert.Equal(ExpandCollapseState.Collapsed, expandCollapse.ExpandCollapseState);
            expandCollapse.Expand();
            Assert.True(pinnedDock.IsExpanded);
            Assert.Equal(ExpandCollapseState.Expanded, expandCollapse.ExpandCollapseState);
            expandCollapse.Collapse();
            Assert.False(pinnedDock.IsExpanded);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ToolPinnedControl_And_PinItem_Expose_Tab_Roles_And_Invoke()
    {
        var context = CreateLayoutContext();
        var pinnedControl = new ToolPinnedControl
        {
            ItemsSource = new[] { context.ToolA, context.ToolB }
        };

        var window = new Window { Width = 480, Height = 240, Content = pinnedControl };
        window.Show();
        pinnedControl.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            var pinnedPeer = ControlAutomationPeer.CreatePeerForElement(pinnedControl);
            Assert.NotNull(pinnedPeer);
            Assert.Equal(AutomationControlType.Tab, pinnedPeer.GetAutomationControlType());
            Assert.Equal(nameof(ToolPinnedControl), pinnedPeer.GetClassName());
            Assert.Contains("Items=2", pinnedPeer.GetHelpText());

            var selectionProvider = pinnedPeer.GetProvider<ISelectionProvider>();
            Assert.NotNull(selectionProvider);
            Assert.Single(selectionProvider.GetSelection());

            var pinItem = pinnedControl.ContainerFromIndex(0) as ToolPinItemControl;
            Assert.NotNull(pinItem);

            context.ToolDock.ActiveDockable = null;
            var pinItemPeer = ControlAutomationPeer.CreatePeerForElement(pinItem!);
            Assert.NotNull(pinItemPeer);
            Assert.Equal(AutomationControlType.TabItem, pinItemPeer.GetAutomationControlType());
            Assert.Equal(nameof(ToolPinItemControl), pinItemPeer.GetClassName());
            Assert.Equal(context.ToolA.Id, pinItemPeer.GetAutomationId());

            var selectionItemProvider = pinItemPeer.GetProvider<ISelectionItemProvider>();
            Assert.NotNull(selectionItemProvider);
            Assert.NotNull(selectionItemProvider.SelectionContainer);

            var invokeProvider = pinItemPeer.GetProvider<IInvokeProvider>();
            Assert.NotNull(invokeProvider);
            invokeProvider.Invoke();
            Assert.Same(context.ToolA, context.ToolDock.ActiveDockable);
            Assert.True(selectionItemProvider.IsSelected);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void HostWindow_And_TitleBar_Expose_Dedicated_Peers()
    {
        var hostWindow = new HostWindow
        {
            Width = 640,
            Height = 400,
            Content = new HostWindowTitleBar()
        };

        hostWindow.Show();
        hostWindow.ApplyTemplate();
        hostWindow.UpdateLayout();

        try
        {
            var hostPeer = ControlAutomationPeer.CreatePeerForElement(hostWindow);
            Assert.NotNull(hostPeer);
            Assert.Equal(AutomationControlType.Window, hostPeer.GetAutomationControlType());
            Assert.Equal(nameof(HostWindow), hostPeer.GetClassName());

            var hostInvoke = hostPeer.GetProvider<IInvokeProvider>();
            Assert.NotNull(hostInvoke);
            hostInvoke.Invoke();

            var titleBar = hostWindow.Content as HostWindowTitleBar;
            Assert.NotNull(titleBar);
            var titlePeer = ControlAutomationPeer.CreatePeerForElement(titleBar!);
            Assert.NotNull(titlePeer);
            Assert.Equal(AutomationControlType.TitleBar, titlePeer.GetAutomationControlType());
            Assert.Equal(nameof(HostWindowTitleBar), titlePeer.GetClassName());

            var titleInvoke = titlePeer.GetProvider<IInvokeProvider>();
            Assert.NotNull(titleInvoke);
            titleInvoke.Invoke();
        }
        finally
        {
            hostWindow.Close();
        }
    }

    [AvaloniaFact]
    public void PinnedDockWindow_Exposes_Window_Role_And_Invoke()
    {
        var pinnedWindow = new PinnedDockWindow
        {
            Width = 420,
            Height = 240,
            Content = new TextBlock { Text = "Pinned" }
        };

        pinnedWindow.Show();
        pinnedWindow.ApplyTemplate();
        pinnedWindow.UpdateLayout();

        try
        {
            var peer = ControlAutomationPeer.CreatePeerForElement(pinnedWindow);
            Assert.NotNull(peer);
            Assert.Equal(AutomationControlType.Window, peer.GetAutomationControlType());
            Assert.Equal(nameof(PinnedDockWindow), peer.GetClassName());

            var invokeProvider = peer.GetProvider<IInvokeProvider>();
            Assert.NotNull(invokeProvider);
            invokeProvider.Invoke();
        }
        finally
        {
            pinnedWindow.Close();
        }
    }

    [AvaloniaFact]
    public void DragPreview_And_Adorner_Windows_Are_NonInteractive_Panes()
    {
        var dragPreviewWindow = new DragPreviewWindow
        {
            Width = 320,
            Height = 180,
            Content = new TextBlock { Text = "Preview" }
        };

        var adornerWindow = new DockAdornerWindow
        {
            Width = 320,
            Height = 180,
            Content = new TextBlock { Text = "Adorner" }
        };

        dragPreviewWindow.Show();
        adornerWindow.Show();
        dragPreviewWindow.ApplyTemplate();
        adornerWindow.ApplyTemplate();
        dragPreviewWindow.UpdateLayout();
        adornerWindow.UpdateLayout();

        try
        {
            var previewPeer = ControlAutomationPeer.CreatePeerForElement(dragPreviewWindow);
            Assert.NotNull(previewPeer);
            Assert.Equal(nameof(DragPreviewWindow), previewPeer.GetClassName());
            Assert.Equal(AutomationControlType.Pane, previewPeer.GetAutomationControlType());
            Assert.False(previewPeer.IsControlElement());
            Assert.False(previewPeer.IsContentElement());

            var adornerPeer = ControlAutomationPeer.CreatePeerForElement(adornerWindow);
            Assert.NotNull(adornerPeer);
            Assert.Equal(nameof(DockAdornerWindow), adornerPeer.GetClassName());
            Assert.Equal(AutomationControlType.Pane, adornerPeer.GetAutomationControlType());
            Assert.False(adornerPeer.IsControlElement());
            Assert.False(adornerPeer.IsContentElement());
        }
        finally
        {
            adornerWindow.Close();
            dragPreviewWindow.Close();
        }
    }

    [AvaloniaFact]
    public void DragPreviewControl_Exposes_ReadOnly_Value_Provider_And_Change_Event()
    {
        var control = new DragPreviewControl
        {
            Status = "Dock",
            ShowContent = true
        };

        var window = new Window { Width = 420, Height = 240, Content = control };
        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            var peer = ControlAutomationPeer.CreatePeerForElement(control);
            Assert.NotNull(peer);
            Assert.Equal(AutomationControlType.Pane, peer.GetAutomationControlType());
            Assert.Equal(nameof(DragPreviewControl), peer.GetClassName());

            var valueProvider = peer.GetProvider<IValueProvider>();
            Assert.NotNull(valueProvider);
            Assert.True(valueProvider.IsReadOnly);
            Assert.Equal("Dock", valueProvider.Value);

            var changedProperties = new List<AutomationProperty>();
            peer.PropertyChanged += (_, args) => changedProperties.Add(args.Property);

            control.Status = "Float";
            Assert.Equal("Float", valueProvider.Value);
            Assert.Contains(ValuePatternIdentifiers.ValueProperty, changedProperties);
        }
        finally
        {
            window.Close();
        }
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
