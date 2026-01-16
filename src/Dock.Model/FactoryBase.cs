// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using Dock.Model.Adapters;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Factory base class.
/// </summary>
public abstract partial class FactoryBase : IFactory
{
    private readonly IOverlayAdapter _overlayAdapter;

    /// <summary>
    /// Initializes new instance of the <see cref="FactoryBase"/> class.
    /// </summary>
    protected FactoryBase()
    {
        _overlayAdapter = new OverlayAdapter();
    }

    /// <inheritdoc/>
    public IOverlayAdapter OverlayAdapter => _overlayAdapter;

    private static void CopyDockGroup(IDockable source, IDockable target)
    {
        var group = DockGroupValidator.GetEffectiveDockGroup(source);
        if (!string.IsNullOrEmpty(group))
        {
            target.DockGroup = group;
        }
    }

    private bool IsDockPinned(IList<IDockable>? pinnedDockables, IDock dock)
    {
        if (pinnedDockables is not null && pinnedDockables.Count != 0)
        {
            foreach (var pinnedDockable in pinnedDockables)
            {
                if (pinnedDockable.Owner == dock)
                {
                    return true;
                }
            }
            return true;
        }
        return false;
    }

    private void CleanupProportionalDockTree(IProportionalDock dock)
    {
        if (dock.VisibleDockables == null || dock.Owner is not IDock owner || dock.Owner is IRootDock)
            return;

        // Check if this dock has only one visible dockable
        if (dock.VisibleDockables.Count == 1)
        {
            var singleDockable = dock.VisibleDockables[0];

            // Get the index where the current dock is in the owner's collection
            if (owner.VisibleDockables != null)
            {
                var dockIndex = owner.VisibleDockables.IndexOf(dock);
                if (dockIndex >= 0)
                {
                    // Preserve the proportion from the dock being collapsed
                    if (singleDockable is IDock singleDock && !double.IsNaN(dock.Proportion))
                    {
                        singleDock.Proportion = dock.Proportion;
                    }

                    // Remove the current dock from owner
                    owner.VisibleDockables.RemoveAt(dockIndex);

                    // Insert the single dockable at the same position
                    owner.VisibleDockables.Insert(dockIndex, singleDockable);

                    // Update the parent reference
                    singleDockable.Owner = owner;

                    // Initialize the dockable in its new owner
                    InitDockable(singleDockable, owner);

                    // Continue cleanup up the tree if the owner is also a proportional dock
                    if (owner is IProportionalDock proportionalOwner)
                    {
                        CleanupProportionalDockTree(proportionalOwner);
                    }
                }
            }
        }
    }

