using System;
using System.Collections.Generic;
using System.Linq;
using Dock.Avalonia.Controls;
using DockOverlayReactiveUISample.Models;
using DockOverlayReactiveUISample.ViewModels.Documents;
using DockOverlayReactiveUISample.ViewModels.Tools;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Controls;
using ModelOrientation = Dock.Model.Core.Orientation;

namespace DockOverlayReactiveUISample.ViewModels;

public class OverlayDockFactory : Factory
{
    private readonly Scenario _scenario;
    private readonly object _context;
    private IRootDock? _rootDock;
    private IOverlayDock? _overlayDock;

    public OverlayDockFactory(Scenario scenario, object context)
    {
        _scenario = scenario;
        _context = context;
    }

    public override IRootDock CreateLayout()
    {
        var overlay = CreateOverlayDock();
        overlay.Id = "OverlayDock";
        overlay.Title = "Overlay Workspace";
        overlay.AllowPanelDragging = true;
        overlay.AllowPanelResizing = true;
        overlay.EnableSnapToEdge = true;
        overlay.EnableSnapToPanel = true;

        switch (_scenario)
        {
            case Scenario.VideoEditor:
                BuildVideoEditor(overlay);
                break;
            case Scenario.GameLevelEditor:
                BuildGameLevelEditor(overlay);
                break;
            case Scenario.Dashboard:
                BuildDashboard(overlay);
                break;
            case Scenario.Interop:
                BuildInterop(overlay);
                break;
            default:
                BuildDashboard(overlay);
                break;
        }

        // Use the background as the default target so initialization selects it.
        overlay.DefaultDockable ??= overlay.BackgroundDockable;

        // Ensure a sensible active dockable so the view has something selected.
        if (overlay.ActiveDockable is null)
        {
            overlay.ActiveDockable = overlay.BackgroundDockable ?? overlay.OverlayPanels?.FirstOrDefault();
        }

        var root = CreateRootDock();
        root.Id = "Root";
        root.Title = "Overlay Dock Root";
        root.ActiveDockable = overlay;
        root.DefaultDockable = overlay;
        root.IsCollapsable = false;
        root.VisibleDockables = CreateList<IDockable>(overlay);
        root.LeftPinnedDockables = CreateList<IDockable>();
        root.RightPinnedDockables = CreateList<IDockable>();
        root.TopPinnedDockables = CreateList<IDockable>();
        root.BottomPinnedDockables = CreateList<IDockable>();
        root.PinnedDock = null;

        _rootDock = root;
        _overlayDock = overlay;

        return root;
    }

    public override void InitLayout(IDockable layout)
    {
        var contextLocator = new Dictionary<string, Func<object?>>();

        if (_rootDock is not null)
        {
            CollectContexts(_rootDock, contextLocator);
        }

        ContextLocator = contextLocator;

        DockableLocator = new Dictionary<string, Func<IDockable?>>
        {
            ["Root"] = () => _rootDock,
            ["OverlayDock"] = () => _overlayDock
        };

        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () => new HostWindow()
        };

