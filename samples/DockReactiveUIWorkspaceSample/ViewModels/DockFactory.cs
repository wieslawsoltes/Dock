using System.Collections.Generic;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Controls;

namespace DockReactiveUIWorkspaceSample.ViewModels;

public class DockFactory : Factory
{
    private int _documentIndex = 1;

    public override IRootDock CreateLayout()
    {
        var documentDock = new DocumentDock
        {
            Id = "Documents",
            Title = "Documents",
            IsCollapsable = false,
            CanCreateDocument = true,
            AllowedDropOperations = DockOperationMask.Fill
        };

        documentDock.DocumentFactory = () => CreateDocument($"Doc{_documentIndex}", "New document content.");

        var firstDocument = CreateDocument("Welcome", "This document dock only allows tabbed docking (Fill). Splits are blocked by docking restrictions.");
        var secondDocument = CreateDocument("Notes", "Use the toolbar to capture and restore workspace layouts.");

        documentDock.VisibleDockables = CreateList<IDockable>(firstDocument, secondDocument);
        documentDock.ActiveDockable = firstDocument;

        var explorerTool = CreateTool("Explorer", "Restricted to left docking or floating.");
        explorerTool.AllowedDockOperations = DockOperationMask.Left | DockOperationMask.Fill | DockOperationMask.Window;

        var outputTool = CreateTool("Output", "Restricted to bottom docking or floating.");
        outputTool.AllowedDockOperations = DockOperationMask.Bottom | DockOperationMask.Fill | DockOperationMask.Window;

        var inspectorTool = CreateTool("Inspector", "Restricted to right docking or floating.");
        inspectorTool.AllowedDockOperations = DockOperationMask.Right | DockOperationMask.Fill | DockOperationMask.Window;

        var floatOnlyTool = CreateTool("Float Only", "This tool can only be floated.");
        floatOnlyTool.AllowedDockOperations = DockOperationMask.Window;

        var leftToolDock = new ToolDock
        {
            Id = "LeftTools",
            Title = "Left Tools",
            Alignment = Alignment.Left,
            Proportion = 0.2,
            VisibleDockables = CreateList<IDockable>(explorerTool),
            ActiveDockable = explorerTool
        };

        var rightToolDock = new ToolDock
        {
            Id = "RightTools",
            Title = "Right Tools",
            Alignment = Alignment.Right,
            Proportion = 0.2,
            VisibleDockables = CreateList<IDockable>(inspectorTool),
            ActiveDockable = inspectorTool
        };

        var bottomToolDock = new ToolDock
        {
            Id = "BottomTools",
            Title = "Bottom Tools",
            Alignment = Alignment.Bottom,
            Proportion = 0.25,
            VisibleDockables = CreateList<IDockable>(outputTool, floatOnlyTool),
            ActiveDockable = outputTool
        };

        var centerDock = new ProportionalDock
        {
            Orientation = Orientation.Vertical,
            VisibleDockables = CreateList<IDockable>(
                documentDock,
                new ProportionalDockSplitter(),
                bottomToolDock)
        };

        var mainLayout = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>(
                leftToolDock,
                new ProportionalDockSplitter(),
                centerDock,
                new ProportionalDockSplitter(),
                rightToolDock)
        };

        var rootDock = CreateRootDock();
        rootDock.IsCollapsable = false;
        rootDock.ActiveDockable = mainLayout;
        rootDock.DefaultDockable = mainLayout;
        rootDock.VisibleDockables = CreateList<IDockable>(mainLayout);

        return rootDock;
    }

    private DocumentViewModel CreateDocument(string title, string body)
    {
        var index = _documentIndex++;
        return new DocumentViewModel
        {
            Id = $"doc-{index}",
            Title = title,
            Body = body
        };
    }

    private ToolViewModel CreateTool(string title, string description)
    {
        return new ToolViewModel
        {
            Id = $"tool-{title.ToLowerInvariant().Replace(' ', '-')}",
            Title = title,
            Description = description
        };
    }
}
