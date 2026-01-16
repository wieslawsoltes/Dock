using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

/// <summary>
/// Integration tests covering overlay dock targets and docking operations in DockManager/DockService.
/// </summary>
public class DockManagerOverlayIntegrationTests
{
    private static Factory CreateFactory()
    {
        return new Factory();
    }

    private static (Factory factory, DockService service, DockManager manager) CreateSystem()
    {
        var factory = CreateFactory();
        var service = new DockService();
        var manager = new DockManager(service);
        return (factory, service, manager);
    }

    [Fact]
    public void ValidateTool_DockIntoOverlayPanel_CreatesNestedDockAndMovesTool()
    {
        var (factory, _, manager) = CreateSystem();

        var sourceOwner = factory.CreateToolDock();
        sourceOwner.Factory = factory;
        sourceOwner.VisibleDockables = factory.CreateList<IDockable>();

        var tool = (ITool)factory.CreateTool();
        tool.Factory = factory;
        tool.DockGroup = "group-A";
        tool.Owner = sourceOwner;
        sourceOwner.VisibleDockables.Add(tool);

        var overlayPanel = factory.CreateOverlayPanel();
        overlayPanel.Factory = factory;
        overlayPanel.AllowDockInto = true;

        var result = manager.ValidateTool(tool, overlayPanel, DragAction.Move, DockOperation.Fill, bExecute: true);

        Assert.True(result);
        Assert.NotNull(overlayPanel.NestedDock);
        Assert.Equal("group-A", overlayPanel.NestedDock!.DockGroup);
        Assert.Equal(overlayPanel, overlayPanel.NestedDock!.Owner);
        Assert.Contains(tool, overlayPanel.NestedDock!.VisibleDockables!);
        Assert.Equal(overlayPanel.NestedDock, tool.Owner);
    }

    [Fact]
    public void ValidateTool_DockIntoOverlayPanel_RejectsMismatchedPanelGroup()
    {
        var (factory, _, manager) = CreateSystem();

        var sourceOwner = factory.CreateToolDock();
        sourceOwner.Factory = factory;
        sourceOwner.VisibleDockables = factory.CreateList<IDockable>();

        var tool = (ITool)factory.CreateTool();
        tool.Factory = factory;
        tool.DockGroup = "group-B";
        tool.Owner = sourceOwner;
        sourceOwner.VisibleDockables.Add(tool);

        var overlayPanel = factory.CreateOverlayPanel();
        overlayPanel.Factory = factory;
        overlayPanel.AllowDockInto = true;
        overlayPanel.DockGroup = "group-A";

        var result = manager.ValidateTool(tool, overlayPanel, DragAction.Move, DockOperation.Fill, bExecute: true);

        Assert.False(result);
    }

    [Fact]
    public void ValidateTool_DockIntoOverlayDock_AllowsWhenGlobalDockingDisabled()
    {
        var (factory, _, manager) = CreateSystem();

        var sourceOwner = factory.CreateToolDock();
        sourceOwner.Factory = factory;
        sourceOwner.VisibleDockables = factory.CreateList<IDockable>();

        var tool = (ITool)factory.CreateTool();
        tool.Factory = factory;
        tool.Owner = sourceOwner;
        sourceOwner.VisibleDockables.Add(tool);

        var overlayDock = factory.CreateOverlayDock();
        overlayDock.Factory = factory;
        overlayDock.EnableGlobalDocking = false;

        var result = manager.ValidateTool(tool, overlayDock, DragAction.Move, DockOperation.Fill, bExecute: true);

        Assert.True(result);
        Assert.NotNull(overlayDock.OverlayPanels);
    }

    [Fact]
    public void ValidateTool_DockIntoOverlayDock_RejectsCopyAndLink()
    {
        var (factory, _, manager) = CreateSystem();

        var sourceOwner = factory.CreateToolDock();
        sourceOwner.Factory = factory;
        sourceOwner.VisibleDockables = factory.CreateList<IDockable>();

        var tool = (ITool)factory.CreateTool();
        tool.Factory = factory;
        tool.Owner = sourceOwner;
        sourceOwner.VisibleDockables.Add(tool);

        var overlayDock = factory.CreateOverlayDock();
        overlayDock.Factory = factory;

        var copyResult = manager.ValidateTool(tool, overlayDock, DragAction.Copy, DockOperation.Fill, bExecute: true);
        var linkResult = manager.ValidateTool(tool, overlayDock, DragAction.Link, DockOperation.Fill, bExecute: true);

        Assert.False(copyResult);
        Assert.False(linkResult);
    }

