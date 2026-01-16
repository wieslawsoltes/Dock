using System.Collections.Generic;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

/// <summary>
/// Core model coverage for OverlayDock tree invariants, ownership bookkeeping, and inheritance helpers.
/// </summary>
public class OverlayDockCoreTests
{
    [Fact]
    public void BackgroundAndPanels_Setters_UpdateVisibleDockablesAndOwners()
    {
        var overlay = new OverlayDock();
        var background = new DocumentDock { Id = "Background" };
        var panel1 = new OverlayPanel { Id = "Panel1" };
        var panel2 = new OverlayPanel { Id = "Panel2" };

        overlay.BackgroundDockable = background;
        overlay.OverlayPanels = new List<IOverlayPanel> { panel1, panel2 };

        Assert.NotNull(overlay.VisibleDockables);
        Assert.Collection(
            overlay.VisibleDockables!,
            first => Assert.Same(background, first),
            second => Assert.Same(panel1, second),
            third => Assert.Same(panel2, third));

        Assert.Same(overlay, background.Owner);
        Assert.Same(overlay, panel1.Owner);
        Assert.Same(overlay, panel2.Owner);
    }

    [Fact]
    public void OverlayPanels_Reset_ReplacesVisibleDockables()
    {
        var overlay = new OverlayDock();
        var background = new DocumentDock { Id = "Background" };
        var oldPanel = new OverlayPanel { Id = "Old" };
        var newPanel = new OverlayPanel { Id = "New" };

        overlay.BackgroundDockable = background;
        overlay.OverlayPanels = new List<IOverlayPanel> { oldPanel };

        overlay.OverlayPanels = new List<IOverlayPanel> { newPanel };

        Assert.NotNull(overlay.VisibleDockables);
        Assert.Equal(2, overlay.VisibleDockables!.Count);
        Assert.Same(background, overlay.VisibleDockables![0]);
        Assert.Same(newPanel, overlay.VisibleDockables![1]);
        Assert.DoesNotContain(oldPanel, overlay.VisibleDockables!);
    }

    [Fact]
    public void SplitterGroups_Set_AssignsOwner()
    {
        var overlay = new OverlayDock();
        var group1 = new OverlaySplitterGroup { Id = "Group1" };
        var group2 = new OverlaySplitterGroup { Id = "Group2" };

        overlay.SplitterGroups = new List<IOverlaySplitterGroup> { group1, group2 };

        Assert.Same(overlay, group1.Owner);
        Assert.Same(overlay, group2.Owner);
    }

    [Fact]
    public void SplitterGroup_Panels_Update_BackReferences()
    {
        var group = new OverlaySplitterGroup { Id = "Group" };
        var first = new OverlayPanel { Id = "First" };
        var second = new OverlayPanel { Id = "Second" };

        group.Panels = new List<IOverlayPanel> { first };
        Assert.Same(group, first.SplitterGroup);

        group.Panels = new List<IOverlayPanel> { second };

        Assert.Null(first.SplitterGroup);
        Assert.Same(group, second.SplitterGroup);
    }

    [Fact]
    public void SplitterGroup_Splitters_Update_Owners()
    {
        var group = new OverlaySplitterGroup { Id = "Group" };
        var splitter1 = new OverlaySplitter { Id = "S1" };
        var splitter2 = new OverlaySplitter { Id = "S2" };

        group.Splitters = new List<IOverlaySplitter> { splitter1, splitter2 };

        Assert.Same(group, splitter1.Owner);
        Assert.Same(group, splitter2.Owner);
    }

    [Fact]
    public void OverlayDock_OverlayPanels_ExcludeGroupedPanels()
    {
        var overlay = new OverlayDock();
        var freePanel = new OverlayPanel { Id = "Free" };
        var groupedPanel = new OverlayPanel { Id = "Grouped" };
        var group = new OverlaySplitterGroup { Id = "Group" };

        group.Panels = new List<IOverlayPanel> { groupedPanel };
        overlay.SplitterGroups = new List<IOverlaySplitterGroup> { group };
        overlay.OverlayPanels = new List<IOverlayPanel> { freePanel, groupedPanel };

        Assert.NotNull(overlay.VisibleDockables);
        Assert.Contains(freePanel, overlay.VisibleDockables!);
        Assert.Contains(groupedPanel, overlay.VisibleDockables!);

        Assert.NotNull(overlay.OverlayPanels);
        Assert.Single(overlay.OverlayPanels!);
        Assert.Same(freePanel, overlay.OverlayPanels![0]);
    }

