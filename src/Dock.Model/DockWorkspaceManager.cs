// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Core.Events;

namespace Dock.Model;

/// <summary>
/// Manages named workspace snapshots for docking layouts.
/// </summary>
public sealed class DockWorkspaceManager
{
    private readonly IDockSerializer _serializer;
    private readonly Dictionary<string, DockWorkspace> _workspaces;
    private IFactory? _trackingFactory;
    private IRootDock? _trackedRoot;
    private DockWorkspaceTrackingOptions _trackingOptions;
    private bool _layoutDirty;
    private bool _lastReportedDirty;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockWorkspaceManager"/> class.
    /// </summary>
    /// <param name="serializer">The dock serializer.</param>
    public DockWorkspaceManager(IDockSerializer serializer)
    {
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _workspaces = new Dictionary<string, DockWorkspace>(StringComparer.OrdinalIgnoreCase);
        _trackingOptions = new DockWorkspaceTrackingOptions();
    }

    /// <summary>
    /// Gets the known workspaces.
    /// </summary>
    public IReadOnlyCollection<DockWorkspace> Workspaces => _workspaces.Values;

    /// <summary>
    /// Gets the most recently captured or restored workspace.
    /// </summary>
    public DockWorkspace? ActiveWorkspace { get; private set; }

    /// <summary>
    /// Gets whether the current layout has unsaved changes.
    /// </summary>
    public bool IsDirty => ActiveWorkspace?.IsDirty ?? _layoutDirty;

    /// <summary>
    /// Gets whether the manager is tracking layout changes.
    /// </summary>
    public bool IsTracking => _trackingFactory is not null;

    /// <summary>
    /// Gets the current tracking options.
    /// </summary>
    public DockWorkspaceTrackingOptions TrackingOptions => _trackingOptions;

    /// <summary>
    /// Raised when the dirty state changes.
    /// </summary>
    public event EventHandler<DockWorkspaceDirtyChangedEventArgs>? WorkspaceDirtyChanged;

    /// <summary>
    /// Captures a workspace snapshot.
    /// </summary>
    /// <param name="id">Workspace identifier.</param>
    /// <param name="layout">Layout to serialize.</param>
    /// <param name="includeState">Whether to capture dock state.</param>
    /// <param name="name">Optional workspace name.</param>
    /// <param name="description">Optional workspace description.</param>
    /// <returns>The captured workspace.</returns>
    public DockWorkspace Capture(string id, IDock layout, bool includeState = true, string? name = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Workspace id cannot be null or whitespace.", nameof(id));
        }

        if (layout is null)
        {
            throw new ArgumentNullException(nameof(layout));
        }

        DockState? state = null;
        if (includeState)
        {
            state = new DockState();
            state.Save(layout);
        }

        var payload = _serializer.Serialize(layout);

        if (_workspaces.TryGetValue(id, out var existing))
        {
            existing.Update(payload, state, name, description);
            SetActiveWorkspace(existing);
            MarkClean();
            return existing;
        }

        var workspace = new DockWorkspace(id, payload, state)
        {
            Name = string.IsNullOrWhiteSpace(name) ? id : name,
            Description = description
        };

