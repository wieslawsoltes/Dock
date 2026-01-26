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

public class ExcelWorkspaceDockFactory : Factory
{
    private readonly IScreen _host;

    public ExcelWorkspaceDockFactory(IScreen host)
    {
        _host = host;
    }

    public IReadOnlyList<ExcelDocumentViewModel> Documents { get; private set; } = Array.Empty<ExcelDocumentViewModel>();
    public OfficeInspectorToolViewModel InspectorTool { get; private set; } = null!;

    public override IRootDock CreateLayout()
    {
        var ribbon = new OfficeRibbonToolViewModel(
            OfficeAppKind.Excel,
            OfficeSampleData.GetRibbonTabs(OfficeAppKind.Excel),
            OfficeSampleData.GetRibbonGroups(OfficeAppKind.Excel))
        {
            Id = "ExcelRibbon",
            Title = "Ribbon"
        };

        var sheets = new OfficeToolPanelViewModel(
            OfficeAppKind.Excel,
            "Sheets",
            OfficeSampleData.ExcelSheets)
        {
            Id = "ExcelSheets",
            Title = "Sheets"
        };

        var insights = new OfficeToolPanelViewModel(
            OfficeAppKind.Excel,
            "Insights",
            OfficeSampleData.ExcelInsights)
        {
            Id = "ExcelInsights",
            Title = "Insights"
        };

        InspectorTool = new OfficeInspectorToolViewModel(_host, OfficeAppKind.Excel)
        {
            Id = "ExcelInspector",
            Title = "Formulas"
        };

        InspectorTool.InitializeSections(new[]
        {
            new InspectorSectionViewModel(
                host: InspectorTool,
                appKind: OfficeAppKind.Excel,
                urlSegment: "formulas",
                sectionTitle: "Formulas",
                description: "Build reliable calculations and reuse logic.",
                items: OfficeSampleData.ExcelFormulas),
            new InspectorSectionViewModel(
                host: InspectorTool,
                appKind: OfficeAppKind.Excel,
                urlSegment: "data",
                sectionTitle: "Data Tools",
                description: "Clean, validate, and manage datasets.",
                items: OfficeSampleData.ExcelDataTools),
            new InspectorSectionViewModel(
                host: InspectorTool,
                appKind: OfficeAppKind.Excel,
                urlSegment: "charts",
                sectionTitle: "Charts",
                description: "Turn numbers into clear visuals.",
                items: OfficeSampleData.ExcelCharts)
        });

        var document1 = new ExcelDocumentViewModel(_host, "Q3Forecast.xlsx")
        {
            Id = "ExcelDoc1",
            Title = "Q3Forecast.xlsx"
        };

        var document2 = new ExcelDocumentViewModel(_host, "ScenarioModel.xlsx")
        {
            Id = "ExcelDoc2",
            Title = "ScenarioModel.xlsx",
            CanClose = true
        };

        Documents = new[] { document1, document2 };

        var documentDock = new DocumentDock
        {
            Id = "ExcelDocuments",
            VisibleDockables = CreateList<IDockable>(document1, document2),
            ActiveDockable = document1,
            CanCreateDocument = false
        };

        var topDock = new ToolDock
        {
            Id = "ExcelRibbonDock",
            Alignment = Alignment.Top,
            Proportion = 0.18,
            VisibleDockables = CreateList<IDockable>(ribbon),
            ActiveDockable = ribbon,
            CanDrop = false
        };

        var leftDock = new ToolDock
        {
            Id = "ExcelSheetsDock",
            Alignment = Alignment.Left,
            Proportion = 0.22,
            VisibleDockables = CreateList<IDockable>(sheets),
            ActiveDockable = sheets
        };

        var rightDock = new ToolDock
        {
            Id = "ExcelInspectorDock",
            Alignment = Alignment.Right,
            Proportion = 0.24,
            VisibleDockables = CreateList<IDockable>(InspectorTool),
            ActiveDockable = InspectorTool
        };

        var bottomDock = new ToolDock
        {
            Id = "ExcelInsightsDock",
            Alignment = Alignment.Bottom,
            Proportion = 0.2,
            VisibleDockables = CreateList<IDockable>(insights),
            ActiveDockable = insights
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

        var nameBox = new OfficeToolPanelViewModel(
            OfficeAppKind.Excel,
            "Name Box",
            new[] { "Define Name", "Name Manager", "Selection" })
        {
            Id = "ExcelNameBox",
            Title = "Name Box",
            CanPin = true,
            KeepPinnedDockableVisible = true
        };

        var quickAnalysis = new OfficeToolPanelViewModel(
            OfficeAppKind.Excel,
            "Quick Analysis",
            new[] { "Totals", "Charts", "Sparklines" })
        {
            Id = "ExcelQuickAnalysis",
            Title = "Quick Analysis",
            CanPin = true
        };

        root.LeftPinnedDockables = CreateList<IDockable>(nameBox);
        root.RightPinnedDockables = CreateList<IDockable>(quickAnalysis);
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
