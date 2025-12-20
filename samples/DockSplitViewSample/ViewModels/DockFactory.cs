// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;

namespace DockSplitViewSample.ViewModels;

public class DockFactory : Factory
{
    private readonly Dictionary<string, IDockable> _dockables = new();
    
    public ISplitViewDock? SplitViewDock { get; private set; }
    public IDocumentDock? DocumentDock { get; private set; }

    public IDockable? GetDockable(string id)
    {
        return _dockables.TryGetValue(id, out var dockable) ? dockable : null;
    }

    public override IRootDock CreateLayout()
    {
        // Create pane content (navigation items)
        var navTool = new NavigationViewModel
        {
            Id = "Navigation",
            Title = "Navigation"
        };
        _dockables[navTool.Id] = navTool;

        // Create main content documents
        var homeDocument = new HomeViewModel
        {
            Id = "Home",
            Title = "Home"
        };
        _dockables[homeDocument.Id] = homeDocument;

        var settingsDocument = new SettingsViewModel
        {
            Id = "Settings",
            Title = "Settings"
        };
        _dockables[settingsDocument.Id] = settingsDocument;

        var aboutDocument = new AboutViewModel
        {
            Id = "About",
            Title = "About"
        };
        _dockables[aboutDocument.Id] = aboutDocument;

        // Create document dock for main content area
        DocumentDock = new DocumentDock
        {
            Id = "DocumentDock",
            Title = "Documents",
            IsCollapsable = false,
            CanCreateDocument = false,
            VisibleDockables = CreateList<IDockable>(homeDocument, settingsDocument, aboutDocument),
            ActiveDockable = homeDocument
        };

        // Create the SplitViewDock
        SplitViewDock = new SplitViewDock
        {
            Id = "SplitViewDock",
            Title = "Main",
            DisplayMode = SplitViewDisplayMode.CompactOverlay,
            IsPaneOpen = false,
            CompactPaneLength = 48,
            OpenPaneLength = 280,
            PanePlacement = SplitViewPanePlacement.Left,
            PaneDockable = navTool,
            ContentDockable = DocumentDock
        };

        // Wire up the SplitViewDock reference to settings
        settingsDocument.SplitViewDock = SplitViewDock;

        // Create root dock
        var rootDock = new RootDock
        {
            Id = "Root",
            Title = "Root",
            IsCollapsable = false,
            VisibleDockables = CreateList<IDockable>(SplitViewDock),
            ActiveDockable = SplitViewDock,
            DefaultDockable = SplitViewDock
        };

        return rootDock;
    }
}
