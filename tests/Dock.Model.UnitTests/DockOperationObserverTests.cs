using System;
using System.Collections.Generic;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class DockOperationObserverTests
{
    [Fact]
    public void Observer_Brackets_The_Actual_Move_And_Sees_Its_Ownership_Changes()
    {
        Workspace workspace = new();
        workspace.Factory.Before = () => Assert.Same(workspace.Source, workspace.Document.Owner);
        workspace.Factory.After = () => Assert.Same(workspace.Target, workspace.Document.Owner);
        Assert.True(workspace.Manager.ValidateDockable(workspace.Document, workspace.Target,
            DragAction.Move, DockOperation.Fill, true));
        Assert.Equal(new[] { "begin", "complete", "dispose" }, workspace.Factory.Trace);
        Assert.Same(workspace.Document, workspace.Factory.ObservedSource);
        Assert.Same(workspace.Target, workspace.Factory.ObservedTarget);
        Assert.Equal(DockOperation.Fill, workspace.Factory.ObservedOperation);
    }

    [Fact]
    public void Validation_And_Rejected_Execution_Do_Not_Open_An_Observer_Scope()
    {
        Workspace workspace = new();
        Assert.True(workspace.Manager.ValidateDockable(workspace.Document, workspace.Target,
            DragAction.Move, DockOperation.Fill, false));
        Assert.Empty(workspace.Factory.Trace);
        Assert.Same(workspace.Source, workspace.Document.Owner);
        workspace.Document.CanFloat = false;
        Assert.False(workspace.Manager.ValidateDockable(workspace.Document, workspace.Target,
            DragAction.Move, DockOperation.Window, true));
        Assert.Empty(workspace.Factory.Trace);
    }

    [Fact]
    public void Execution_Failure_Disposes_The_Scope_Without_Completing_It()
    {
        Workspace workspace = new();
        workspace.Factory.ThrowOnMove = true;
        Assert.Throws<InvalidOperationException>(() => workspace.Manager.ValidateDockable(workspace.Document,
            workspace.Target, DragAction.Move, DockOperation.Fill, true));
        Assert.Equal(new[] { "begin", "dispose" }, workspace.Factory.Trace);
        workspace.Factory.Trace.Clear();
        workspace.Factory.ThrowOnMove = false;
        Assert.True(workspace.Manager.ValidateDockable(workspace.Document, workspace.Target,
            DragAction.Move, DockOperation.Fill, true));
        Assert.Equal(new[] { "begin", "complete", "dispose" }, workspace.Factory.Trace);
    }

    [Fact]
    public void Execution_Rechecks_Policies_Changed_By_An_Observer()
    {
        Workspace workspace = new();
        workspace.Factory.Before = () => workspace.Manager.IsDockingEnabled = false;
        Assert.False(workspace.Manager.ValidateDockable(workspace.Document, workspace.Target,
            DragAction.Move, DockOperation.Fill, true));
        Assert.Equal(new[] { "begin", "dispose" }, workspace.Factory.Trace);
        Assert.Same(workspace.Source, workspace.Document.Owner);
    }

    [Fact]
    public void An_Observer_Can_Decline_To_Create_A_Scope()
    {
        Workspace workspace = new();
        workspace.Factory.OmitScope = true;
        Assert.True(workspace.Manager.ValidateDockable(workspace.Document, workspace.Target,
            DragAction.Move, DockOperation.Fill, true));
        Assert.Equal(new[] { "begin" }, workspace.Factory.Trace);
        Assert.Same(workspace.Target, workspace.Document.Owner);
    }

    private sealed class RecordingFactory : Factory, IDockOperationObserver
    {
        public List<string> Trace { get; } = new();
        public Action? Before { get; set; }
        public Action? After { get; set; }
        public bool ThrowOnMove { get; set; }
        public bool OmitScope { get; set; }
        public IDockable? ObservedSource { get; private set; }
        public IDockable? ObservedTarget { get; private set; }
        public DockOperation ObservedOperation { get; private set; }

        public IDockOperationScope? BeginDockOperation(IDockable source, IDockable target, DockOperation operation)
        {
            ObservedSource = source;
            ObservedTarget = target;
            ObservedOperation = operation;
            Trace.Add("begin");
            Before?.Invoke();
            return OmitScope ? null : new Scope(this);
        }

        public override void MoveDockable(IDock sourceDock, IDock targetDock, IDockable sourceDockable, IDockable? targetDockable)
        {
            if (ThrowOnMove) throw new InvalidOperationException("Execution failed.");
            base.MoveDockable(sourceDock, targetDock, sourceDockable, targetDockable);
        }

        private sealed class Scope(RecordingFactory factory) : IDockOperationScope
        {
            public void Complete() { factory.Trace.Add("complete"); factory.After?.Invoke(); }
            public void Dispose() => factory.Trace.Add("dispose");
        }
    }

    private sealed class Workspace
    {
        public RecordingFactory Factory { get; } = new();
        public DockManager Manager { get; } = new(new DockService());
        public Document Document { get; } = new() { Id = "source" };
        public DocumentDock Source { get; }
        public DocumentDock Target { get; }

        public Workspace()
        {
            Source = new() { IsCollapsable = false, VisibleDockables = Factory.CreateList<IDockable>(Document) };
            Target = new() { IsCollapsable = false, VisibleDockables = Factory.CreateList<IDockable>(new Document { Id = "target" }) };
            ProportionalDock panel = new() { VisibleDockables = Factory.CreateList<IDockable>(Source, Target) };
            RootDock root = new() { VisibleDockables = Factory.CreateList<IDockable>(panel) };
            Factory.InitLayout(root);
        }
    }
}