    [Fact]
    public void ValidateTool_DockIntoOverlayDock_UsesDockGroupValidation()
    {
        var (factory, _, manager) = CreateSystem();

        var sourceOwner = factory.CreateToolDock();
        sourceOwner.Factory = factory;
        sourceOwner.VisibleDockables = factory.CreateList<IDockable>();

        var tool = (ITool)factory.CreateTool();
        tool.Factory = factory;
        tool.DockGroup = "group-A";
        tool.Owner = sourceOwner;
        sourceOwner.VisibleDockables.Add(tool);

        var overlayDock = factory.CreateOverlayDock();
        overlayDock.Factory = factory;
        var background = factory.CreateToolDock();
        background.Factory = factory;
        background.DockGroup = "group-B";
        overlayDock.BackgroundDockable = background;

        var result = manager.ValidateTool(tool, overlayDock, DragAction.Move, DockOperation.Fill, bExecute: true);

        Assert.False(result);
    }

    [Fact]
    public void ValidateTool_DockIntoOverlayDock_UsesLocalGroupRules_ForPanelCreation()
    {
        var (factory, _, manager) = CreateSystem();

        var sourceOwner = factory.CreateToolDock();
        sourceOwner.Factory = factory;
        sourceOwner.VisibleDockables = factory.CreateList<IDockable>();

        var tool = (ITool)factory.CreateTool();
        tool.Factory = factory;
        tool.Owner = sourceOwner;
        sourceOwner.VisibleDockables.Add(tool);

        var overlayDock = factory.CreateOverlayDock();
        overlayDock.Factory = factory;
        var background = factory.CreateToolDock();
        background.Factory = factory;
        background.DockGroup = "group-A";
        overlayDock.BackgroundDockable = background;

        var result = manager.ValidateTool(tool, overlayDock, DragAction.Move, DockOperation.Fill, bExecute: true);

        Assert.False(result);
    }

    [Fact]
    public void ValidateTool_DockIntoOverlayDock_SplitForwardsToBackgroundDock()
    {
        var (factory, _, manager) = CreateSystem();

        var sourceOwner = factory.CreateToolDock();
        sourceOwner.Factory = factory;
        sourceOwner.VisibleDockables = factory.CreateList<IDockable>();

        var tool = (ITool)factory.CreateTool();
        tool.Factory = factory;
        tool.Owner = sourceOwner;
        sourceOwner.VisibleDockables.Add(tool);

        var background = factory.CreateDocumentDock();
        background.Factory = factory;
        background.VisibleDockables = factory.CreateList<IDockable>();

        var document = (IDocument)factory.CreateDocument();
        document.Factory = factory;
        document.Owner = background;
        background.VisibleDockables.Add(document);
        background.ActiveDockable = document;

        var overlayDock = factory.CreateOverlayDock();
        overlayDock.Factory = factory;
        overlayDock.BackgroundDockable = background;

        var result = manager.ValidateTool(tool, overlayDock, DragAction.Move, DockOperation.Left, bExecute: true);

        Assert.True(result);
        Assert.NotSame(sourceOwner, tool.Owner);
        Assert.NotNull(tool.Owner);
    }

    [Fact]
    public void ValidateTool_DockIntoOverlayDock_SplitWithoutBackgroundFails()
    {
        var (factory, _, manager) = CreateSystem();

        var sourceOwner = factory.CreateToolDock();
        sourceOwner.Factory = factory;
        sourceOwner.VisibleDockables = factory.CreateList<IDockable>();

        var tool = (ITool)factory.CreateTool();
        tool.Factory = factory;
        tool.Owner = sourceOwner;
        sourceOwner.VisibleDockables.Add(tool);

        var overlayDock = factory.CreateOverlayDock();
        overlayDock.Factory = factory;

        var result = manager.ValidateTool(tool, overlayDock, DragAction.Move, DockOperation.Left, bExecute: true);

        Assert.False(result);
    }

    [Fact]
    public void IsDockTargetVisible_OverlayPanelRespectsAllowDockIntoAndShowDockTargets()
    {
        var (_, _, manager) = CreateSystem();

        var overlayPanel = new OverlayPanel
        {
            AllowDockInto = false,
            ShowDockTargets = true
        };

        Assert.False(manager.IsDockTargetVisible(overlayPanel, overlayPanel, DockOperation.Fill));

        overlayPanel.AllowDockInto = true;
        overlayPanel.ShowDockTargets = false;

        Assert.False(manager.IsDockTargetVisible(overlayPanel, overlayPanel, DockOperation.Fill));
    }

    [Fact]
    public void IsDockTargetVisible_OverlayDock_IgnoresGlobalDockingDisablement()
    {
        var (_, _, manager) = CreateSystem();

        var overlayDock = new OverlayDock
        {
            EnableGlobalDocking = false
        };

        var tool = new Tool();

        Assert.True(manager.IsDockTargetVisible(tool, overlayDock, DockOperation.Fill));
    }
}