    [Fact]
    public void GetEffectiveEnableGlobalDocking_Inheritance_DisablesFromAncestor()
    {
        var root = new RootDock { Id = "Root", EnableGlobalDocking = false };
        var overlay = new OverlayDock { Id = "Overlay", Owner = root };
        var panel = new OverlayPanel { Id = "Panel", Owner = overlay };

        Assert.False(DockInheritanceHelper.GetEffectiveEnableGlobalDocking(panel));
    }

    [Fact]
    public void GetEffectiveEnableGlobalDocking_Inheritance_AllowsWhenEnabled()
    {
        var root = new RootDock { Id = "Root", EnableGlobalDocking = true };
        var overlay = new OverlayDock { Id = "Overlay", Owner = root };
        var panel = new OverlayPanel { Id = "Panel", Owner = overlay };

        Assert.True(DockInheritanceHelper.GetEffectiveEnableGlobalDocking(panel));
    }
}

public class OverlayDockManagerTests
{
    private sealed class StubDockService : IDockService
    {
        public int MoveDockableCalls { get; private set; }

        public int SwapDockableCalls { get; private set; }

        public int SplitDockableCalls { get; private set; }

        public int DockIntoWindowCalls { get; private set; }

        public int DockIntoDockableCalls { get; private set; }

        public bool MoveDockableResult { get; set; } = true;

        public bool SwapDockableResult { get; set; } = true;

        public bool SplitDockableResult { get; set; } = true;

        public bool DockIntoWindowResult { get; set; } = true;

        public bool DockIntoDockableResult { get; set; } = true;

        public bool MoveDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, bool bExecute)
        {
            MoveDockableCalls++;
            return MoveDockableResult;
        }

