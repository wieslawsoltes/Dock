using System;
using System.Collections.Generic;
using System.IO;
using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class DockWorkspaceManagerTests
{
    private sealed class StubDockSerializer : IDockSerializer
    {
        public string? LastSerialized { get; private set; }
        public object? LastValue { get; private set; }

        public string Serialize<T>(T value)
        {
            LastValue = value;
            LastSerialized = Guid.NewGuid().ToString();
            return LastSerialized;
        }

        public T? Deserialize<T>(string text)
        {
            if (LastSerialized == text && LastValue is T typed)
            {
                return typed;
            }

            return default;
        }

        public T? Load<T>(Stream stream)
        {
            throw new NotSupportedException();
        }

        public void Save<T>(Stream stream, T value)
        {
            throw new NotSupportedException();
        }
    }

    [Fact]
    public void Capture_Includes_State_When_Requested()
    {
        var serializer = new StubDockSerializer();
        var manager = new DockWorkspaceManager(serializer);

        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();
        factory.InitLayout(root);

        var workspace = manager.Capture("A", root, includeState: true);

        Assert.NotNull(workspace.State);
        Assert.Equal(workspace, manager.ActiveWorkspace);
    }

    [Fact]
    public void Restore_Uses_Serializer_And_Updates_ActiveWorkspace()
    {
        var serializer = new StubDockSerializer();
        var manager = new DockWorkspaceManager(serializer);

        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();
        factory.InitLayout(root);

        var workspace = manager.Capture("B", root, includeState: false);

        var restored = manager.Restore(workspace);

        Assert.Same(root, restored);
        Assert.Equal(workspace, manager.ActiveWorkspace);
    }

    [Fact]
    public void TrackFactory_MarksDirty_WhenDockableChanges()
    {
        var serializer = new StubDockSerializer();
        var manager = new DockWorkspaceManager(serializer);
        var factory = new Factory();
        var dockable = factory.CreateDocument();

        manager.TrackFactory(factory);

        Assert.False(manager.IsDirty);

        factory.OnDockableMoved(dockable);

        Assert.True(manager.IsDirty);
    }

    [Fact]
    public void Capture_ClearsDirtyState()
    {
        var serializer = new StubDockSerializer();
        var manager = new DockWorkspaceManager(serializer);
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();
        factory.InitLayout(root);
        var dockable = factory.CreateDocument();

        manager.TrackFactory(factory);
        factory.OnDockableDocked(dockable, DockOperation.Fill);

        Assert.True(manager.IsDirty);

        var workspace = manager.Capture("Layout", root, includeState: false);

        Assert.False(manager.IsDirty);
        Assert.False(workspace.IsDirty);
    }

    [Fact]
    public void TrackFactory_RespectsDockableFilter()
    {
        var serializer = new StubDockSerializer();
        var manager = new DockWorkspaceManager(serializer);
        var factory = new Factory();
        var dockable = factory.CreateDocument();

        manager.TrackFactory(factory, new DockWorkspaceTrackingOptions
        {
            DockableFilter = _ => false
        });

        factory.OnDockableDocked(dockable, DockOperation.Fill);

        Assert.False(manager.IsDirty);
    }

    [Fact]
    public void TrackLayout_IgnoresDockablesOutsideTrackedRoot()
    {
        var serializer = new StubDockSerializer();
        var manager = new DockWorkspaceManager(serializer);
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();
        factory.InitLayout(root);

        var otherRoot = factory.CreateRootDock();
        otherRoot.VisibleDockables = factory.CreateList<IDockable>();
        factory.InitLayout(otherRoot);

        manager.TrackLayout(root);

        var dockable = factory.CreateDocument();
        dockable.Owner = otherRoot;

        factory.OnDockableMoved(dockable);

        Assert.False(manager.IsDirty);

        dockable.Owner = root;
        factory.OnDockableMoved(dockable);

        Assert.True(manager.IsDirty);
    }

    [Fact]
    public void TrackFactory_ClearsLayoutScope()
    {
        var serializer = new StubDockSerializer();
        var manager = new DockWorkspaceManager(serializer);
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();
        factory.InitLayout(root);

        var otherRoot = factory.CreateRootDock();
        otherRoot.VisibleDockables = factory.CreateList<IDockable>();
        factory.InitLayout(otherRoot);

        manager.TrackLayout(root);
        manager.TrackFactory(factory);

        var dockable = factory.CreateDocument();
        dockable.Owner = otherRoot;

        factory.OnDockableMoved(dockable);

        Assert.True(manager.IsDirty);
    }

    [Fact]
    public void TrackFactory_MarksActiveWorkspaceDirty()
    {
        var serializer = new StubDockSerializer();
        var manager = new DockWorkspaceManager(serializer);
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();
        factory.InitLayout(root);

        manager.TrackFactory(factory);
        var workspace = manager.Capture("Active", root, includeState: false);

        var dockable = factory.CreateDocument();
        dockable.Owner = root;

        factory.OnDockableMoved(dockable);

        Assert.True(manager.IsDirty);
        Assert.True(workspace.IsDirty);
    }

    [Fact]
    public void WorkspaceDirtyChanged_Raises_On_State_Changes()
    {
        var serializer = new StubDockSerializer();
        var manager = new DockWorkspaceManager(serializer);
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();
        factory.InitLayout(root);

        manager.TrackFactory(factory);
        var workspace = manager.Capture("Active", root, includeState: false);

        var events = new List<DockWorkspaceDirtyChangedEventArgs>();
        manager.WorkspaceDirtyChanged += (_, args) => events.Add(args);

        var dockable = factory.CreateDocument();
        dockable.Owner = root;

        factory.OnDockableMoved(dockable);

        Assert.Single(events);
        Assert.True(events[0].IsDirty);
        Assert.Same(workspace, events[0].Workspace);

        manager.MarkClean();

        Assert.Equal(2, events.Count);
        Assert.False(events[1].IsDirty);
        Assert.Same(workspace, events[1].Workspace);

        manager.MarkClean();

        Assert.Equal(2, events.Count);
    }

    [Fact]
    public void TrackFactory_Respects_Window_Move_Option()
    {
        var serializer = new StubDockSerializer();
        var manager = new DockWorkspaceManager(serializer);
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();
        factory.InitLayout(root);

        manager.TrackFactory(factory, new DockWorkspaceTrackingOptions
        {
            TrackWindowMoves = false
        });

        var window = new DockWindow { Layout = root };

        factory.OnWindowMoveDragEnd(window);

        Assert.False(manager.IsDirty);

        factory.OnWindowOpened(window);

        Assert.True(manager.IsDirty);
    }

    [Fact]
    public void TrackLayout_Ignores_Windows_Outside_Tracked_Root()
    {
        var serializer = new StubDockSerializer();
        var manager = new DockWorkspaceManager(serializer);
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();
        factory.InitLayout(root);

        var otherRoot = factory.CreateRootDock();
        otherRoot.VisibleDockables = factory.CreateList<IDockable>();
        factory.InitLayout(otherRoot);

        manager.TrackLayout(root);

        var window = new DockWindow { Layout = otherRoot };

        factory.OnWindowOpened(window);

        Assert.False(manager.IsDirty);

        window.Layout = root;
        factory.OnWindowOpened(window);

        Assert.True(manager.IsDirty);
    }

    [Fact]
    public void TrackLayout_Uses_Window_Owner_When_Layout_Is_Null()
    {
        var serializer = new StubDockSerializer();
        var manager = new DockWorkspaceManager(serializer);
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();
        factory.InitLayout(root);

        manager.TrackLayout(root);

        var dockable = factory.CreateDocument();
        dockable.Owner = root;

        var window = new DockWindow { Owner = dockable };

        factory.OnWindowOpened(window);

        Assert.True(manager.IsDirty);
    }
}
