using System;
using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Controls;
using Dock.Model.ReactiveUI.Navigation.Controls;
using Dock.Settings;
using DockOfficeSample.Models;
using DockOfficeSample.ViewModels.Documents;
using DockOfficeSample.ViewModels.Tools;
using ReactiveUI;

namespace DockOfficeSample.ViewModels.Workspaces;

public class PowerPointWorkspaceDockFactory : Factory
{
    private readonly IScreen _host;

    public PowerPointWorkspaceDockFactory(IScreen host)
    {
        _host = host;
    }

    public IReadOnlyList<PowerPointDocumentViewModel> Documents { get; private set; } = Array.Empty<PowerPointDocumentViewModel>();
    public OfficeInspectorToolViewModel InspectorTool { get; private set; } = null!;

    public override IRootDock CreateLayout()
    {
        var ribbon = new OfficeRibbonToolViewModel(
            OfficeAppKind.PowerPoint,
            OfficeSampleData.GetRibbonTabs(OfficeAppKind.PowerPoint),
            OfficeSampleData.GetRibbonGroups(OfficeAppKind.PowerPoint))
        {
            Id = "PowerPointRibbon",
            Title = "Ribbon"
        };

        var slides = new OfficeToolPanelViewModel(
            OfficeAppKind.PowerPoint,
            "Slides",
            OfficeSampleData.PowerPointSlides)
        {
            Id = "PowerPointSlides",
            Title = "Slides"
        };

        var notes = new OfficeToolPanelViewModel(
            OfficeAppKind.PowerPoint,
            "Speaker Notes",
            OfficeSampleData.PowerPointNotes)
        {
            Id = "PowerPointNotes",
            Title = "Notes"
        };

        InspectorTool = new OfficeInspectorToolViewModel(_host, OfficeAppKind.PowerPoint)
        {
            Id = "PowerPointInspector",
            Title = "Design"
        };

        InspectorTool.InitializeSections(new[]
        {
            new InspectorSectionViewModel(
                host: InspectorTool,
                appKind: OfficeAppKind.PowerPoint,
                urlSegment: "design",
                sectionTitle: "Design",
                description: "Themes, colors, and layout controls.",
                items: OfficeSampleData.PowerPointDesign),
            new InspectorSectionViewModel(
                host: InspectorTool,
                appKind: OfficeAppKind.PowerPoint,
                urlSegment: "animations",
                sectionTitle: "Animations",
                description: "Bring elements to life with motion.",
                items: OfficeSampleData.PowerPointAnimations),
            new InspectorSectionViewModel(
                host: InspectorTool,
                appKind: OfficeAppKind.PowerPoint,
                urlSegment: "transitions",
                sectionTitle: "Transitions",
                description: "Control slide-to-slide flow.",
                items: OfficeSampleData.PowerPointTransitions)
        });

        var document1 = new PowerPointDocumentViewModel(_host, "BoardDeck.pptx")
        {
            Id = "PowerPointDoc1",
            Title = "BoardDeck.pptx"
        };

        var document2 = new PowerPointDocumentViewModel(_host, "PitchUpdate.pptx")
        {
            Id = "PowerPointDoc2",
            Title = "PitchUpdate.pptx",
            CanClose = true
        };

        Documents = new[] { document1, document2 };

        var documentDock = new DocumentDock
        {
            Id = "PowerPointDocuments",
            VisibleDockables = CreateList<IDockable>(document1, document2),
            ActiveDockable = document1,
            CanCreateDocument = false
        };

        var topDock = new ToolDock
        {
            Id = "PowerPointRibbonDock",
            Alignment = Alignment.Top,
            Proportion = 0.18,
            VisibleDockables = CreateList<IDockable>(ribbon),
            ActiveDockable = ribbon,
            CanDrop = false
        };

        var leftDock = new ToolDock
        {
            Id = "PowerPointSlidesDock",
            Alignment = Alignment.Left,
            Proportion = 0.2,
            VisibleDockables = CreateList<IDockable>(slides),
            ActiveDockable = slides
        };

        var rightDock = new ToolDock
        {
            Id = "PowerPointInspectorDock",
            Alignment = Alignment.Right,
            Proportion = 0.24,
            VisibleDockables = CreateList<IDockable>(InspectorTool),
            ActiveDockable = InspectorTool
        };

        var bottomDock = new ToolDock
        {
            Id = "PowerPointNotesDock",
            Alignment = Alignment.Bottom,
            Proportion = 0.22,
            VisibleDockables = CreateList<IDockable>(notes),
            ActiveDockable = notes
        };

        var mainRow = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>(
                leftDock,
                new ProportionalDockSplitter(),
                documentDock,
                new ProportionalDockSplitter(),
                rightDock),
            ActiveDockable = documentDock
        };

        var mainLayout = new ProportionalDock
        {
            Orientation = Orientation.Vertical,
            VisibleDockables = CreateList<IDockable>(
                topDock,
                new ProportionalDockSplitter(),
                mainRow,
                new ProportionalDockSplitter(),
                bottomDock),
            ActiveDockable = mainRow
        };

        var root = new RoutableRootDock(_host)
        {
            VisibleDockables = CreateList<IDockable>(mainLayout),
            DefaultDockable = mainLayout,
            ActiveDockable = mainLayout
        };

        var presenterTools = new OfficeToolPanelViewModel(
            OfficeAppKind.PowerPoint,
            "Presenter Tools",
            new[] { "Laser Pointer", "Pen", "Live Captions" })
        {
            Id = "PowerPointPresenterTools",
            Title = "Presenter",
            CanPin = true,
            KeepPinnedDockableVisible = true
        };

        var animationPane = new OfficeToolPanelViewModel(
            OfficeAppKind.PowerPoint,
            "Animation Pane",
            new[] { "Entrance", "Emphasis", "Exit" })
        {
            Id = "PowerPointAnimationPane",
            Title = "Animation Pane",
            CanPin = true
        };

        root.LeftPinnedDockables = CreateList<IDockable>(presenterTools);
        root.RightPinnedDockables = CreateList<IDockable>(animationPane);
        root.TopPinnedDockables = CreateList<IDockable>();
        root.BottomPinnedDockables = CreateList<IDockable>();
        root.PinnedDock = null;

        return root;
    }

    public override void InitLayout(IDockable layout)
    {
        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () => DockSettings.UseManagedWindows ? new ManagedHostWindow() : new HostWindow()
        };

        base.InitLayout(layout);
    }
}
