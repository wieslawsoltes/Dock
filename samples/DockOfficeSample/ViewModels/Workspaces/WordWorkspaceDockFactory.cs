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

public class WordWorkspaceDockFactory : Factory
{
    private readonly IScreen _host;

    public WordWorkspaceDockFactory(IScreen host)
    {
        _host = host;
    }

    public IReadOnlyList<WordDocumentViewModel> Documents { get; private set; } = Array.Empty<WordDocumentViewModel>();
    public OfficeInspectorToolViewModel InspectorTool { get; private set; } = null!;

    public override IRootDock CreateLayout()
    {
        var ribbon = new OfficeRibbonToolViewModel(
            OfficeAppKind.Word,
            OfficeSampleData.GetRibbonTabs(OfficeAppKind.Word),
            OfficeSampleData.GetRibbonGroups(OfficeAppKind.Word))
        {
            Id = "WordRibbon",
            Title = "Ribbon"
        };

        var navigation = new OfficeToolPanelViewModel(
            OfficeAppKind.Word,
            "Outline",
            OfficeSampleData.WordOutlineItems)
        {
            Id = "WordNavigation",
            Title = "Navigation"
        };

        var comments = new OfficeToolPanelViewModel(
            OfficeAppKind.Word,
            "Review Notes",
            OfficeSampleData.WordComments)
        {
            Id = "WordComments",
            Title = "Comments"
        };

        InspectorTool = new OfficeInspectorToolViewModel(_host, OfficeAppKind.Word)
        {
            Id = "WordInspector",
            Title = "Styles"
        };

        InspectorTool.InitializeSections(new[]
        {
            new InspectorSectionViewModel(
                host: InspectorTool,
                appKind: OfficeAppKind.Word,
                urlSegment: "styles",
                sectionTitle: "Styles",
                description: "Quickly apply consistent styling.",
                items: OfficeSampleData.WordStyles),
            new InspectorSectionViewModel(
                host: InspectorTool,
                appKind: OfficeAppKind.Word,
                urlSegment: "references",
                sectionTitle: "References",
                description: "Citations, captions, and references tools.",
                items: OfficeSampleData.WordReferences),
            new InspectorSectionViewModel(
                host: InspectorTool,
                appKind: OfficeAppKind.Word,
                urlSegment: "review",
                sectionTitle: "Review",
                description: "Track changes and collaborate.",
                items: OfficeSampleData.WordReview)
        });

        var document1 = new WordDocumentViewModel(_host, "LaunchPlan.docx")
        {
            Id = "WordDoc1",
            Title = "LaunchPlan.docx"
        };

        var document2 = new WordDocumentViewModel(_host, "BrandNarrative.docx")
        {
            Id = "WordDoc2",
            Title = "BrandNarrative.docx",
            CanClose = true
        };

        Documents = new[] { document1, document2 };

        var documentDock = new DocumentDock
        {
            Id = "WordDocuments",
            VisibleDockables = CreateList<IDockable>(document1, document2),
            ActiveDockable = document1,
            CanCreateDocument = false
        };

        var topDock = new ToolDock
        {
            Id = "WordRibbonDock",
            Alignment = Alignment.Top,
            Proportion = 0.18,
            VisibleDockables = CreateList<IDockable>(ribbon),
            ActiveDockable = ribbon,
            CanDrop = false
        };

        var leftDock = new ToolDock
        {
            Id = "WordNavigationDock",
            Alignment = Alignment.Left,
            Proportion = 0.2,
            VisibleDockables = CreateList<IDockable>(navigation),
            ActiveDockable = navigation
        };

        var rightDock = new ToolDock
        {
            Id = "WordInspectorDock",
            Alignment = Alignment.Right,
            Proportion = 0.24,
            VisibleDockables = CreateList<IDockable>(InspectorTool),
            ActiveDockable = InspectorTool
        };

        var bottomDock = new ToolDock
        {
            Id = "WordCommentsDock",
            Alignment = Alignment.Bottom,
            Proportion = 0.22,
            VisibleDockables = CreateList<IDockable>(comments),
            ActiveDockable = comments
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

        var clipboard = new OfficeToolPanelViewModel(
            OfficeAppKind.Word,
            "Clipboard",
            new[] { "Paste", "Cut", "Copy", "Format Painter" })
        {
            Id = "WordClipboard",
            Title = "Clipboard",
            CanPin = true,
            KeepPinnedDockableVisible = true
        };

        var research = new OfficeToolPanelViewModel(
            OfficeAppKind.Word,
            "Research",
            new[] { "Translate", "Editor", "Smart Lookup" })
        {
            Id = "WordResearch",
            Title = "Research",
            CanPin = true
        };

        root.LeftPinnedDockables = CreateList<IDockable>(clipboard);
        root.RightPinnedDockables = CreateList<IDockable>(research);
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