    /// <inheritdoc/>
    public virtual void CollapseDock(IDock dock)
    {
        // Preconditions: must be collapsable and currently empty.
        if (dock is null) return;
        if (!dock.IsCollapsable) return;
        if (dock is ISplitViewDock splitViewDock)
        {
            if (dock.VisibleDockables is not null && dock.VisibleDockables.Count != 0) return;
            if (!IsSplitViewDockContentEmpty(splitViewDock)) return;
        }
        else
        {
            if (dock.VisibleDockables is null) return; // nothing to evaluate
            if (dock.VisibleDockables.Count != 0) return; // only collapse when empty
        }

        // Prevent collapsing pinned tool docks.
        var rootDock = FindRoot(dock, _ => true);
        if (rootDock is IRootDock root && dock is IToolDock toolDock)
        {
            switch (toolDock.Alignment)
            {
                case Alignment.Left:
                    if (IsDockPinned(root.LeftPinnedDockables, dock)) return;
                    break;
                case Alignment.Right:
                    if (IsDockPinned(root.RightPinnedDockables, dock)) return;
                    break;
                case Alignment.Top:
                    if (IsDockPinned(root.TopPinnedDockables, dock)) return;
                    break;
                case Alignment.Bottom:
                    if (IsDockPinned(root.BottomPinnedDockables, dock)) return;
                    break;
            }
        }

        var ownerDock = dock.Owner as IDock;
        var ownerProportional = dock.Owner as IProportionalDock;
        bool wasChildOfOwner = ownerDock?.VisibleDockables?.Contains(dock) == true;

        // If the dock is actually present in its owner list, adjust neighboring splitters.
        if (wasChildOfOwner && ownerDock?.VisibleDockables is { })
        {
            var list = ownerDock.VisibleDockables;
            int dockIndex = list.IndexOf(dock);
            if (dockIndex >= 0)
            {
                IProportionalDockSplitter? previousSplitter = null;
                IProportionalDockSplitter? nextSplitter = null;

                int prevIndex = dockIndex - 1;
                if (prevIndex >= 0 && list[prevIndex] is IProportionalDockSplitter ps)
                {
                    previousSplitter = ps;
                }
                int nextIndex = dockIndex + 1;
                if (nextIndex < list.Count && list[nextIndex] is IProportionalDockSplitter ns)
                {
                    nextSplitter = ns;
                }

                // Remove at most one splitter if two flank the collapsing dock; prefer removing the previous one.
                if (previousSplitter is not null && nextSplitter is not null)
                {
                    RemoveDockable(previousSplitter, true);
                }
                else if (previousSplitter is not null)
                {
                    RemoveDockable(previousSplitter, true);
                }
                else if (nextSplitter is not null)
                {
                    RemoveDockable(nextSplitter, true);
                }
            }
        }

        // If this is a root dock with a window, remove its window and stop.
        if (dock is IRootDock rootWithWindow && rootWithWindow.Window is { })
        {
            RemoveWindow(rootWithWindow.Window);
            return;
        }

        // Remove the dock itself only if it was actually attached to its owner.
        if (wasChildOfOwner)
        {
            RemoveDockable(dock, true);
        }

        // Decide if proportional cleanup should occur: only when <=1 non-splitter remains.
        if (ownerProportional is not null)
        {
            var list = ownerProportional.VisibleDockables;
            if (list is not null)
            {
                int nonSplitterCount = 0;
                foreach (var d in list)
                {
                    if (d is not IProportionalDockSplitter)
                    {
                        nonSplitterCount++;
                        if (nonSplitterCount > 1) break;
                    }
                }

                if (nonSplitterCount <= 1)
                {
                    CleanupProportionalDockTree(ownerProportional);
                    CleanupOrphanedSplitters(ownerProportional);
                }
            }
        }
    }

    /// <inheritdoc/>
    public virtual IDock CreateSplitLayout(IDock dock, IDockable dockable, DockOperation operation)
    {
        IDock? split;

        if (dockable is IDock dockableDock)
        {
            split = dockableDock;
        }
        else
        {
            split = CreateProportionalDock();
            split.Title = nameof(IProportionalDock);
            split.VisibleDockables = CreateList<IDockable>();
            CopyDockGroup(dockable, split);
            if (split.VisibleDockables is not null)
            {
                AddVisibleDockable(split, dockable);
                OnDockableAdded(dockable);
                split.ActiveDockable = dockable;
            }
        }

        var containerProportion = dock.Proportion;
        dock.Proportion = double.NaN;

        var layout = CreateProportionalDock();
        layout.Title = nameof(IProportionalDock);
        layout.VisibleDockables = CreateList<IDockable>();
        layout.Proportion = containerProportion;
    CopyDockGroup(dock, layout);

        var splitter = CreateProportionalDockSplitter();
        splitter.Title = nameof(IProportionalDockSplitter);

        switch (operation)
        {
            case DockOperation.Left:
            case DockOperation.Right:
            {
                layout.Orientation = Orientation.Horizontal;
                break;
            }
            case DockOperation.Top:
            case DockOperation.Bottom:
            {
                layout.Orientation = Orientation.Vertical;
                break;
            }
        }

        switch (operation)
        {
            case DockOperation.Left:
            case DockOperation.Top:
            {
                if (layout.VisibleDockables is not null)
                {
                    AddVisibleDockable(layout, split);
                    OnDockableAdded(split);
                    layout.ActiveDockable = split;
                }

                break;
            }
            case DockOperation.Right:
            case DockOperation.Bottom:
            {
                if (layout.VisibleDockables is not null)
                {
                    AddVisibleDockable(layout, dock);
                    OnDockableAdded(dock);
                    layout.ActiveDockable = dock;
                }

                break;
            }
        }

        AddVisibleDockable(layout, splitter);
        OnDockableAdded(splitter);

        switch (operation)
        {
            case DockOperation.Left:
            case DockOperation.Top:
            {
                if (layout.VisibleDockables is not null)
                {
                    AddVisibleDockable(layout, dock);
                    OnDockableAdded(dock);
                    layout.ActiveDockable = dock;
                }

                break;
            }
            case DockOperation.Right:
            case DockOperation.Bottom:
            {
                if (layout.VisibleDockables is not null)
                {
                    AddVisibleDockable(layout, split);
                    OnDockableAdded(split);
                    layout.ActiveDockable = split;
                }

                break;
            }
        }

        return layout;
    }