        base.InitLayout(layout);
    }

    private void CollectContexts(IDockable dockable, IDictionary<string, Func<object?>> contextLocator)
    {
        if (!string.IsNullOrWhiteSpace(dockable.Id))
        {
            var context = dockable.Context ?? dockable.Title ?? _context;
            contextLocator[dockable.Id] = () => context;
        }

        if (dockable is IDock dock && dock.VisibleDockables is { })
        {
            foreach (var child in dock.VisibleDockables)
            {
                if (child is null)
                {
                    continue;
                }

                CollectContexts(child, contextLocator);
            }
        }
    }

    private OverlayDocumentViewModel CreateDocument(string id, string title, string context, string? dockGroup = null)
    {
        var document = new OverlayDocumentViewModel
        {
            Id = id,
            Title = title,
            Context = null,
            CanClose = false
        };

        document.Context = document;

        if (!string.IsNullOrWhiteSpace(dockGroup))
        {
            document.DockGroup = dockGroup;
        }

        return document;
    }

    private OverlayToolViewModel CreateTool(string id, string title, string context, string? dockGroup = null)
    {
        var tool = new OverlayToolViewModel
        {
            Id = id,
            Title = title,
            Context = null,
            CanClose = false
        };

        tool.Context = tool;

        if (!string.IsNullOrWhiteSpace(dockGroup))
        {
            tool.DockGroup = dockGroup;
        }

        return tool;
    }

    private IDock CreateDocumentDock(string id, string title, OverlayDocumentViewModel active, params OverlayDocumentViewModel[] documents)
    {
        var list = CreateList<IDockable>();
        foreach (var document in documents)
        {
            list.Add(document);
        }

        var documentDock = new DocumentDock
        {
            Id = id,
            Title = title,
            ActiveDockable = active,
            VisibleDockables = list,
            CanCreateDocument = false,
            IsCollapsable = false
        };

        return documentDock;
    }

    private IDock CreateToolDock(string id, string title, params OverlayToolViewModel[] tools)
    {
        var list = CreateList<IDockable>();
        foreach (var tool in tools)
        {
            list.Add(tool);
        }

        var toolDock = new ToolDock
        {
            Id = id,
            Title = title,
            ActiveDockable = tools.Length > 0 ? tools[0] : null,
            VisibleDockables = list,
            CanClose = false
        };

        return toolDock;
    }

    private IOverlayPanel CreatePanel(
        string id,
        string title,
        double x,
        double y,
        double width,
        double height,
        IDockable content,
        string? dockGroup = null,
        bool allowDockInto = true,
        int zIndex = 0,
        OverlayAnchor anchor = OverlayAnchor.None)
    {
        var panel = CreateOverlayPanel();
        panel.Id = id;
        panel.Title = title;
        panel.X = x;
        panel.Y = y;
        panel.Width = width;
        panel.Height = height;
        panel.ZIndex = zIndex;
        panel.Anchor = anchor;
        panel.AllowDockInto = allowDockInto;
        panel.Content = content;

        if (!string.IsNullOrWhiteSpace(dockGroup))
        {
            ApplyDockGroup(panel, dockGroup);
        }

        return panel;
    }

    private void ApplyDockGroup(IDockable dockable, string dockGroup)
    {
        dockable.DockGroup = dockGroup;

        if (dockable is IDock dock && dock.VisibleDockables is { })
        {
            foreach (var child in dock.VisibleDockables)
            {
                if (child is null)
                {
                    continue;
                }

                ApplyDockGroup(child, dockGroup);
            }
        }
    }

    private void BuildVideoEditor(IOverlayDock overlay)
    {
        var preview = CreateDocument("VideoPreview", "Video Preview", "Full-size video preview");
        var background = CreateDocumentDock("VideoBackgroundDock", "Preview", preview, preview);
        overlay.BackgroundDockable = background;

        var mediaBin = CreatePanel(
            id: "MediaBinPanel",
            title: "Media Bin",
            x: 32,
            y: 64,
            width: 320,
            height: 420,
            content: CreateToolDock(
                "MediaBinDock",
                "Media",
                CreateTool("MediaClips", "Clips", "Footage and images"),
                CreateTool("MediaEffects", "Effects", "Transitions and filters")));

        var inspector = CreatePanel(
            id: "InspectorPanel",
            title: "Inspector",
            x: 960,
            y: 64,
            width: 320,
            height: 420,
            content: CreateToolDock(
                "InspectorDock",
                "Inspector",
                CreateTool("ClipInspector", "Clip", "Metadata and codecs"),
                CreateTool("ColorInspector", "Color", "Color grading controls")));

        var timelineDocument = CreateDocument("TimelineDocument", "Timeline", "Sequence and tracks");
        var timelineDock = CreateDocumentDock("TimelineDock", "Timeline", timelineDocument, timelineDocument);
        var mixerDock = CreateToolDock(
            "MixerDock",
            "Mixer",
            CreateTool("AudioMixer", "Audio Mixer", "Bus and track levels"),
            CreateTool("EffectsChain", "FX Chain", "Track effects"));

        var timelinePanel = CreatePanel(
            id: "TimelinePanel",
            title: "Timeline",
            x: 0,
            y: 0,
            width: 640,
            height: 220,
            content: timelineDock,
            dockGroup: null,
            allowDockInto: true,
            zIndex: 0,
            anchor: OverlayAnchor.None);
        timelinePanel.Proportion = 0.67;

        var mixerPanel = CreatePanel(
            id: "MixerPanel",
            title: "Mixer",
            x: 0,
            y: 0,
            width: 320,
            height: 220,
            content: mixerDock,
            dockGroup: null,
            allowDockInto: true,
            zIndex: 0,
            anchor: OverlayAnchor.None);
        mixerPanel.Proportion = 0.33;

        var timelineSplitter = CreateOverlaySplitter();
        timelineSplitter.Id = "TimelineSplitter";
        timelineSplitter.Title = "Timeline Splitter";
        timelineSplitter.Orientation = ModelOrientation.Vertical;
        timelineSplitter.Thickness = 8;
        timelineSplitter.CanResize = true;
        timelineSplitter.ResizePreview = true;
        timelineSplitter.MinSizeBefore = 200;
        timelineSplitter.MinSizeAfter = 160;
        timelineSplitter.PanelBefore = timelinePanel;
        timelineSplitter.PanelAfter = mixerPanel;

        var timelineGroup = CreateOverlaySplitterGroup();
        timelineGroup.Id = "TimelineGroup";
        timelineGroup.Title = "Timeline Group";
        timelineGroup.GroupTitle = "Timeline + Mixer";
        timelineGroup.Orientation = ModelOrientation.Horizontal;
        timelineGroup.X = 160;
        timelineGroup.Y = 520;
        timelineGroup.Width = 960;
        timelineGroup.Height = 240;
        timelineGroup.ZIndex = 1;
        timelineGroup.Anchor = OverlayAnchor.Bottom;
        timelineGroup.ShowGroupHeader = false;
        timelineGroup.Panels = CreateList<IOverlayPanel>(timelinePanel, mixerPanel);
        timelineGroup.Splitters = CreateList<IOverlaySplitter>(timelineSplitter);

        timelinePanel.SplitterGroup = timelineGroup;
        mixerPanel.SplitterGroup = timelineGroup;

        overlay.OverlayPanels = CreateList<IOverlayPanel>(mediaBin, inspector);
        overlay.SplitterGroups = CreateList<IOverlaySplitterGroup>(timelineGroup);
    }

    private void BuildGameLevelEditor(IOverlayDock overlay)
    {
        var viewport = CreateDocument("Viewport", "3D Viewport", "Game view and navigation");
        var background = CreateDocumentDock("ViewportDock", "Viewport", viewport, viewport);
        overlay.BackgroundDockable = background;

        var hierarchy = CreatePanel(
            id: "HierarchyPanel",
            title: "Hierarchy",
            x: 24,
            y: 40,
            width: 280,
            height: 420,
            content: CreateToolDock(
                "HierarchyDock",
                "Hierarchy",
                CreateTool("SceneObjects", "Objects", "Entities in the scene"),
                CreateTool("Prefabs", "Prefabs", "Reusable prefabs")));

        var properties = CreatePanel(
            id: "PropertiesPanel",
            title: "Properties",
            x: 1010,
            y: 40,
            width: 320,
            height: 420,
            content: CreateToolDock(
                "PropertiesDock",
                "Properties",
                CreateTool("Transform", "Transform", "Position, rotation, scale"),
                CreateTool("Rendering", "Rendering", "Materials and lighting")));

        var assets = CreatePanel(
            id: "AssetsPanel",
            title: "Asset Browser",
            x: 200,
            y: 500,
            width: 960,
            height: 220,
            content: CreateToolDock(
                "AssetsDock",
                "Assets",
                CreateTool("Models", "Models", "Meshes and FBX"),
                CreateTool("Textures", "Textures", "Images and atlases")));

        overlay.OverlayPanels = CreateList<IOverlayPanel>(hierarchy, properties, assets);
    }

    private void BuildDashboard(IOverlayDock overlay)
    {
        var dashboard = CreateDocument("Dashboard", "Dashboard", "Map + chart background");
        var background = CreateDocumentDock("DashboardDock", "Dashboard", dashboard, dashboard);
        overlay.BackgroundDockable = background;

        var cpu = CreatePanel(
            id: "CpuPanel",
            title: "CPU",
            x: 60,
            y: 60,
            width: 260,
            height: 180,
            content: CreateToolDock(
                "CpuDock",
                "CPU",
                CreateTool("CpuNow", "Live", "Current usage"),
                CreateTool("CpuTrend", "Trend", "10 min trend")),
            allowDockInto: false,
            zIndex: 2);

        var memory = CreatePanel(
            id: "MemoryPanel",
            title: "Memory",
            x: 360,
            y: 60,
            width: 260,
            height: 180,
            content: CreateToolDock(
                "MemoryDock",
                "Memory",
                CreateTool("MemoryLive", "Live", "Active set"),
                CreateTool("MemoryAlloc", "Alloc", "Allocations")),
            allowDockInto: false,
            zIndex: 3);

        var network = CreatePanel(
            id: "NetworkPanel",
            title: "Network",
            x: 660,
            y: 60,
            width: 260,
            height: 180,
            content: CreateToolDock(
                "NetworkDock",
                "Network",
                CreateTool("Upstream", "Upstream", "Upload throughput"),
                CreateTool("Downstream", "Downstream", "Download throughput")),
            allowDockInto: false,
            zIndex: 4);

        var activityDock = CreateToolDock(
            "ActivityDock",
            "Activity",
            CreateTool("Feed", "Feed", "Recent events"));

        var activity = CreatePanel(
            id: "ActivityPanel",
            title: "Activity",
            x: 960,
            y: 260,
            width: 320,
            height: 260,
            content: activityDock,
            allowDockInto: true,
            zIndex: 5);

        var actions = new DashboardActionsViewModel(activity)
        {
            Id = "ActivityActions",
            Title = "Actions"
        };

        activityDock.VisibleDockables?.Add(actions);

        overlay.OverlayPanels = CreateList<IOverlayPanel>(cpu, memory, network, activity);
    }

    private void BuildInterop(IOverlayDock overlay)
    {
        var editor = CreateDocument("EditorSurface", "Editor Surface", "Shared editing surface", "editor");
        var background = CreateDocumentDock("InteropBackground", "Editor", editor, editor);
        ApplyDockGroup(background, "editor");
        overlay.BackgroundDockable = background;
        overlay.EnableGlobalDocking = false;

        var debugPanel = CreatePanel(
            id: "DebugPanel",
            title: "Debug Tools",
            x: 40,
            y: 80,
            width: 320,
            height: 360,
            content: CreateToolDock(
                "DebugDock",
                "Debug",
                CreateTool("Logs", "Logs", "Merged logs", "debug"),
                CreateTool("Profiling", "Profiling", "CPU profiling", "debug")),
            dockGroup: "debug",
            allowDockInto: true,
            zIndex: 1);

        var editorPanel = CreatePanel(
            id: "EditorPanel",
            title: "Editor Tools",
            x: 980,
            y: 80,
            width: 320,
            height: 360,
            content: CreateToolDock(
                "EditorDock",
                "Editor",
                CreateTool("HierarchyInterop", "Hierarchy", "Scoped to editor", "editor"),
                CreateTool("InspectorInterop", "Inspector", "Same group as background", "editor")),
            dockGroup: "editor",
            allowDockInto: true,
            zIndex: 2);

        var consolePanel = CreatePanel(
            id: "ConsolePanel",
            title: "Console",
            x: 300,
            y: 520,
            width: 760,
            height: 220,
            content: CreateToolDock(
                "ConsoleDock",
                "Console",
                CreateTool("Routing", "Routing", "Global docking disabled", "debug")),
            dockGroup: "debug",
            allowDockInto: true,
            zIndex: 3,
            anchor: OverlayAnchor.Bottom);

        overlay.OverlayPanels = CreateList<IOverlayPanel>(debugPanel, editorPanel, consolePanel);
    }
}
