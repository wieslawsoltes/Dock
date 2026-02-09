using Avalonia.Headless.XUnit;
using Avalonia.Collections;
using System;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class FactoryTests
{
    [AvaloniaFact]
    public void TestFactory_Ctor()
    {
        var actual = new TestFactory();
        Assert.NotNull(actual);
    }

    [AvaloniaFact]
    public void CreateList_Creates_AvaloniaList_Empty()
    {
        var factory = new TestFactory();
        var actual = factory.CreateList<IDockable>();
        Assert.NotNull(actual);
        Assert.IsType<AvaloniaList<IDockable>>(actual);
        Assert.Empty(actual);
    }

    [AvaloniaFact]
    public void CreateRootDock_Creates_RootDock_Type()
    {
        var factory = new TestFactory();
        var actual = factory.CreateRootDock();
        Assert.NotNull(actual);
        Assert.IsType<RootDock>(actual);
    }

    [AvaloniaFact]
    public void CreateProportionalDock_Creates_ProportionalDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateProportionalDock();
        Assert.NotNull(actual);
        Assert.IsType<ProportionalDock>(actual);
    }

    [AvaloniaFact]
    public void CreateDockDock_Creates_DockDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateDockDock();
        Assert.NotNull(actual);
        Assert.IsType<DockDock>(actual);
        Assert.True(actual.LastChildFill);
    }

    [AvaloniaFact]
    public void CreateStackDock_Creates_StackDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateStackDock();
        Assert.NotNull(actual);
        Assert.IsType<StackDock>(actual);
    }

    [AvaloniaFact]
    public void CreateGridDock_Creates_GridDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateGridDock();
        Assert.NotNull(actual);
        Assert.IsType<GridDock>(actual);
    }

    [AvaloniaFact]
    public void CreateWrapDock_Creates_WrapDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateWrapDock();
        Assert.NotNull(actual);
        Assert.IsType<WrapDock>(actual);
    }

    [AvaloniaFact]
    public void CreateUniformGridDock_Creates_UniformGridDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateUniformGridDock();
        Assert.NotNull(actual);
        Assert.IsType<UniformGridDock>(actual);
    }

    [AvaloniaFact]
    public void CreateProportionalDockSplitter_Creates_ProportionalDockSplitter()
    {
        var factory = new TestFactory();
        var actual = factory.CreateProportionalDockSplitter();
        Assert.NotNull(actual);
        Assert.IsType<ProportionalDockSplitter>(actual);
        Assert.True(actual.CanResize);
    }

    [AvaloniaFact]
    public void CreateGridDockSplitter_Creates_GridDockSplitter()
    {
        var factory = new TestFactory();
        var actual = factory.CreateGridDockSplitter();
        Assert.NotNull(actual);
        Assert.IsType<GridDockSplitter>(actual);
    }

    [AvaloniaFact]
    public void CreateToolDock_Creates_ToolDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateToolDock();
        Assert.NotNull(actual);
        Assert.IsType<ToolDock>(actual);
    }

    [AvaloniaFact]
    public void Tool_Default_Sizes_Are_NaN()
    {
        var tool = new Tool();
        Assert.True(double.IsNaN(tool.MinWidth));
        Assert.True(double.IsNaN(tool.MaxWidth));
        Assert.True(double.IsNaN(tool.MinHeight));
        Assert.True(double.IsNaN(tool.MaxHeight));
        Assert.Equal(DockingWindowState.Docked, tool.DockingState);
    }

    [AvaloniaFact]
    public void DockingState_Transitions_Cover_Docked_Pinned_Document_Floating_And_Hidden()
    {
        var factory = new TestFactory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();
        root.HiddenDockables = factory.CreateList<IDockable>();
        root.Windows = factory.CreateList<IDockWindow>();

        var toolDock = factory.CreateToolDock();
        toolDock.VisibleDockables = factory.CreateList<IDockable>();

        var documentDock = factory.CreateDocumentDock();
        documentDock.VisibleDockables = factory.CreateList<IDockable>();

        factory.AddDockable(root, toolDock);
        factory.AddDockable(root, documentDock);

        var tool = factory.CreateTool();
        factory.AddDockable(toolDock, tool);

        var document = factory.CreateDocument();
        factory.AddDockable(documentDock, document);

        Assert.Equal(DockingWindowState.Docked, tool.DockingState);
        Assert.Equal(DockingWindowState.Document, document.DockingState);

        factory.DockAsDocument(tool);
        Assert.Equal(DockingWindowState.Document, tool.DockingState);

        factory.MoveDockable(documentDock, toolDock, tool, null);
        Assert.Equal(DockingWindowState.Docked, tool.DockingState);

        factory.PinDockable(tool);
        Assert.Equal(DockingWindowState.Pinned, tool.DockingState);

        factory.UnpinDockable(tool);
        Assert.Equal(DockingWindowState.Docked, tool.DockingState);

        factory.FloatDockable(tool);
        Assert.Equal(DockingWindowState.Docked | DockingWindowState.Floating, tool.DockingState);

        factory.HideDockable(tool);
        Assert.Equal(
            DockingWindowState.Docked | DockingWindowState.Floating | DockingWindowState.Hidden,
            tool.DockingState);

        factory.RestoreDockable(tool);
        Assert.Equal(DockingWindowState.Docked | DockingWindowState.Floating, tool.DockingState);

        factory.PinDockable(tool);
        Assert.Equal(DockingWindowState.Pinned | DockingWindowState.Floating, tool.DockingState);

        factory.UnpinDockable(tool);
        Assert.Equal(DockingWindowState.Docked | DockingWindowState.Floating, tool.DockingState);

        factory.FloatDockable(document);
        Assert.Equal(DockingWindowState.Document | DockingWindowState.Floating, document.DockingState);

        factory.HideDockable(document);
        Assert.Equal(
            DockingWindowState.Document | DockingWindowState.Floating | DockingWindowState.Hidden,
            document.DockingState);

        factory.RestoreDockable(document);
        Assert.Equal(DockingWindowState.Document | DockingWindowState.Floating, document.DockingState);
    }


    [AvaloniaFact]
    public void DockingState_HiddenContainer_Propagates_To_Descendants()
    {
        var factory = new TestFactory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();
        root.HiddenDockables = factory.CreateList<IDockable>();
        root.Windows = factory.CreateList<IDockWindow>();

        var documentDock = factory.CreateDocumentDock();
        documentDock.VisibleDockables = factory.CreateList<IDockable>();

        var document = factory.CreateDocument();

        factory.AddDockable(root, documentDock);
        factory.AddDockable(documentDock, document);

        factory.HideDockable(documentDock);

        Assert.Equal(DockingWindowState.Document | DockingWindowState.Hidden, documentDock.DockingState);
        Assert.Equal(DockingWindowState.Document | DockingWindowState.Hidden, document.DockingState);

        factory.RestoreDockable(documentDock);

        Assert.Equal(DockingWindowState.Document, documentDock.DockingState);
        Assert.Equal(DockingWindowState.Document, document.DockingState);

        factory.FloatDockable(documentDock);

        Assert.Equal(DockingWindowState.Document | DockingWindowState.Floating, documentDock.DockingState);
        Assert.Equal(DockingWindowState.Document | DockingWindowState.Floating, document.DockingState);

        factory.HideDockable(documentDock);

        Assert.Equal(
            DockingWindowState.Document | DockingWindowState.Floating | DockingWindowState.Hidden,
            documentDock.DockingState);
        Assert.Equal(
            DockingWindowState.Document | DockingWindowState.Floating | DockingWindowState.Hidden,
            document.DockingState);

        factory.RestoreDockable(documentDock);

        Assert.Equal(DockingWindowState.Document | DockingWindowState.Floating, documentDock.DockingState);
        Assert.Equal(DockingWindowState.Document | DockingWindowState.Floating, document.DockingState);
    }

    [AvaloniaFact]
    public void CreateDocumentDock_Creates_DocumentDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateDocumentDock();
        Assert.NotNull(actual);
        Assert.IsType<DocumentDock>(actual);
    }

    [AvaloniaFact]
    public void CreateDockWindow_Creates_DockWindow()
    {
        var factory = new TestFactory();
        var actual = factory.CreateDockWindow();
        Assert.NotNull(actual);
        Assert.IsType<DockWindow>(actual);
    }

    [AvaloniaFact]
    public void CreateLayout_Creates_RootDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateLayout();
        Assert.NotNull(actual);
        Assert.IsType<RootDock>(actual);
    }
}

public class TestFactory : Factory
{
}