        public bool SwapDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, bool bExecute)
        {
            SwapDockableCalls++;
            return SwapDockableResult;
        }

        public bool SplitDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, DockOperation operation, bool bExecute)
        {
            SplitDockableCalls++;
            return SplitDockableResult;
        }

        public bool DockDockableIntoWindow(IDockable sourceDockable, IDockable targetDockable, DockPoint screenPosition, bool bExecute)
        {
            DockIntoWindowCalls++;
            return DockIntoWindowResult;
        }

        public bool DockDockableIntoDockable(IDockable sourceDockable, IDockable targetDockable, DragAction action, bool bExecute)
        {
            DockIntoDockableCalls++;
            return DockIntoDockableResult;
        }
    }

    private static Document CreateDocumentWithOwner(Factory factory, RootDock root, out DocumentDock owner)
    {
        owner = (DocumentDock)factory.CreateDocumentDock();
        owner.Factory = factory;
        owner.Owner = root;
        owner.IsCollapsable = false;
        owner.VisibleDockables = factory.CreateList<IDockable>();

        var document = (Document)factory.CreateDocument();
        document.Factory = factory;
        document.Owner = owner;
        owner.VisibleDockables.Add(document);
        owner.ActiveDockable = document;

        return document;
    }

    private static OverlayDock CreateOverlayDock(Factory factory, RootDock root)
    {
        var overlay = (OverlayDock)factory.CreateOverlayDock();
        overlay.Factory = factory;
        overlay.Owner = root;
        return overlay;
    }

    private static Tool CreateToolWithOwner(Factory factory, RootDock root, out ToolDock owner)
    {
        owner = (ToolDock)factory.CreateToolDock();
        owner.Factory = factory;
        owner.Owner = root;
        owner.IsCollapsable = false;
        owner.VisibleDockables = factory.CreateList<IDockable>();

        var tool = (Tool)factory.CreateTool();
        tool.Factory = factory;
        tool.Owner = owner;
        owner.VisibleDockables.Add(tool);
        owner.ActiveDockable = tool;

        return tool;
    }

    [Fact]
    public void OverlayDock_ValidateDocument_RequiresMoveAction()
    {
        var dockService = new StubDockService();
        var dockManager = new DockManager(dockService);
        var factory = new Factory();
        var root = (RootDock)factory.CreateRootDock();
        root.Factory = factory;
        var document = CreateDocumentWithOwner(factory, root, out var sourceOwner);
        var overlay = CreateOverlayDock(factory, root);

        var result = dockManager.ValidateDocument(document, overlay, DragAction.Copy, DockOperation.Fill, true);

        Assert.False(result);
        Assert.Equal(0, dockService.SplitDockableCalls);
        Assert.Equal(0, dockService.MoveDockableCalls);
        Assert.Same(sourceOwner, document.Owner);
    }

    [Fact]
    public void OverlayDock_ValidateTool_RequiresMoveAction()
    {
        var dockService = new StubDockService();
        var dockManager = new DockManager(dockService);
        var factory = new Factory();
        var root = (RootDock)factory.CreateRootDock();
        root.Factory = factory;
        var tool = CreateToolWithOwner(factory, root, out var sourceOwner);
        var overlay = CreateOverlayDock(factory, root);

        var result = dockManager.ValidateTool(tool, overlay, DragAction.Copy, DockOperation.Fill, true);

        Assert.False(result);
        Assert.Equal(0, dockService.SplitDockableCalls);
        Assert.Equal(0, dockService.MoveDockableCalls);
        Assert.Same(sourceOwner, tool.Owner);
    }

    [Fact]
    public void OverlayDock_ValidateTool_NonFill_ForwardsToBackgroundDock()
    {
        var dockService = new StubDockService();
        var dockManager = new DockManager(dockService);
        var factory = new Factory();
        var root = (RootDock)factory.CreateRootDock();
        root.Factory = factory;
        root.VisibleDockables = factory.CreateList<IDockable>();

        var tool = CreateToolWithOwner(factory, root, out _);
        var overlay = CreateOverlayDock(factory, root);
        var background = (ToolDock)factory.CreateToolDock();
        background.Factory = factory;
        overlay.BackgroundDockable = background;

        var result = dockManager.ValidateTool(tool, overlay, DragAction.Move, DockOperation.Left, true);

        Assert.True(result);
        Assert.Equal(1, dockService.SplitDockableCalls);
        Assert.Equal(0, dockService.MoveDockableCalls);
    }

    [Fact]
    public void OverlayDock_ValidateTool_Fill_CreatesPanelAndMovesTool()
    {
        var dockService = new StubDockService();
        var dockManager = new DockManager(dockService);
        var factory = new Factory();
        var root = (RootDock)factory.CreateRootDock();
        root.Factory = factory;
        root.VisibleDockables = factory.CreateList<IDockable>();

        var tool = CreateToolWithOwner(factory, root, out var sourceOwner);
        var overlay = CreateOverlayDock(factory, root);
        root.VisibleDockables.Add(sourceOwner);
        root.VisibleDockables.Add(overlay);

        var result = dockManager.ValidateTool(tool, overlay, DragAction.Move, DockOperation.Fill, true);

        Assert.True(result);
        Assert.NotNull(overlay.OverlayPanels);
        Assert.Single(overlay.OverlayPanels!);
        var panel = Assert.IsType<OverlayPanel>(overlay.OverlayPanels![0]);
        Assert.Same(overlay, panel.Owner);
        var nestedDock = Assert.IsType<ToolDock>(panel.Content);
        Assert.Same(panel, nestedDock.Owner);
        Assert.Same(tool, nestedDock.ActiveDockable);
        Assert.Same(nestedDock, tool.Owner);
        Assert.Equal(0, sourceOwner.VisibleDockables?.Count);
        Assert.Equal(panel, overlay.ActiveDockable);
        Assert.Equal(0, dockService.MoveDockableCalls);
    }

    [Fact]
    public void OverlayDock_ValidateTool_Fill_Applies_DefaultPanelProperties()
    {
        var dockService = new StubDockService();
        var dockManager = new DockManager(dockService);
        var factory = new Factory();
        var root = (RootDock)factory.CreateRootDock();
        root.Factory = factory;
        root.VisibleDockables = factory.CreateList<IDockable>();

        var tool = CreateToolWithOwner(factory, root, out var sourceOwner);
        var overlay = CreateOverlayDock(factory, root);
        overlay.DefaultPanelOpacity = 0.35;
        overlay.EnableBackdropBlur = true;
        root.VisibleDockables.Add(sourceOwner);
        root.VisibleDockables.Add(overlay);

        var result = dockManager.ValidateTool(tool, overlay, DragAction.Move, DockOperation.Fill, true);

        Assert.True(result);
        Assert.NotNull(overlay.OverlayPanels);
        var panel = Assert.IsType<OverlayPanel>(Assert.Single(overlay.OverlayPanels!));
        Assert.Equal(0.35, panel.Opacity);
        Assert.True(panel.UseBackdropBlur);
    }

    [Fact]
    public void OverlayDock_ValidateTool_Fill_AllowsWhenGlobalDockingDisabled()
    {
        var dockService = new StubDockService();
        var dockManager = new DockManager(dockService);
        var factory = new Factory();
        var root = (RootDock)factory.CreateRootDock();
        root.Factory = factory;
        root.EnableGlobalDocking = false;
        var tool = CreateToolWithOwner(factory, root, out _);
        var overlay = CreateOverlayDock(factory, root);

        var result = dockManager.ValidateTool(tool, overlay, DragAction.Move, DockOperation.Fill, true);

        Assert.True(result);
        Assert.NotNull(overlay.OverlayPanels);
        Assert.Single(overlay.OverlayPanels!);
        Assert.Equal(0, dockService.MoveDockableCalls);
    }

    [Fact]
    public void OverlayDock_ValidateTool_Fill_BlocksMismatchedGroups()
    {
        var dockService = new StubDockService();
        var dockManager = new DockManager(dockService);
        var factory = new Factory();
        var root = (RootDock)factory.CreateRootDock();
        root.Factory = factory;
        var tool = CreateToolWithOwner(factory, root, out _);
        tool.DockGroup = "Alpha";
        var overlay = CreateOverlayDock(factory, root);
        var background = (ToolDock)factory.CreateToolDock();
        background.Factory = factory;
        background.DockGroup = "Beta";
        overlay.BackgroundDockable = background;

        var result = dockManager.ValidateTool(tool, overlay, DragAction.Move, DockOperation.Fill, true);

        Assert.False(result);
        Assert.Null(overlay.OverlayPanels);
        Assert.Equal(0, dockService.MoveDockableCalls);
    }

    [Fact]
    public void OverlayDock_ValidateTool_Fill_AllowsMatchingGroups()
    {
        var dockService = new StubDockService();
        var dockManager = new DockManager(dockService);
        var factory = new Factory();
        var root = (RootDock)factory.CreateRootDock();
        root.Factory = factory;
        root.VisibleDockables = factory.CreateList<IDockable>();

        var tool = CreateToolWithOwner(factory, root, out var sourceOwner);
        tool.DockGroup = "Alpha";
        var overlay = CreateOverlayDock(factory, root);
        var background = (ToolDock)factory.CreateToolDock();
        background.Factory = factory;
        background.DockGroup = "Alpha";
        overlay.BackgroundDockable = background;
        root.VisibleDockables.Add(sourceOwner);
        root.VisibleDockables.Add(overlay);

        var result = dockManager.ValidateTool(tool, overlay, DragAction.Move, DockOperation.Fill, true);

        Assert.True(result);
        Assert.NotNull(overlay.OverlayPanels);
        var panels = overlay.OverlayPanels!;
        Assert.Single(panels);
        var panel = Assert.IsType<OverlayPanel>(panels[0]);
        Assert.Same(overlay, panel.Owner);
        var nestedDock = Assert.IsType<ToolDock>(panel.Content);
        Assert.Same(panel, nestedDock.Owner);
        Assert.Same(tool, nestedDock.ActiveDockable);
        Assert.Same(nestedDock, tool.Owner);
        Assert.Equal(0, sourceOwner.VisibleDockables?.Count);
        Assert.Equal(panel, overlay.ActiveDockable);
        Assert.Equal(0, dockService.MoveDockableCalls);
    }

    [Fact]
    public void OverlayDock_ValidateTool_LocalDocking_DisabledByPanel()
    {
        var dockService = new StubDockService();
        var dockManager = new DockManager(dockService);
        var factory = new Factory();
        var root = (RootDock)factory.CreateRootDock();
        root.Factory = factory;
        var tool = CreateToolWithOwner(factory, root, out var sourceOwner);

        var overlay = CreateOverlayDock(factory, root);
        var panel = (OverlayPanel)factory.CreateOverlayPanel();
        panel.Factory = factory;
        panel.AllowDockInto = false;
        var panels = factory.CreateList<IOverlayPanel>();
        panels.Add(panel);
        factory.SetOverlayDockOverlayPanels(overlay, panels);

        var result = dockManager.ValidateTool(tool, panel, DragAction.Move, DockOperation.Fill, true);

        Assert.False(result);
        Assert.Null(panel.NestedDock);
        Assert.Same(sourceOwner, tool.Owner);
        Assert.Equal(0, dockService.MoveDockableCalls);
        Assert.Equal(0, dockService.SplitDockableCalls);
    }

    [Fact]
    public void OverlayDock_ValidateTool_LocalDocking_BlocksMismatchedGroups()
    {
        var dockService = new StubDockService();
        var dockManager = new DockManager(dockService);
        var factory = new Factory();
        var root = (RootDock)factory.CreateRootDock();
        root.Factory = factory;
        var tool = CreateToolWithOwner(factory, root, out var sourceOwner);
        tool.DockGroup = "Alpha";

        var overlay = CreateOverlayDock(factory, root);
        var panel = (OverlayPanel)factory.CreateOverlayPanel();
        panel.Factory = factory;
        var nestedDock = (ToolDock)factory.CreateToolDock();
        nestedDock.Factory = factory;
        nestedDock.DockGroup = "Beta";
        nestedDock.VisibleDockables = factory.CreateList<IDockable>();
        var existingTool = (Tool)factory.CreateTool();
        existingTool.Factory = factory;
        existingTool.DockGroup = "Beta";
        existingTool.Owner = nestedDock;
        nestedDock.VisibleDockables.Add(existingTool);
        factory.SetOverlayPanelContent(panel, nestedDock);
        var panels = factory.CreateList<IOverlayPanel>();
        panels.Add(panel);
        factory.SetOverlayDockOverlayPanels(overlay, panels);

        var result = dockManager.ValidateTool(tool, panel, DragAction.Move, DockOperation.Fill, true);

        Assert.False(result);
        Assert.Same(nestedDock, panel.NestedDock);
        Assert.Same(sourceOwner, tool.Owner);
        Assert.Equal(0, dockService.MoveDockableCalls);
        Assert.Equal(0, dockService.SplitDockableCalls);
    }

    [Fact]
    public void OverlayDock_ValidateDocument_LocalDocking_DisabledByPanel()
    {
        var dockService = new StubDockService();
        var dockManager = new DockManager(dockService);
        var factory = new Factory();
        var root = (RootDock)factory.CreateRootDock();
        root.Factory = factory;
        var document = CreateDocumentWithOwner(factory, root, out var sourceOwner);

        var overlay = CreateOverlayDock(factory, root);
        var panel = (OverlayPanel)factory.CreateOverlayPanel();
        panel.Factory = factory;
        panel.AllowDockInto = false;
        var panels = factory.CreateList<IOverlayPanel>();
        panels.Add(panel);
        factory.SetOverlayDockOverlayPanels(overlay, panels);

        var result = dockManager.ValidateDocument(document, panel, DragAction.Move, DockOperation.Fill, true);

        Assert.False(result);
        Assert.Null(panel.NestedDock);
        Assert.Same(sourceOwner, document.Owner);
        Assert.Equal(0, dockService.MoveDockableCalls);
        Assert.Equal(0, dockService.SplitDockableCalls);
    }

    [Fact]
    public void OverlayDock_ValidateDocument_LocalDocking_BlocksMismatchedGroups()
    {
        var dockService = new StubDockService();
        var dockManager = new DockManager(dockService);
        var factory = new Factory();
        var root = (RootDock)factory.CreateRootDock();
        root.Factory = factory;
        var document = CreateDocumentWithOwner(factory, root, out var sourceOwner);
        document.DockGroup = "Alpha";

        var overlay = CreateOverlayDock(factory, root);
        var panel = (OverlayPanel)factory.CreateOverlayPanel();
        panel.Factory = factory;
        var nestedDock = (DocumentDock)factory.CreateDocumentDock();
        nestedDock.Factory = factory;
        nestedDock.DockGroup = "Beta";
        nestedDock.VisibleDockables = factory.CreateList<IDockable>();
        var existingDocument = (Document)factory.CreateDocument();
        existingDocument.Factory = factory;
        existingDocument.DockGroup = "Beta";
        existingDocument.Owner = nestedDock;
        nestedDock.VisibleDockables.Add(existingDocument);
        factory.SetOverlayPanelContent(panel, nestedDock);
        var panels = factory.CreateList<IOverlayPanel>();
        panels.Add(panel);
        factory.SetOverlayDockOverlayPanels(overlay, panels);

        var result = dockManager.ValidateDocument(document, panel, DragAction.Move, DockOperation.Fill, true);

        Assert.False(result);
        Assert.Same(nestedDock, panel.NestedDock);
        Assert.Same(sourceOwner, document.Owner);
        Assert.Equal(0, dockService.MoveDockableCalls);
        Assert.Equal(0, dockService.SplitDockableCalls);
    }

    [Fact]
    public void OverlayDock_ValidateDocument_LocalDocking_CreatesNestedDock()
    {
        var dockService = new StubDockService();
        var dockManager = new DockManager(dockService);
        var factory = new Factory();
        var root = (RootDock)factory.CreateRootDock();
        root.Factory = factory;
        var document = CreateDocumentWithOwner(factory, root, out _);
        document.DockGroup = "Alpha";

        var overlay = CreateOverlayDock(factory, root);
        var panel = (OverlayPanel)factory.CreateOverlayPanel();
        panel.Factory = factory;
        panel.AllowDockInto = true;
        var panels = factory.CreateList<IOverlayPanel>();
        panels.Add(panel);
        factory.SetOverlayDockOverlayPanels(overlay, panels);

        var result = dockManager.ValidateDocument(document, panel, DragAction.Move, DockOperation.Fill, true);

        Assert.True(result);
        var nestedDock = Assert.IsType<DocumentDock>(panel.NestedDock);
        Assert.Same(panel, nestedDock.Owner);
        Assert.Equal("Alpha", nestedDock.DockGroup);
        Assert.NotNull(panel.VisibleDockables);
        Assert.Contains(nestedDock, panel.VisibleDockables!);
        Assert.Equal(1, dockService.MoveDockableCalls);
        Assert.Equal(0, dockService.SplitDockableCalls);
    }

    [Fact]
    public void OverlayDock_ValidateTool_Fill_CreatesNewPanelPerFill()
    {
        var dockService = new StubDockService();
        var dockManager = new DockManager(dockService);
        var factory = new Factory();
        var root = (RootDock)factory.CreateRootDock();
        root.Factory = factory;
        root.VisibleDockables = factory.CreateList<IDockable>();

        var tool1 = CreateToolWithOwner(factory, root, out var owner1);
        var tool2 = CreateToolWithOwner(factory, root, out var owner2);
        var overlay = CreateOverlayDock(factory, root);
        root.VisibleDockables.Add(owner1);
        root.VisibleDockables.Add(owner2);
        root.VisibleDockables.Add(overlay);

        var first = dockManager.ValidateTool(tool1, overlay, DragAction.Move, DockOperation.Fill, true);
        var second = dockManager.ValidateTool(tool2, overlay, DragAction.Move, DockOperation.Fill, true);

        Assert.True(first);
        Assert.True(second);
        Assert.NotNull(overlay.OverlayPanels);
        var panels = overlay.OverlayPanels!;
        Assert.Equal(2, panels.Count);
        var firstPanel = Assert.IsType<OverlayPanel>(panels[0]);
        var firstNested = Assert.IsType<ToolDock>(firstPanel.Content);
        Assert.Same(tool1, firstNested.ActiveDockable);
        var secondPanel = Assert.IsType<OverlayPanel>(panels[1]);
        var secondNested = Assert.IsType<ToolDock>(secondPanel.Content);
        Assert.Same(tool2, secondNested.ActiveDockable);
        Assert.Empty(owner1.VisibleDockables!);
        Assert.Empty(owner2.VisibleDockables!);
        Assert.Equal(0, dockService.MoveDockableCalls);
    }

    [Fact]
    public void OverlayDock_ValidateDocument_Fill_CreatesNewPanelPerFill()
    {
        var dockService = new StubDockService();
        var dockManager = new DockManager(dockService);
        var factory = new Factory();
        var root = (RootDock)factory.CreateRootDock();
        root.Factory = factory;
        root.VisibleDockables = factory.CreateList<IDockable>();

        var document1 = CreateDocumentWithOwner(factory, root, out var owner1);
        var document2 = CreateDocumentWithOwner(factory, root, out var owner2);
        var overlay = CreateOverlayDock(factory, root);
        root.VisibleDockables.Add(owner1);
        root.VisibleDockables.Add(owner2);
        root.VisibleDockables.Add(overlay);

        var first = dockManager.ValidateDocument(document1, overlay, DragAction.Move, DockOperation.Fill, true);
        var second = dockManager.ValidateDocument(document2, overlay, DragAction.Move, DockOperation.Fill, true);

        Assert.True(first);
        Assert.True(second);
        Assert.NotNull(overlay.OverlayPanels);
        var panels = overlay.OverlayPanels!;
        Assert.Equal(2, panels.Count);
        var firstPanel = Assert.IsType<OverlayPanel>(panels[0]);
        var firstNested = Assert.IsType<DocumentDock>(firstPanel.Content);
        Assert.Same(document1, firstNested.ActiveDockable);
        var secondPanel = Assert.IsType<OverlayPanel>(panels[1]);
        var secondNested = Assert.IsType<DocumentDock>(secondPanel.Content);
        Assert.Same(document2, secondNested.ActiveDockable);
        Assert.Empty(owner1.VisibleDockables!);
        Assert.Empty(owner2.VisibleDockables!);
        Assert.Equal(0, dockService.MoveDockableCalls);
    }

    [Fact]
    public void OverlayDock_ValidateDocument_NonFill_ForwardsToBackgroundDock()
    {
        var dockService = new StubDockService();
        var dockManager = new DockManager(dockService);
        var factory = new Factory();
        var root = (RootDock)factory.CreateRootDock();
        root.Factory = factory;
        root.VisibleDockables = factory.CreateList<IDockable>();

        var document = CreateDocumentWithOwner(factory, root, out _);
        var overlay = CreateOverlayDock(factory, root);
        var background = (DocumentDock)factory.CreateDocumentDock();
        background.Factory = factory;
        overlay.BackgroundDockable = background;

        var result = dockManager.ValidateDocument(document, overlay, DragAction.Move, DockOperation.Left, true);

        Assert.True(result);
        Assert.Equal(1, dockService.SplitDockableCalls);
        Assert.Equal(0, dockService.MoveDockableCalls);
    }

    [Fact]
    public void OverlayDock_ValidateDocument_Fill_AllowsWhenGlobalDockingDisabled()
    {
        var dockService = new StubDockService();
        var dockManager = new DockManager(dockService);
        var factory = new Factory();
        var root = (RootDock)factory.CreateRootDock();
        root.Factory = factory;
        root.EnableGlobalDocking = false;
        var document = CreateDocumentWithOwner(factory, root, out _);
        var overlay = CreateOverlayDock(factory, root);

        var result = dockManager.ValidateDocument(document, overlay, DragAction.Move, DockOperation.Fill, true);

        Assert.True(result);
        Assert.NotNull(overlay.OverlayPanels);
        Assert.Single(overlay.OverlayPanels!);
        Assert.Equal(0, dockService.MoveDockableCalls);
    }

    [Fact]
    public void OverlayDock_ValidateDocument_Fill_BlocksMismatchedGroups()
    {
        var dockService = new StubDockService();
        var dockManager = new DockManager(dockService);
        var factory = new Factory();
        var root = (RootDock)factory.CreateRootDock();
        root.Factory = factory;
        var document = CreateDocumentWithOwner(factory, root, out _);
        document.DockGroup = "Alpha";
        var overlay = CreateOverlayDock(factory, root);
        var background = (DocumentDock)factory.CreateDocumentDock();
        background.Factory = factory;
        background.DockGroup = "Beta";
        overlay.BackgroundDockable = background;

        var result = dockManager.ValidateDocument(document, overlay, DragAction.Move, DockOperation.Fill, true);

        Assert.False(result);
        Assert.Null(overlay.OverlayPanels);
        Assert.Equal(0, dockService.MoveDockableCalls);
    }

    [Fact]
    public void OverlayDock_ValidateDocument_Fill_AllowsMatchingGroups()
    {
        var dockService = new StubDockService();
        var dockManager = new DockManager(dockService);
        var factory = new Factory();
        var root = (RootDock)factory.CreateRootDock();
        root.Factory = factory;
        root.VisibleDockables = factory.CreateList<IDockable>();

        var document = CreateDocumentWithOwner(factory, root, out var sourceOwner);
        document.DockGroup = "Alpha";
        var overlay = CreateOverlayDock(factory, root);
        var background = (DocumentDock)factory.CreateDocumentDock();
        background.Factory = factory;
        background.DockGroup = "Alpha";
        overlay.BackgroundDockable = background;
        root.VisibleDockables.Add(sourceOwner);
        root.VisibleDockables.Add(overlay);

        var result = dockManager.ValidateDocument(document, overlay, DragAction.Move, DockOperation.Fill, true);

        Assert.True(result);
        Assert.NotNull(overlay.OverlayPanels);
        var panels = overlay.OverlayPanels!;
        Assert.Single(panels);
        var panel = Assert.IsType<OverlayPanel>(panels[0]);
        Assert.Same(overlay, panel.Owner);
        var nestedDock = Assert.IsType<DocumentDock>(panel.Content);
        Assert.Same(panel, nestedDock.Owner);
        Assert.Same(document, nestedDock.ActiveDockable);
        Assert.Same(nestedDock, document.Owner);
        Assert.Equal(0, sourceOwner.VisibleDockables?.Count);
        Assert.Equal(panel, overlay.ActiveDockable);
        Assert.Equal(0, dockService.MoveDockableCalls);
    }

    [Fact]
    public void OverlayDock_ValidateDocument_Fill_CreatesPanelAndMovesDocument()
    {
        var dockService = new StubDockService();
        var dockManager = new DockManager(dockService);
        var factory = new Factory();
        var root = (RootDock)factory.CreateRootDock();
        root.Factory = factory;
        root.VisibleDockables = factory.CreateList<IDockable>();

        var document = CreateDocumentWithOwner(factory, root, out var sourceOwner);
        var overlay = CreateOverlayDock(factory, root);
        root.VisibleDockables.Add(sourceOwner);
        root.VisibleDockables.Add(overlay);

        var result = dockManager.ValidateDocument(document, overlay, DragAction.Move, DockOperation.Fill, true);

        Assert.True(result);
        Assert.NotNull(overlay.OverlayPanels);
        Assert.Single(overlay.OverlayPanels!);
        var panel = Assert.IsType<OverlayPanel>(overlay.OverlayPanels![0]);
        Assert.Same(overlay, panel.Owner);
        var nestedDock = Assert.IsType<DocumentDock>(panel.Content);
        Assert.Same(panel, nestedDock.Owner);
        Assert.Same(document, nestedDock.ActiveDockable);
        Assert.Same(nestedDock, document.Owner);
        Assert.Equal(0, sourceOwner.VisibleDockables?.Count);
        Assert.Equal(panel, overlay.ActiveDockable);
        Assert.Equal(0, dockService.MoveDockableCalls);
    }
}
