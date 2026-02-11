using System.Text.Json;
using Avalonia.Collections;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Avalonia.Json;
using Dock.Model.Controls;
using Dock.Model.Core;
using System.Linq;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests;

public class AvaloniaDockSerializerTests
{
    [AvaloniaFact]
    public void Serialize_RootDock_IncludesPinnedDockDisplayMode()
    {
        var serializer = new AvaloniaDockSerializer();
        var root = new RootDock
        {
            PinnedDockDisplayMode = PinnedDockDisplayMode.Inline
        };

        var json = serializer.Serialize(root);

        using var document = JsonDocument.Parse(json);
        Assert.True(document.RootElement.TryGetProperty("PinnedDockDisplayMode", out var value));
        Assert.Equal((int)PinnedDockDisplayMode.Inline, value.GetInt32());
    }

    [AvaloniaFact]
    public void Serialize_Tool_IncludesPinnedBounds()
    {
        var serializer = new AvaloniaDockSerializer();
        var tool = new Tool();
        tool.SetPinnedBounds(1, 2, 300, 400);
        Assert.NotNull(tool.PinnedBounds);

        var json = serializer.Serialize(tool);

        using var document = JsonDocument.Parse(json);
        Assert.True(document.RootElement.TryGetProperty("PinnedBounds", out var bounds), json);
        Assert.Equal(1, bounds.GetProperty("X").GetDouble());
        Assert.Equal(2, bounds.GetProperty("Y").GetDouble());
        Assert.Equal(300, bounds.GetProperty("Width").GetDouble());
        Assert.Equal(400, bounds.GetProperty("Height").GetDouble());
    }

    [AvaloniaFact]
    public void Serialize_Tool_IncludesDockingState()
    {
        var serializer = new AvaloniaDockSerializer();
        var tool = new Tool
        {
            DockingState = DockingWindowState.Document | DockingWindowState.Floating
        };

        var json = serializer.Serialize(tool);

        using var document = JsonDocument.Parse(json);
        Assert.True(document.RootElement.TryGetProperty("DockingState", out var state), json);
        Assert.Equal((int)(DockingWindowState.Document | DockingWindowState.Floating), state.GetInt32());
    }

    [AvaloniaFact]
    public void Serialize_Tool_DoesNotInclude_DockingWindowStateMixinProperties()
    {
        var serializer = new AvaloniaDockSerializer();
        var tool = new Tool
        {
            IsOpen = true,
            IsActive = true,
            IsSelected = true
        };

        var json = serializer.Serialize(tool);

        using var document = JsonDocument.Parse(json);
        Assert.False(document.RootElement.TryGetProperty("IsOpen", out _), json);
        Assert.False(document.RootElement.TryGetProperty("IsActive", out _), json);
        Assert.False(document.RootElement.TryGetProperty("IsSelected", out _), json);
    }

    [AvaloniaFact]
    public void Serialize_ToolDock_DoesNotIncludeItemsSourceOrToolTemplate()
    {
        var serializer = new AvaloniaDockSerializer();
        var dock = new ToolDock
        {
            ToolTemplate = new ToolTemplate { Content = "template" },
            ItemsSource = new[] { "ToolA" }
        };

        var json = serializer.Serialize(dock);

        using var document = JsonDocument.Parse(json);
        Assert.False(document.RootElement.TryGetProperty("ItemsSource", out _), json);
        Assert.False(document.RootElement.TryGetProperty("ToolTemplate", out _), json);
    }

    [AvaloniaFact]
    public void Serialize_DocumentDock_IncludesCanUpdateItemsSourceOnUnregister()
    {
        var serializer = new AvaloniaDockSerializer();
        var dock = new DocumentDock
        {
            CanUpdateItemsSourceOnUnregister = false
        };

        var json = serializer.Serialize(dock);

        using var document = JsonDocument.Parse(json);
        Assert.True(document.RootElement.TryGetProperty("CanUpdateItemsSourceOnUnregister", out var value), json);
        Assert.False(value.GetBoolean());
    }

    [AvaloniaFact]
    public void Deserialize_DocumentDock_RestoresCanUpdateItemsSourceOnUnregister()
    {
        var serializer = new AvaloniaDockSerializer();
        var dock = new DocumentDock
        {
            CanUpdateItemsSourceOnUnregister = false
        };

        var json = serializer.Serialize(dock);
        var restored = serializer.Deserialize<DocumentDock>(json);

        Assert.NotNull(restored);
        Assert.False(restored!.CanUpdateItemsSourceOnUnregister);
    }

