using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests.Controls;

public class MixedDockSiteItemsSourceRegressionTests
{
    private sealed class TestDocumentItem
    {
        public string Title { get; set; } = string.Empty;

        public bool CanClose { get; set; } = true;
    }

    private sealed class TestToolItem
    {
        public string Title { get; set; } = string.Empty;

        public bool CanClose { get; set; } = true;
    }

    private sealed class MixedDockSiteContext
    {
        public required Factory Factory { get; init; }

        public required IRootDock Root { get; init; }

        public required DocumentDock DocumentDock { get; init; }

        public required ToolDock ToolDock { get; init; }

        public required Document ManualDocument { get; init; }

        public required Tool ManualTool { get; init; }

        public required ObservableCollection<TestDocumentItem> DocumentItems { get; init; }

        public required ObservableCollection<TestToolItem> ToolItems { get; init; }

        public required TestDocumentItem GeneratedDocumentItem { get; init; }

        public required TestToolItem GeneratedToolItem { get; init; }

        public required Document GeneratedDocument { get; init; }

        public required Tool GeneratedTool { get; init; }
    }

    private static IList<IDockable> RequireVisibleDockables(DocumentDock dock) =>
        dock.VisibleDockables ?? throw new InvalidOperationException("VisibleDockables should not be null.");

    private static IList<IDockable> RequireVisibleDockables(ToolDock dock) =>
        dock.VisibleDockables ?? throw new InvalidOperationException("VisibleDockables should not be null.");

    private static IList<IDockable> RequireHiddenDockables(IRootDock root) =>
        root.HiddenDockables ?? throw new InvalidOperationException("HiddenDockables should not be null.");

    private static MixedDockSiteContext CreateMixedDockSite()
    {
        var factory = new Factory
        {
            HideDocumentsOnClose = true,
            HideToolsOnClose = true
        };

        var root = factory.CreateRootDock();
        root.Id = "Root";
        root.VisibleDockables = factory.CreateList<IDockable>();
        root.HiddenDockables = factory.CreateList<IDockable>();
        root.Windows = factory.CreateList<IDockWindow>();

        var documentDock = new DocumentDock
        {
            Id = "Documents",
            VisibleDockables = factory.CreateList<IDockable>(),
            DocumentTemplate = new DocumentTemplate(),
            Factory = factory
        };

        var toolDock = new ToolDock
        {
            Id = "Tools",
            VisibleDockables = factory.CreateList<IDockable>(),
            ToolTemplate = new ToolTemplate(),
            Factory = factory
        };

        factory.AddDockable(root, documentDock);
        factory.AddDockable(root, toolDock);
        root.ActiveDockable = documentDock;

        var manualDocument = new Document
        {
            Id = "ManualDocument",
            Title = "Manual Document",
            CanClose = true,
            Content = "Static document content"
        };

        var manualTool = new Tool
        {
            Id = "ManualTool",
            Title = "Manual Tool",
            CanClose = true,
            Content = "Static tool content"
        };

        factory.AddDockable(documentDock, manualDocument);
        factory.AddDockable(toolDock, manualTool);

        factory.InitLayout(root);

        var generatedDocumentItem = new TestDocumentItem { Title = "Generated Document", CanClose = true };
        var generatedToolItem = new TestToolItem { Title = "Generated Tool", CanClose = true };
        var documentItems = new ObservableCollection<TestDocumentItem> { generatedDocumentItem };
        var toolItems = new ObservableCollection<TestToolItem> { generatedToolItem };

        documentDock.ItemsSource = documentItems;
        toolDock.ItemsSource = toolItems;

        var generatedDocument = Assert.IsType<Document>(factory.GetContainerFromItem(generatedDocumentItem));
        var generatedTool = Assert.IsType<Tool>(factory.GetContainerFromItem(generatedToolItem));

        return new MixedDockSiteContext
        {
            Factory = factory,
            Root = root,
            DocumentDock = documentDock,
            ToolDock = toolDock,
            ManualDocument = manualDocument,
            ManualTool = manualTool,
            DocumentItems = documentItems,
            ToolItems = toolItems,
            GeneratedDocumentItem = generatedDocumentItem,
            GeneratedToolItem = generatedToolItem,
            GeneratedDocument = generatedDocument,
            GeneratedTool = generatedTool
        };
    }

    [AvaloniaFact]
    public void MixedDockSite_CloseSemantics_DistinguishesGeneratedAndStaticDockables()
    {
        var context = CreateMixedDockSite();

        context.Factory.CloseDockable(context.GeneratedDocument);
        context.Factory.CloseDockable(context.GeneratedTool);

        Assert.Empty(context.DocumentItems);
        Assert.Empty(context.ToolItems);
        Assert.Null(context.Factory.GetContainerFromItem(context.GeneratedDocumentItem));
        Assert.Null(context.Factory.GetContainerFromItem(context.GeneratedToolItem));
        Assert.Empty(RequireHiddenDockables(context.Root));

        context.Factory.CloseDockable(context.ManualDocument);
        context.Factory.CloseDockable(context.ManualTool);

        Assert.Empty(context.DocumentItems);
        Assert.Empty(context.ToolItems);

        var hiddenDockables = RequireHiddenDockables(context.Root);
        Assert.Equal(2, hiddenDockables.Count);
        Assert.Contains(context.ManualDocument, hiddenDockables);
        Assert.Contains(context.ManualTool, hiddenDockables);
        Assert.DoesNotContain(context.GeneratedDocument, hiddenDockables);
        Assert.DoesNotContain(context.GeneratedTool, hiddenDockables);

        Assert.DoesNotContain(context.ManualDocument, RequireVisibleDockables(context.DocumentDock));
        Assert.DoesNotContain(context.ManualTool, RequireVisibleDockables(context.ToolDock));
    }

    [AvaloniaFact]
    public void MixedDockSite_RestoreSemantics_RestoresOnlyHiddenStaticDockables()
    {
        var context = CreateMixedDockSite();

        context.Factory.CloseDockable(context.GeneratedDocument);
        context.Factory.CloseDockable(context.GeneratedTool);
        context.Factory.CloseDockable(context.ManualDocument);
        context.Factory.CloseDockable(context.ManualTool);

        var restoreGeneratedDocumentException = Record.Exception(() => context.Factory.RestoreDockable(context.GeneratedDocument));
        var restoreGeneratedToolException = Record.Exception(() => context.Factory.RestoreDockable(context.GeneratedTool));

        Assert.Null(restoreGeneratedDocumentException);
        Assert.Null(restoreGeneratedToolException);
        Assert.Equal(2, RequireHiddenDockables(context.Root).Count);

        context.Factory.RestoreDockable(context.ManualDocument);
        context.Factory.RestoreDockable(context.ManualTool);

        Assert.Empty(RequireHiddenDockables(context.Root));
        Assert.Contains(context.ManualDocument, RequireVisibleDockables(context.DocumentDock));
        Assert.Contains(context.ManualTool, RequireVisibleDockables(context.ToolDock));

        Assert.Empty(context.DocumentItems);
        Assert.Empty(context.ToolItems);
        Assert.Null(context.Factory.GetContainerFromItem(context.GeneratedDocumentItem));
        Assert.Null(context.Factory.GetContainerFromItem(context.GeneratedToolItem));
    }
}
