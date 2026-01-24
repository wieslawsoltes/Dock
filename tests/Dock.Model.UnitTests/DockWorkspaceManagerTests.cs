using System;
using System.IO;
using Dock.Model;
using Dock.Model.Avalonia;
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
}