    [AvaloniaFact]
    public void Serialize_ToolDock_IncludesCanUpdateItemsSourceOnUnregister()
    {
        var serializer = new AvaloniaDockSerializer();
        var dock = new ToolDock
        {
            CanUpdateItemsSourceOnUnregister = false
        };

        var json = serializer.Serialize(dock);

        using var document = JsonDocument.Parse(json);
        Assert.True(document.RootElement.TryGetProperty("CanUpdateItemsSourceOnUnregister", out var value), json);
        Assert.False(value.GetBoolean());
    }

    [AvaloniaFact]
    public void Deserialize_ToolDock_RestoresCanUpdateItemsSourceOnUnregister()
    {
        var serializer = new AvaloniaDockSerializer();
        var dock = new ToolDock
        {
            CanUpdateItemsSourceOnUnregister = false
        };

        var json = serializer.Serialize(dock);
        var restored = serializer.Deserialize<ToolDock>(json);

        Assert.NotNull(restored);
        Assert.False(restored!.CanUpdateItemsSourceOnUnregister);
    }

    [AvaloniaFact]
    public void Deserialize_Tool_RestoresDockingState()
    {
        var serializer = new AvaloniaDockSerializer();
        var tool = new Tool
        {
            DockingState = DockingWindowState.Document | DockingWindowState.Floating | DockingWindowState.Hidden
        };

        var json = serializer.Serialize(tool);
        var restored = serializer.Deserialize<Tool>(json);

        Assert.NotNull(restored);
        Assert.Equal(tool.DockingState, restored!.DockingState);
    }

    [AvaloniaFact]
    public void Serialize_RootDock_IncludesPinnedDock()
    {
        var serializer = new AvaloniaDockSerializer();
        var tool = new Tool { Id = "PinnedTool" };
        var pinnedDock = new ToolDock
        {
            VisibleDockables = new AvaloniaList<IDockable> { tool },
            ActiveDockable = tool
        };
        var root = new RootDock
        {
            PinnedDock = pinnedDock
        };

        var json = serializer.Serialize(root);

        using var document = JsonDocument.Parse(json);
        Assert.True(document.RootElement.TryGetProperty("PinnedDock", out var pinnedDockElement), json);
        Assert.True(pinnedDockElement.TryGetProperty("VisibleDockables", out _), json);
    }

    [AvaloniaFact]
    public void Deserialize_RootDock_RestoresPinnedDock()
    {
        var serializer = new AvaloniaDockSerializer();
        var factory = new Factory();

        var tool = new Tool { Id = "PinnedTool" };
        tool.SetPinnedBounds(0, 0, 240, 180);

        var pinnedDock = new ToolDock
        {
            VisibleDockables = new AvaloniaList<IDockable> { tool },
            ActiveDockable = tool
        };

        var root = new RootDock
        {
            LeftPinnedDockables = new AvaloniaList<IDockable> { tool },
            PinnedDock = pinnedDock
        };

        var json = serializer.Serialize(root);
        var restored = serializer.Deserialize<IRootDock>(json);

        Assert.NotNull(restored);

        factory.InitLayout((IDockable)restored!);

        Assert.NotNull(restored.PinnedDock);
        Assert.NotNull(restored.PinnedDock!.VisibleDockables);
        Assert.Single(restored.PinnedDock.VisibleDockables!);
        Assert.NotNull(restored.LeftPinnedDockables);
        Assert.Single(restored.LeftPinnedDockables!);

        var pinnedTool = restored.PinnedDock.VisibleDockables!.OfType<ITool>().Single();

        Assert.Same(restored.LeftPinnedDockables![0], pinnedTool);
        Assert.Same(pinnedTool, restored.PinnedDock.ActiveDockable);

        pinnedTool.GetPinnedBounds(out _, out _, out var width, out var height);
        Assert.Equal(240, width, 3);
        Assert.Equal(180, height, 3);
    }

    [AvaloniaFact]
    public void Serialize_DockWindow_IncludesWindowState()
    {
        var serializer = new AvaloniaDockSerializer();
        var window = new DockWindow
        {
            Id = "Window",
            Title = "Window",
            WindowState = DockWindowState.Maximized
        };

        var json = serializer.Serialize(window);

        using var document = JsonDocument.Parse(json);
        Assert.True(document.RootElement.TryGetProperty("WindowState", out var value), json);
        Assert.Equal((int)DockWindowState.Maximized, value.GetInt32());
    }