    /// <inheritdoc/>
    public virtual void SplitToDock(IDock dock, IDockable dockable, DockOperation operation)
    {
        switch (operation)
        {
            case DockOperation.Left:
            case DockOperation.Right:
            case DockOperation.Top:
            case DockOperation.Bottom:
            {
                if (dock.Owner is IDock ownerDock && ownerDock.VisibleDockables is { })
                {
                    var index = ownerDock.VisibleDockables.IndexOf(dock);
                    if (index >= 0)
                    {
                        // Check if the owner is already a ProportionalDock with compatible orientation
                        if (ownerDock is IProportionalDock proportionalOwner)
                        {
                            var requiredOrientation = operation switch
                            {
                                DockOperation.Left or DockOperation.Right => Orientation.Horizontal,
                                DockOperation.Top or DockOperation.Bottom => Orientation.Vertical,
                                _ => throw new NotSupportedException($"Not supported split operation: {operation}.")
                            };

                            // If the owner already has the required orientation, add directly to it
                            if (proportionalOwner.Orientation == requiredOrientation)
                            {
                                IDock? split;

                                if (dockable is IDock dockableDock)
                                {
                                    split = dockableDock;
                                }
                                else
                                {
                                    split = CreateProportionalDock();
                                    split.Title = nameof(IProportionalDock);
                                    split.VisibleDockables = CreateList<IDockable>();
                                    if (split.VisibleDockables is not null)
                                    {
                                        AddVisibleDockable(split, dockable);
                                        OnDockableAdded(dockable);
                                        split.ActiveDockable = dockable;
                                    }
                                }

                                var splitter = CreateProportionalDockSplitter();
                                splitter.Title = nameof(IProportionalDockSplitter);

                                // Store the original dock's proportion and split it equally
                                var originalProportion = dock.Proportion;
                                var halfProportion = double.IsNaN(originalProportion) ? double.NaN : originalProportion / 2.0;
                                dock.Proportion = halfProportion;
                                split.Proportion = halfProportion;

                                switch (operation)
                                {
                                    case DockOperation.Left:
                                    case DockOperation.Top:
                                    {
                                        // Insert split before dock
                                        InsertVisibleDockable(proportionalOwner, index, split);
                                        OnDockableAdded(split);
                                        InsertVisibleDockable(proportionalOwner, index + 1, splitter);
                                        OnDockableAdded(splitter);
                                        InitDockable(split, proportionalOwner);
                                        break;
                                    }
                                    case DockOperation.Right:
                                    case DockOperation.Bottom:
                                    {
                                        // Insert split after dock
                                        InsertVisibleDockable(proportionalOwner, index + 1, splitter);
                                        OnDockableAdded(splitter);
                                        InsertVisibleDockable(proportionalOwner, index + 2, split);
                                        OnDockableAdded(split);
                                        InitDockable(split, proportionalOwner);
                                        break;
                                    }
                                }

                                // Clean up any orphaned splitters that might have been created
                                CleanupOrphanedSplitters(proportionalOwner);

                                OnDockableUndocked(dockable, operation);
                                OnDockableDocked(dockable, operation);
                                return;
                            }
                        }

                        // Fallback to the original behavior when optimization is not applicable
                        var layout = CreateSplitLayout(dock, dockable, operation);
                        RemoveVisibleDockableAt(ownerDock, index);
                        OnDockableRemoved(dockable);
                        OnDockableUndocked(dockable, operation);
                        InsertVisibleDockable(ownerDock, index, layout);
                        OnDockableAdded(dockable);
                        ownerDock.ActiveDockable = layout;
                        InitDockable(layout, ownerDock);
                        OnDockableDocked(dockable, operation);
                    }
                }
                break;
            }
            default:
                throw new NotSupportedException($"Not supported split operation: {operation}.");
        }
    }