        _workspaces[id] = workspace;
        SetActiveWorkspace(workspace);
        MarkClean();
        return workspace;
    }

    /// <summary>
    /// Gets a workspace by id.
    /// </summary>
    /// <param name="id">Workspace identifier.</param>
    /// <returns>The workspace or null.</returns>
    public DockWorkspace? Get(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        _workspaces.TryGetValue(id, out var workspace);
        return workspace;
    }

    /// <summary>
    /// Removes a workspace by id.
    /// </summary>
    /// <param name="id">Workspace identifier.</param>
    /// <returns>True when removed.</returns>
    public bool Remove(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        var removed = _workspaces.Remove(id);
        if (removed && ActiveWorkspace?.Id == id)
        {
            SetActiveWorkspace(null);
            MarkClean();
        }

        return removed;
    }

    /// <summary>
    /// Clears all workspaces.
    /// </summary>
    public void Clear()
    {
        _workspaces.Clear();
        SetActiveWorkspace(null);
        MarkClean();
    }

    /// <summary>
    /// Restores a layout from a workspace.
    /// </summary>
    /// <param name="workspace">Workspace to restore.</param>
    /// <returns>The restored layout, or null if deserialization fails.</returns>
    public IDock? Restore(DockWorkspace workspace)
    {
        if (workspace is null)
        {
            throw new ArgumentNullException(nameof(workspace));
        }

        var layout = _serializer.Deserialize<IDock>(workspace.Layout);
        if (layout is null)
        {
            return null;
        }

        workspace.State?.Restore(layout);
        SetActiveWorkspace(workspace);
        MarkClean();
        return layout;
    }

    /// <summary>
    /// Restores a layout by workspace id.
    /// </summary>
    /// <param name="id">Workspace identifier.</param>
    /// <param name="layout">The restored layout, when successful.</param>
    /// <returns>True when the restore succeeded.</returns>
    public bool TryRestore(string id, out IDock? layout)
    {
        layout = null;

        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        if (!_workspaces.TryGetValue(id, out var workspace))
        {
            return false;
        }

        layout = Restore(workspace);
        return layout is not null;
    }

    /// <summary>
    /// Starts tracking layout changes for a factory.
    /// </summary>
    /// <param name="factory">The factory to observe.</param>
    /// <param name="options">Optional tracking options.</param>
    public void TrackFactory(IFactory factory, DockWorkspaceTrackingOptions? options = null)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        if (ReferenceEquals(_trackingFactory, factory))
        {
            _trackingOptions = options ?? new DockWorkspaceTrackingOptions();
            return;
        }

        StopTracking();

        _trackingFactory = factory;
        _trackingOptions = options ?? new DockWorkspaceTrackingOptions();

        SubscribeFactory(factory);
    }

    /// <summary>
    /// Starts tracking layout changes for a layout and its factory.
    /// </summary>
    /// <param name="layout">The layout to track.</param>
    /// <param name="options">Optional tracking options.</param>
    public void TrackLayout(IDock layout, DockWorkspaceTrackingOptions? options = null)
    {
        if (layout is null)
        {
            throw new ArgumentNullException(nameof(layout));
        }

        if (layout.Factory is null)
        {
            throw new ArgumentException("Layout factory is not set.", nameof(layout));
        }

        TrackFactory(layout.Factory, options);
        _trackedRoot = layout as IRootDock ?? layout.Factory.FindRoot(layout);
    }

    /// <summary>
    /// Stops tracking layout changes.
    /// </summary>
    public void StopTracking()
    {
        if (_trackingFactory is null)
        {
            return;
        }

        UnsubscribeFactory(_trackingFactory);
        _trackingFactory = null;
        _trackedRoot = null;
        _trackingOptions = new DockWorkspaceTrackingOptions();
    }

    /// <summary>
    /// Marks the current layout as clean.
    /// </summary>
    public void MarkClean()
    {
        _layoutDirty = false;

        if (ActiveWorkspace is { } workspace)
        {
            workspace.SetDirty(false);
        }

        UpdateDirtyState();
    }

    private void SetActiveWorkspace(DockWorkspace? workspace)
    {
        ActiveWorkspace = workspace;
    }

    private void SubscribeFactory(IFactory factory)
    {
        factory.DockableAdded += OnDockableAdded;
        factory.DockableRemoved += OnDockableRemoved;
        factory.DockableClosed += OnDockableClosed;
        factory.DockableMoved += OnDockableMoved;
        factory.DockableDocked += OnDockableDocked;
        factory.DockableUndocked += OnDockableUndocked;
        factory.DockableSwapped += OnDockableSwapped;
        factory.DockablePinned += OnDockablePinned;
        factory.DockableUnpinned += OnDockableUnpinned;
        factory.DockableHidden += OnDockableHidden;
        factory.DockableRestored += OnDockableRestored;
        factory.WindowOpened += OnWindowOpened;
        factory.WindowClosed += OnWindowClosed;
        factory.WindowAdded += OnWindowAdded;
        factory.WindowRemoved += OnWindowRemoved;
        factory.WindowMoveDragEnd += OnWindowMoveDragEnd;
    }

    private void UnsubscribeFactory(IFactory factory)
    {
        factory.DockableAdded -= OnDockableAdded;
        factory.DockableRemoved -= OnDockableRemoved;
        factory.DockableClosed -= OnDockableClosed;
        factory.DockableMoved -= OnDockableMoved;
        factory.DockableDocked -= OnDockableDocked;
        factory.DockableUndocked -= OnDockableUndocked;
        factory.DockableSwapped -= OnDockableSwapped;
        factory.DockablePinned -= OnDockablePinned;
        factory.DockableUnpinned -= OnDockableUnpinned;
        factory.DockableHidden -= OnDockableHidden;
        factory.DockableRestored -= OnDockableRestored;
        factory.WindowOpened -= OnWindowOpened;
        factory.WindowClosed -= OnWindowClosed;
        factory.WindowAdded -= OnWindowAdded;
        factory.WindowRemoved -= OnWindowRemoved;
        factory.WindowMoveDragEnd -= OnWindowMoveDragEnd;
    }

    private void OnDockableAdded(object? sender, DockableAddedEventArgs args) => MarkDirty(args.Dockable);

    private void OnDockableRemoved(object? sender, DockableRemovedEventArgs args) => MarkDirty(args.Dockable);

    private void OnDockableClosed(object? sender, DockableClosedEventArgs args) => MarkDirty(args.Dockable);

    private void OnDockableMoved(object? sender, DockableMovedEventArgs args) => MarkDirty(args.Dockable);

    private void OnDockableDocked(object? sender, DockableDockedEventArgs args) => MarkDirty(args.Dockable);

    private void OnDockableUndocked(object? sender, DockableUndockedEventArgs args) => MarkDirty(args.Dockable);

    private void OnDockableSwapped(object? sender, DockableSwappedEventArgs args) => MarkDirty(args.Dockable);

    private void OnDockablePinned(object? sender, DockablePinnedEventArgs args) => MarkDirty(args.Dockable);

    private void OnDockableUnpinned(object? sender, DockableUnpinnedEventArgs args) => MarkDirty(args.Dockable);

    private void OnDockableHidden(object? sender, DockableHiddenEventArgs args) => MarkDirty(args.Dockable);

    private void OnDockableRestored(object? sender, DockableRestoredEventArgs args) => MarkDirty(args.Dockable);

    private void OnWindowOpened(object? sender, WindowOpenedEventArgs args) => MarkDirtyWindow(args.Window);

    private void OnWindowClosed(object? sender, WindowClosedEventArgs args) => MarkDirtyWindow(args.Window);

    private void OnWindowAdded(object? sender, WindowAddedEventArgs args) => MarkDirtyWindow(args.Window);

    private void OnWindowRemoved(object? sender, WindowRemovedEventArgs args) => MarkDirtyWindow(args.Window);

    private void OnWindowMoveDragEnd(object? sender, WindowMoveDragEndEventArgs args)
    {
        if (!_trackingOptions.TrackWindowMoves)
        {
            return;
        }

        MarkDirtyWindow(args.Window);
    }

    private void MarkDirty(IDockable? dockable)
    {
        if (!ShouldTrackDockable(dockable))
        {
            return;
        }

        if (ActiveWorkspace is { } workspace)
        {
            if (workspace.SetDirty(true))
            {
                UpdateDirtyState();
            }
        }
        else if (!_layoutDirty)
        {
            _layoutDirty = true;
            UpdateDirtyState();
        }
    }

    private void MarkDirtyWindow(IDockWindow? window)
    {
        if (!ShouldTrackWindow(window))
        {
            return;
        }

        if (ActiveWorkspace is { } workspace)
        {
            if (workspace.SetDirty(true))
            {
                UpdateDirtyState();
            }
        }
        else if (!_layoutDirty)
        {
            _layoutDirty = true;
            UpdateDirtyState();
        }
    }

    private bool ShouldTrackDockable(IDockable? dockable)
    {
        if (_trackingFactory is null)
        {
            return false;
        }

        if (_trackedRoot is not null && dockable is not null)
        {
            var root = _trackingFactory.FindRoot(dockable);
            if (root is not null && !ReferenceEquals(root, _trackedRoot))
            {
                return false;
            }
        }

        var filter = _trackingOptions.DockableFilter;
        if (filter is not null && !filter(dockable))
        {
            return false;
        }

        return true;
    }

    private bool ShouldTrackWindow(IDockWindow? window)
    {
        if (_trackingFactory is null)
        {
            return false;
        }

        if (_trackedRoot is not null && window is not null)
        {
            var root = window.Layout;
            if (root is null && window.Owner is { } owner)
            {
                root = _trackingFactory.FindRoot(owner);
            }

            if (root is not null && !ReferenceEquals(root, _trackedRoot))
            {
                return false;
            }
        }

        var filter = _trackingOptions.DockableFilter;
        if (filter is not null && !filter(window?.Owner))
        {
            return false;
        }

        return true;
    }

    private void UpdateDirtyState()
    {
        var isDirty = IsDirty;
        if (isDirty == _lastReportedDirty)
        {
            return;
        }

        _lastReportedDirty = isDirty;
        WorkspaceDirtyChanged?.Invoke(this, new DockWorkspaceDirtyChangedEventArgs(ActiveWorkspace, isDirty));
    }
}
