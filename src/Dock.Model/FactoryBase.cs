// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using Dock.Model.Controls;

namespace Dock.Model
{
    /// <summary>
    /// Factory base class.
    /// </summary>
    public abstract class FactoryBase : IFactory
    {
        /// <inheritdoc/>
        public virtual IDictionary<string, Func<object>> ContextLocator { get; set; }

        /// <inheritdoc/>
        public virtual IDictionary<string, Func<IHostWindow>> HostLocator { get; set; }

        /// <inheritdoc/>
        public virtual IDictionary<string, Func<IDockable>> DockableLocator { get; set; }

        /// <inheritdoc/>
        public abstract IList<T> CreateList<T>(params T[] items);

        /// <inheritdoc/>
        public abstract IRootDock CreateRootDock();

        /// <inheritdoc/>
        public abstract IPinDock CreatePinDock();

        /// <inheritdoc/>
        public abstract IProportionalDock CreateProportionalDock();

        /// <inheritdoc/>
        public abstract ISplitterDock CreateSplitterDock();

        /// <inheritdoc/>
        public abstract IToolDock CreateToolDock();

        /// <inheritdoc/>
        public abstract IDocumentDock CreateDocumentDock();

        /// <inheritdoc/>
        public abstract IDockWindow CreateDockWindow();

        /// <inheritdoc/>
        public abstract IDock CreateLayout();

        /// <inheritdoc/>
        public virtual void InitLayout(IDockable layout)
        {
            UpdateDockable(layout, null);
            if (layout is IDock root)
            {
                if (root.DefaultDockable != null)
                {
                    root.ActiveDockable = root.DefaultDockable;
                }
                root.ShowWindows();
            }
        }

        /// <inheritdoc/>
        public virtual object GetContext(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                Func<object> locator = null;
                if (ContextLocator?.TryGetValue(id, out locator) == true)
                {
                    return locator?.Invoke();
                }
            }
            return null;
        }