    /// <inheritdoc/>
    public virtual IDockWindow? CreateWindowFrom(IDockable dockable)
    {
        IDockable? target;

        switch (dockable)
        {
            case ITool:
            {
                target = CreateToolDock();
                target.Title = nameof(IToolDock);
                CopyDockGroup(dockable, target);
                if (target is IDock dock)
                {
                    dock.VisibleDockables = CreateList<IDockable>();
                    if (dock.VisibleDockables is not null)
                    {
                        AddVisibleDockable(dock, dockable);
                        OnDockableAdded(dockable);
                        dock.ActiveDockable = dockable;
                    }
                }
                break;
            }
            case IDocument:
            {
                target = CreateDocumentDock();
                target.Title = nameof(IDocumentDock);
                CopyDockGroup(dockable, target);
                if (target is IDock dock)
                {
                    dock.VisibleDockables = CreateList<IDockable>();
                    if (dockable.Owner is IDocumentDock sourceDocumentDock)
                    {
                        if (target is IDocumentDock targetDocumentDock)
                        {
                            targetDocumentDock.Id = sourceDocumentDock.Id;
                            targetDocumentDock.CanCreateDocument = sourceDocumentDock.CanCreateDocument;
                            targetDocumentDock.EnableWindowDrag = sourceDocumentDock.EnableWindowDrag;

                            if (sourceDocumentDock is IDocumentDockContent sourceDocumentDockContent
                                && targetDocumentDock is IDocumentDockContent targetDocumentDockContent)
                            {
                                
                                targetDocumentDockContent.DocumentTemplate = sourceDocumentDockContent.DocumentTemplate;
                            }
                        }
                    }
                    if (dock.VisibleDockables is not null)
                    {
                        AddVisibleDockable(dock, dockable);
                        OnDockableAdded(dockable);
                        dock.ActiveDockable = dockable;
                    }
                }
                break;
            }
            case IToolDock:
            {
                target = dockable;
                break;
            }
            case IDocumentDock:
            {
                target = dockable;
                break;
            }
            case IProportionalDock proportionalDock:
            {
                target = proportionalDock;
                break;
            }
            case IDockDock dockDock:
            {
                target = dockDock;
                break;
            }
            case IRootDock rootDock:
            {
                target = rootDock.ActiveDockable;
                break;
            }
            case IDock dock:
            {
                target = dock;
                break;
            }
            default:
            {
                return null;
            }
        }

        var currentRoot = FindRoot(dockable);
        var root = CreateRootDock();

        if (currentRoot != null)
        {
            root.EnableAdaptiveGlobalDockTargets = currentRoot.EnableAdaptiveGlobalDockTargets;
        }
        
        
        root.Title = nameof(IRootDock);
        root.VisibleDockables = CreateList<IDockable>();
        if (root.VisibleDockables is not null && target is not null)
        {
            AddVisibleDockable(root, target);
            OnDockableAdded(target);
            root.ActiveDockable = target;
            root.DefaultDockable = target;
        }
        root.Owner = null;

        var window = CreateDockWindow();
        window.Id = nameof(IDockWindow);
        window.Title = "";
        window.Width = double.NaN;
        window.Height = double.NaN;
        window.Layout = root;

        root.Window = window;

        return window;
    }

    /// <inheritdoc/>
    public virtual void SplitToWindow(IDock dock, IDockable dockable, double x, double y, double width, double height)
    {
        var rootDock = FindRoot(dock, _ => true);
        if (rootDock is null)
        {
            return;
        }

        RemoveDockable(dockable, true);
        OnDockableUndocked(dockable, DockOperation.Window);

        var window = CreateWindowFrom(dockable);
        if (window is not null)
        {
            AddWindow(rootDock, window);
            window.X = x;
            window.Y = y;
            window.Width = width;
            window.Height = height;
            window.Present(false);

            OnDockableDocked(dockable, DockOperation.Window);

            if (window.Layout is { })
            {
                SetFocusedDockable(window.Layout, dockable);
            }
        }
    }
}
