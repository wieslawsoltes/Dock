// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Factory base class.
/// </summary>
public abstract partial class FactoryBase : IFactory
{
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

    /// <inheritdoc/>
    public virtual void CollapseDock(IDock dock)
    {
        if (!dock.IsCollapsable || dock.VisibleDockables is null || dock.VisibleDockables.Count != 0)
        {
            return;
        }

        var rootDock = FindRoot(dock, _ => true);
        if (rootDock is { })
        {
            if (dock is IToolDock toolDock)
            {
                if (toolDock.Alignment == Alignment.Left 
                    && IsDockPinned(rootDock.LeftPinnedDockables, dock))
                {
                    return;
                }

                if (toolDock.Alignment == Alignment.Right 
                    && IsDockPinned(rootDock.RightPinnedDockables, dock))
                {
                    return;
                }

                if (toolDock.Alignment == Alignment.Top 
                    && IsDockPinned(rootDock.TopPinnedDockables, dock))
                {
                    return;
                }

                if (toolDock.Alignment == Alignment.Bottom 
                    && IsDockPinned(rootDock.BottomPinnedDockables, dock))
                {
                    return;
                }
            }
        }

        if (dock.Owner is IDock ownerDock && ownerDock.VisibleDockables is { })
        {
            var toRemove = new List<IDockable>();
            var dockIndex = ownerDock.VisibleDockables.IndexOf(dock);

            if (dockIndex >= 0)
            {
                var indexSplitterPrevious = dockIndex - 1;
                if (dockIndex > 0 && indexSplitterPrevious >= 0)
                {
                    var previousVisible = ownerDock.VisibleDockables[indexSplitterPrevious];
                    if (previousVisible is IProportionalDockSplitter splitterPrevious)
                    {
                        toRemove.Add(splitterPrevious);
                    }
                }

                var indexSplitterNext = dockIndex + 1;
                if (dockIndex < ownerDock.VisibleDockables.Count - 1 && indexSplitterNext >= 0)
                {
                    var nextVisible = ownerDock.VisibleDockables[indexSplitterNext];
                    if (nextVisible is IProportionalDockSplitter splitterNext)
                    {
                        toRemove.Add(splitterNext);
                    }
                }

                foreach (var removeVisible in toRemove)
                {
                    RemoveDockable(removeVisible, true);
                }
            }
        }

        if (dock is IRootDock rootDockDock && rootDockDock.Window is { })
        {
            RemoveWindow(rootDockDock.Window);
        }
        else
        {
            RemoveDockable(dock, true);
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
            if (split.VisibleDockables is not null)
            {
                AddVisibleDockable(split, dockable);
                OnDockableAdded(dockable);
                split.ActiveDockable = dockable;
            }
        }

        var layout = CreateProportionalDock();
        layout.Title = nameof(IProportionalDock);
        layout.VisibleDockables = CreateList<IDockable>();
        
        // Use the new overridable method for copying properties
        CopyPropertiesForSplitDock(dock, layout, operation);

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

                                // Use the new overridable method for copying properties
                                CopyPropertiesForSplitDock(dock, split, operation, isNestedLayout: true);

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
                        InitDockable(layout, ownerDock);
                        ownerDock.ActiveDockable = layout;
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
                if (target is IDock dock)
                {
                    dock.VisibleDockables = CreateList<IDockable>();
                    if (dockable.Owner is IDocumentDock sourceDocumentDock)
                    {
                        if (target is IDocumentDock targetDocumentDock)
                        {
                            // Note: Window creation and property copying is handled later in CreateWindowFrom
                            // This section preserves the target dock for later use
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
            default:
            {
                return null;
            }
        }

        var root = CreateRootDock();
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

        // Allow customization of property copying for floating windows
        if (target is IDock targetDock)
        {
            CopyPropertiesForFloatingWindow(dockable, window, targetDock);
        }

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
            
            // Use the new overridable method for copying dimension properties
            CopyDimensionProperties(dockable, window, x, y, width, height);
            window.Present(false);

            OnDockableDocked(dockable, DockOperation.Window);

            if (window.Layout is { })
            {
                SetFocusedDockable(window.Layout, dockable);
            }
        }
    }

    /// <inheritdoc/>
    public virtual void CopyDockableProperties(IDockable source, IDockable target, DockOperation operation)
    {
        // Default implementation - no properties copied
        // Override in derived factories to customize property copying behavior
    }

    /// <inheritdoc/>
    public virtual void CopyDockProperties(IDock source, IDock target, DockOperation operation)
    {
        // Default implementation - no properties copied
        // Override in derived factories to customize property copying behavior
    }

    /// <inheritdoc/>
    public virtual void CopyPropertiesForSplitDock(IDock source, IDock newDock, DockOperation operation, bool isNestedLayout = false)
    {
        // Default implementation - only copy basic properties that are currently copied
        // This maintains backward compatibility while allowing customization
        
        // Copy the proportion as currently done in the original code
        if (operation == DockOperation.Left || operation == DockOperation.Right ||
            operation == DockOperation.Top || operation == DockOperation.Bottom)
        {
            var originalProportion = source.Proportion;
            
            if (isNestedLayout)
            {
                // For nested layouts: both source and target get half the proportion
                var halfProportion = double.IsNaN(originalProportion) ? double.NaN : originalProportion / 2.0;
                source.Proportion = halfProportion;
                newDock.Proportion = halfProportion;
            }
            else
            {
                // For root split layout: source becomes NaN, newDock gets original
                source.Proportion = double.NaN;
                newDock.Proportion = originalProportion;
            }
        }
    }

    /// <inheritdoc/>
    public virtual void CopyPropertiesForFloatingWindow(IDockable source, IDockWindow window, IDock targetDock)
    {
        // Default implementation - copy basic window properties
        // Override to customize window property copying
        
        if (source.Owner is IDocumentDock sourceDocumentDock && targetDock is IDocumentDock targetDocumentDock)
        {
            // Copy document dock specific properties as currently done in CreateWindowFrom
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

    /// <inheritdoc/>
    public virtual void CopyDimensionProperties(IDockable source, object target, double x, double y, double width, double height)
    {
        // Default implementation - copy dimension properties for window operations
        // Override to customize dimension copying behavior
        
        if (target is IDockWindow window)
        {
            window.X = x;
            window.Y = y;
            window.Width = width;
            window.Height = height;
        }
    }
}