    [AvaloniaFact]
    public void Deserialize_SplitLayout_FocusedDocumentSwitches_Update_GlobalTracking_Without_WindowReactivation()
    {
        var serializer = new AvaloniaDockSerializer();
        var factory = new Factory();

        var staleRoot = CreateSingleDocumentRoot(factory, "stale");
        var window = factory.CreateDockWindow();
        window.Id = "main-window";
        window.Layout = staleRoot;
        staleRoot.Window = window;

        var dockControl = new TestDockControl
        {
            Factory = factory,
            Layout = staleRoot
        };
        factory.DockControls.Add(dockControl);

        factory.OnWindowActivated(window);
        Assert.Same(staleRoot, factory.CurrentRootDock);

        var sourceRoot = CreateSplitDocumentRoot(factory, "restored");
        var json = serializer.Serialize(sourceRoot);
        var restored = serializer.Deserialize<IRootDock>(json);
        Assert.NotNull(restored);

        factory.InitLayout((IDockable)restored!);

        window.Layout = restored;
        restored!.Window = window;
        dockControl.Layout = restored;

        var left = factory.Find((IDock)restored, x => x.Id == "left-doc-restored").FirstOrDefault();
        var right = factory.Find((IDock)restored, x => x.Id == "right-doc-restored").FirstOrDefault();
        Assert.NotNull(left);
        Assert.NotNull(right);

        var focusedChangeCount = 0;
        factory.GlobalDockTrackingChanged += (_, args) =>
        {
            if (args.Reason == DockTrackingChangeReason.FocusedDockableChanged)
            {
                focusedChangeCount++;
            }
        };

        factory.OnFocusedDockableChanged(left);
        factory.OnFocusedDockableChanged(right);

        Assert.Equal(2, focusedChangeCount);
        Assert.Same(right, factory.CurrentDockable);
        Assert.Same(restored, factory.CurrentRootDock);
        Assert.Same(window, factory.CurrentDockWindow);
    }

    private static IRootDock CreateSingleDocumentRoot(Factory factory, string idSuffix)
    {
        var root = factory.CreateRootDock();
        root.Id = $"root-{idSuffix}";

        var dock = factory.CreateDocumentDock();
        dock.Id = $"dock-{idSuffix}";
        dock.Owner = root;

        var document = factory.CreateDocument();
        document.Id = $"doc-{idSuffix}";
        document.Title = $"Doc-{idSuffix}";
        document.Owner = dock;

        dock.VisibleDockables = factory.CreateList<IDockable>(document);
        dock.ActiveDockable = document;
        dock.FocusedDockable = document;

        root.VisibleDockables = factory.CreateList<IDockable>(dock);
        root.ActiveDockable = dock;
        root.FocusedDockable = document;

        return root;
    }

    private static IRootDock CreateSplitDocumentRoot(Factory factory, string idSuffix)
    {
        var root = factory.CreateRootDock();
        root.Id = $"root-{idSuffix}";

        var splitDock = factory.CreateProportionalDock();
        splitDock.Id = $"split-{idSuffix}";
        splitDock.Owner = root;

        var leftDock = factory.CreateDocumentDock();
        leftDock.Id = $"left-dock-{idSuffix}";
        leftDock.Owner = splitDock;

        var rightDock = factory.CreateDocumentDock();
        rightDock.Id = $"right-dock-{idSuffix}";
        rightDock.Owner = splitDock;

        var leftDocument = factory.CreateDocument();
        leftDocument.Id = $"left-doc-{idSuffix}";
        leftDocument.Title = $"Left-{idSuffix}";
        leftDocument.Owner = leftDock;

        var rightDocument = factory.CreateDocument();
        rightDocument.Id = $"right-doc-{idSuffix}";
        rightDocument.Title = $"Right-{idSuffix}";
        rightDocument.Owner = rightDock;

        leftDock.VisibleDockables = factory.CreateList<IDockable>(leftDocument);
        leftDock.ActiveDockable = leftDocument;
        leftDock.FocusedDockable = leftDocument;

        rightDock.VisibleDockables = factory.CreateList<IDockable>(rightDocument);
        rightDock.ActiveDockable = rightDocument;
        rightDock.FocusedDockable = rightDocument;

        splitDock.VisibleDockables = factory.CreateList<IDockable>(leftDock, rightDock);
        splitDock.ActiveDockable = leftDock;
        splitDock.FocusedDockable = leftDocument;

        root.VisibleDockables = factory.CreateList<IDockable>(splitDock);
        root.ActiveDockable = splitDock;
        root.FocusedDockable = leftDocument;

        return root;
    }

    private sealed class TestDockControl : IDockControl
    {
        public IDockManager DockManager { get; } = null!;

        public IDockControlState DockControlState { get; } = null!;

        public IDock? Layout { get; set; }

        public object? DefaultContext { get; set; }

        public bool InitializeLayout { get; set; }

        public bool InitializeFactory { get; set; }

        public IFactory? Factory { get; set; }
    }
}