        /// <inheritdoc/>
        public virtual IHostWindow GetHostWindow(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                Func<IHostWindow> locator = null;
                if (HostLocator?.TryGetValue(id, out locator) == true)
                {
                    return locator?.Invoke();
                }
            }
            return null; // Dock host window with provided id is not registered.
        }

        /// <inheritdoc/>
        public virtual void UpdateDockable(IDockWindow window, IDockable owner)
        {
            window.Host = GetHostWindow(window.Id);
            if (window.Host != null)
            {
                window.Host.Window = window;
            }

            window.Owner = owner;
            window.Factory = this;

            if (window.Layout != null)
            {
                UpdateDockable(window.Layout, window.Layout.Owner);
            }
        }

        /// <inheritdoc/>
        public virtual void UpdateDockable(IDockable dockable, IDockable owner)
        {
            if (GetContext(dockable.Id) is object context)
            {
                dockable.Context = context;
            }

            dockable.Owner = owner;

            if (dockable is IDock dock)
            {
                dock.Factory = this;

                if (dock.VisibleDockables != null)
                {
                    foreach (var child in dock.VisibleDockables)
                    {
                        UpdateDockable(child, dockable);
                    }
                }

                if (dock.Windows != null)
                {
                    foreach (var child in dock.Windows)
                    {
                        UpdateDockable(child, dockable);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public virtual void AddDockable(IDock dock, IDockable dockable)
        {
            UpdateDockable(dockable, dock);
            if (dock.VisibleDockables == null)
            {
                dock.VisibleDockables = CreateList<IDockable>();
            }
            dock.VisibleDockables.Add(dockable);
        }

        /// <inheritdoc/>
        public virtual void InsertDockable(IDock dock, IDockable dockable, int index)
        {
            if (index >= 0)
            {
                UpdateDockable(dockable, dock);
                if (dock.VisibleDockables == null)
                {
                    dock.VisibleDockables = CreateList<IDockable>();
                }
                dock.VisibleDockables.Insert(index, dockable);
            }
        }

        /// <inheritdoc/>
        public virtual void AddWindow(IDock dock, IDockWindow window)
        {
            if (dock.Windows == null)
            {
                dock.Windows = CreateList<IDockWindow>();
            }
            dock.Windows.Add(window);
            UpdateDockable(window, dock);
        }

        /// <inheritdoc/>
        public virtual void RemoveWindow(IDockWindow window)
        {
            if (window?.Owner is IDock dock)
            {
                window.Exit();
                dock.Windows?.Remove(window);
            }
        }

        /// <inheritdoc/>
        public virtual void SetActiveDockable(IDockable dockable)
        {
            if (dockable.Owner is IDock dock)
            {
                dock.ActiveDockable = dockable;

                dockable.OnSelected();
            }
        }

        private void SetIsActive(IDockable dockable, bool active)
        {
            if (dockable is IDock dock)
            {
                dock.IsActive = active;
            }
        }

        /// <inheritdoc />
        public void SetFocusedDockable(IDock dock, IDockable dockable)
        {
            if (dock.ActiveDockable != null && FindRoot(dock.ActiveDockable) is IRootDock root)
            {
                if (root.FocusedDockable != null)
                {
                    SetIsActive(root.FocusedDockable.Owner, false);
                }

                if (dockable != null)
                {
                    root.FocusedDockable = dockable;
                }

                if (root.FocusedDockable != null)
                {
                    SetIsActive(root.FocusedDockable.Owner, true);
                }
            }
        }

        /// <inheritdoc/>
        public virtual IRootDock FindRoot(IDockable dockable)
        {
            if (dockable?.Owner == null)
            {
                return null;
            }
            if (dockable.Owner is IRootDock rootDock)
            {
                return rootDock;
            }
            return FindRoot(dockable.Owner);
        }

        /// <inheritdoc/>
        public virtual IDockable FindDockable(IDock dock, Func<IDockable, bool> predicate)
        {
            if (predicate(dock) == true)
            {
                return dock;
            }

            if (dock.VisibleDockables != null)
            {
                foreach (var dockable in dock.VisibleDockables)
                {
                    if (predicate(dockable) == true)
                    {
                        return dockable;
                    }

                    if (dockable is IDock childDock)
                    {
                        var result = FindDockable(childDock, predicate);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }

            if (dock.Windows != null)
            {
                foreach (var window in dock.Windows)
                {
                    if (window.Layout != null)
                    {
                        if (predicate(window.Layout) == true)
                        {
                            return window.Layout;
                        }

                        var result = FindDockable(window.Layout, predicate);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }

            return null;
        }

        private IPinDock PinClosestPinDock(IRootDock root, IDockable dockable)
        {
            // TODO: Find closest pin dock to the dockable (Top, Bottom, Left or Right).

            if (root.Left == null)
            {
                root.Left = CreatePinDock();
                root.Left.Alignment = Alignment.Left;
                root.Left.IsExpanded = false;
                root.Left.AutoHide = true;
            }

            return root.Left;
        }

        private void PinDockable(IPinDock pinDock, IDockable dockable)
        {
            RemoveDockable(dockable, true);

            if (pinDock.VisibleDockables == null)
            {
                pinDock.VisibleDockables = CreateList<IDockable>();
            }

            pinDock.VisibleDockables.Add(dockable);
            pinDock.ActiveDockable = dockable;
        }

        /// <inheritdoc/>
        public virtual void PinDockable(IDockable dockable)
        {
            // TODO: Implement dockable pinning.

            if (dockable != null && FindRoot(dockable) is IRootDock root)
            {
                if (PinClosestPinDock(root, dockable) is IPinDock pinDock)
                {
                    PinDockable(pinDock, dockable);
                }
            }
        }

        private void Collapse(IDock dock)
        {
            if (dock.IsCollapsable && dock.VisibleDockables.Count == 0)
            {
                if (dock.Owner is IDock ownerDock)
                {
                    var toRemove = new List<IDockable>();
                    var dockIndex = ownerDock.VisibleDockables.IndexOf(dock);

                    if (dockIndex >= 0)
                    {
                        int indexSplitterPrevious = dockIndex - 1;
                        if (dockIndex > 0 && indexSplitterPrevious >= 0)
                        {
                            var previousVisible = ownerDock.VisibleDockables[indexSplitterPrevious];
                            if (previousVisible is ISplitterDock splitterPrevious)
                            {
                                toRemove.Add(splitterPrevious);
                            }
                        }

                        int indexSplitterNext = dockIndex + 1;
                        if (dockIndex < ownerDock.VisibleDockables.Count - 1 && indexSplitterNext >= 0)
                        {
                            var nextVisible = ownerDock.VisibleDockables[indexSplitterNext];
                            if (nextVisible is ISplitterDock splitterNext)
                            {
                                toRemove.Add(splitterNext);
                            }
                        }

                        foreach (var removeVisible in toRemove)
                        {
                            RemoveDockable(removeVisible, true);
                        }
                    }
                    else
                    {
                        // TODO:
                    }
                }

                if (dock is IRootDock rootDock && rootDock.Window != null)
                {
                    RemoveWindow(rootDock.Window);
                }
                else
                {
                    RemoveDockable(dock, true);
                }
            }
        }

        /// <inheritdoc/>
        public virtual void RemoveDockable(IDockable dockable, bool collapse)
        {
            if (dockable?.Owner is IDock dock && dock.VisibleDockables != null)
            {
                int index = dock.VisibleDockables.IndexOf(dockable);
                if (index < 0)
                {
                    return;
                }
                dock.VisibleDockables.Remove(dockable);
                int indexActiveDockable = index > 0 ? index - 1 : 0;
                if (indexActiveDockable < 0)
                {
                    return;
                }
                if (dock.VisibleDockables.Count > 0)
                {
                    dock.ActiveDockable = dock.VisibleDockables[indexActiveDockable];
                }
                else
                {
                    dock.ActiveDockable = null;
                }
                if (collapse == true)
                {
                    Collapse(dock);
                }
            }
        }

        /// <inheritdoc/>
        public virtual void CloseDockable(IDockable dockable)
        {
            if (dockable.OnClose())
            {
                RemoveDockable(dockable, true);
            }
        }

        /// <inheritdoc/>
        public virtual void MoveDockable(IDock dock, IDockable sourceDockable, IDockable targetDockable)
        {
            int sourceIndex = dock.VisibleDockables.IndexOf(sourceDockable);
            int targetIndex = dock.VisibleDockables.IndexOf(targetDockable);

            if (sourceIndex >= 0 && targetIndex >= 0)
            {
                dock.VisibleDockables.RemoveAt(sourceIndex);
                dock.VisibleDockables.Insert(targetIndex, sourceDockable);
                dock.ActiveDockable = sourceDockable;
            }
        }

        /// <inheritdoc/>
        public virtual void SwapDockable(IDock dock, IDockable sourceDockable, IDockable targetDockable)
        {
            int sourceIndex = dock.VisibleDockables.IndexOf(sourceDockable);
            int targetIndex = dock.VisibleDockables.IndexOf(targetDockable);

            if (sourceIndex >= 0 && targetIndex >= 0)
            {
                var originalSourceDockable = dock.VisibleDockables[sourceIndex];
                var originalTargetDockable = dock.VisibleDockables[targetIndex];

                dock.VisibleDockables[targetIndex] = originalSourceDockable;
                dock.VisibleDockables[sourceIndex] = originalTargetDockable;
                dock.ActiveDockable = originalTargetDockable;
            }
        }

        /// <inheritdoc/>
        public virtual void MoveDockable(IDock sourceDock, IDock targetDock, IDockable sourceDockable, IDockable targetDockable)
        {
            if (sourceDockable?.Owner is IDock dock && dock.VisibleDockables != null)
            {
                RemoveDockable(sourceDockable, sourceDock != targetDock);
            }

            if (targetDock.VisibleDockables == null)
            {
                targetDock.VisibleDockables = CreateList<IDockable>();
            }

            int targetIndex = targetDock.VisibleDockables.IndexOf(targetDockable);
            if (targetIndex < 0)
            {
                targetIndex = 0;
            }
            else
            {
                targetIndex += 1;
            }

            if (targetIndex < 0)
            {
                return;
            }

            targetDock.VisibleDockables.Insert(targetIndex, sourceDockable);
            UpdateDockable(sourceDockable, targetDock);
            targetDock.ActiveDockable = sourceDockable;
        }

        /// <inheritdoc/>
        public virtual void SwapDockable(IDock sourceDock, IDock targetDock, IDockable sourceDockable, IDockable targetDockable)
        {
            int sourceIndex = sourceDock.VisibleDockables.IndexOf(sourceDockable);
            int targetIndex = targetDock.VisibleDockables.IndexOf(targetDockable);

            if (sourceIndex >= 0 && targetIndex >= 0)
            {
                var originalSourceDockable = sourceDock.VisibleDockables[sourceIndex];
                var originalTargetDockable = targetDock.VisibleDockables[targetIndex];
                sourceDock.VisibleDockables[sourceIndex] = originalTargetDockable;
                targetDock.VisibleDockables[targetIndex] = originalSourceDockable;

                UpdateDockable(originalSourceDockable, targetDock);
                UpdateDockable(originalTargetDockable, sourceDock);

                sourceDock.ActiveDockable = originalTargetDockable;
                targetDock.ActiveDockable = originalSourceDockable;
            }
        }

        /// <inheritdoc/>
        public virtual IDock CreateSplitLayout(IDock dock, IDockable dockable, DockOperation operation)
        {
            IDock split = null;

            var containerProportion = dock.Proportion;
            dock.Proportion = double.NaN;

            if (dockable is IDock dockableDock)
            {
                split = dockableDock;
            }
            else
            {
                split = CreateProportionalDock();
                split.Id = nameof(IProportionalDock);
                split.Title = nameof(IProportionalDock);

                if (dockable != null)
                {
                    split.ActiveDockable = dockable;
                    split.VisibleDockables = CreateList<IDockable>();
                    split.VisibleDockables.Add(dockable);
                }
            }

            var layout = CreateProportionalDock();
            layout.Id = nameof(IProportionalDock);
            layout.Title = nameof(IProportionalDock);
            layout.ActiveDockable = null;
            layout.Proportion = containerProportion;

            var splitter = CreateSplitterDock();
            splitter.Id = nameof(ISplitterDock);
            splitter.Title = nameof(ISplitterDock);

            switch (operation)
            {
                case DockOperation.Left:
                case DockOperation.Right:
                    layout.Orientation = Orientation.Horizontal;
                    break;
                case DockOperation.Top:
                case DockOperation.Bottom:
                    layout.Orientation = Orientation.Vertical;
                    break;
            }

            layout.VisibleDockables = CreateList<IDockable>();

            switch (operation)
            {
                case DockOperation.Left:
                case DockOperation.Top:
                    layout.VisibleDockables.Add(split);
                    break;
                case DockOperation.Right:
                case DockOperation.Bottom:
                    layout.VisibleDockables.Add(dock);
                    break;
            }

            layout.VisibleDockables.Add(splitter);

            switch (operation)
            {
                case DockOperation.Left:
                case DockOperation.Top:
                    layout.VisibleDockables.Add(dock);
                    break;
                case DockOperation.Right:
                case DockOperation.Bottom:
                    layout.VisibleDockables.Add(split);
                    break;
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
                        var layout = CreateSplitLayout(dock, dockable, operation);
                        if (dock.Owner is IDock ownerDock)
                        {
                            int index = ownerDock.VisibleDockables.IndexOf(dock);
                            if (index >= 0)
                            {
                                ownerDock.VisibleDockables.RemoveAt(index);
                                ownerDock.VisibleDockables.Insert(index, layout);
                            }
                        }
                        UpdateDockable(layout, dock.Owner);
                    }
                    break;
                default:
                    throw new NotSupportedException($"Not supported split operation: {operation}.");
            }
        }

        /// <inheritdoc/>
        public virtual IDockWindow CreateWindowFrom(IDockable dockable)
        {
            IDockable target = null;
            bool topmost = false;

            switch (dockable)
            {
                case ITool tool:
                    {
                        target = CreateToolDock();
                        target.Id = nameof(IToolDock);
                        target.Title = nameof(IToolDock);

                        if (target is IDock dock)
                        {
                            dock.ActiveDockable = dockable;
                            dock.VisibleDockables = CreateList<IDockable>();
                            dock.VisibleDockables.Add(dockable);
                        }

                        topmost = true;
                    }
                    break;
                case IDocument document:
                    {
                        target = CreateDocumentDock();
                        target.Id = nameof(IDocumentDock);
                        target.Title = nameof(IDocumentDock);

                        if (target is IDock dock)
                        {
                            dock.ActiveDockable = dockable;
                            dock.VisibleDockables = CreateList<IDockable>();
                            dock.VisibleDockables.Add(dockable);
                        }

                        topmost = false;
                    }
                    break;
                case IToolDock tooolDock:
                    {
                        target = dockable;
                        topmost = true;
                    }
                    break;
                case IDocumentDock documentDock:
                    {
                        target = dockable;
                        topmost = false;
                    }
                    break;
                case IProportionalDock proportionalDock:
                    {
                        target = proportionalDock;
                        topmost = false;
                    }
                    break;
                case IRootDock rootDock:
                    {
                        target = rootDock.ActiveDockable;
                        topmost = false;
                    }
                    break;
                default:
                    {
                        return null;
                    }
            }

            var root = CreateRootDock();
            root.Id = nameof(IRootDock);
            root.Title = nameof(IRootDock);
            root.ActiveDockable = target;
            root.DefaultDockable = target;
            root.VisibleDockables = CreateList<IDockable>();
            root.VisibleDockables.Add(target);
            root.Owner = null;

            var window = CreateDockWindow();
            window.Id = nameof(IDockWindow);
            window.Title = nameof(IDockWindow);
            window.Width = double.NaN;
            window.Height = double.NaN;
            window.Topmost = topmost;
            window.Layout = root;

            root.Window = window;

            return window;
        }
        
        /// <inheritdoc/>
        public virtual void SplitToWindow(IDock dock, IDockable dockable, double x, double y, double width, double height)
        {
            RemoveDockable(dockable, true);
            var window = CreateWindowFrom(dockable);
            if (window != null)
            {
                AddWindow(dock, window);
                window.X = x;
                window.Y = y;
                window.Width = width;
                window.Height = height;
                window.Present(false);
            }
        }
    }
}
