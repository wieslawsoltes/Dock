// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using Dock.Model.Controls;

namespace Dock.Model
{
    /// <summary>
    /// Dock factory base class.
    /// </summary>
    public abstract class DockFactoryBase : IDockFactory
    {
        /// <inheritdoc/>
        public virtual IDictionary<string, Func<object>> ContextLocator { get; set; }

        /// <inheritdoc/>
        public virtual IDictionary<string, Func<IDockHost>> HostLocator { get; set; }

        /// <inheritdoc/>
        public virtual IDictionary<string, Func<IView>> ViewLocator { get; set; }

        /// <inheritdoc/>
        public abstract IList<T> CreateList<T>(params T[] items);

        /// <inheritdoc/>
        public abstract IRootDock CreateRootDock();

        /// <inheritdoc/>
        public abstract IPinDock CreatePinDock();

        /// <inheritdoc/>
        public abstract ILayoutDock CreateLayoutDock();

        /// <inheritdoc/>
        public abstract ISplitterDock CreateSplitterDock();

        /// <inheritdoc/>
        public abstract IToolDock CreateToolDock();

        /// <inheritdoc/>
        public abstract IDocumentDock CreateDocumentDock();

        /// <inheritdoc/>
        public abstract IDockWindow CreateDockWindow();

        /// <inheritdoc/>
        public abstract IToolTab CreateToolTab();

        /// <inheritdoc/>
        public abstract IDocumentTab CreateDocumentTab();

        /// <inheritdoc/>
        public abstract IDock CreateLayout();

        /// <inheritdoc/>
        public virtual void InitLayout(IView layout)
        {
            Update(layout, null);
            if (layout is IDock root)
            {
                root.ShowWindows();
                root.CurrentView = root.DefaultView;
                if (root.CurrentView is IDock dock)
                {
                    dock.ShowWindows();
                }
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
        public virtual IDockHost GetHost(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                Func<IDockHost> locator = null;
                if (HostLocator?.TryGetValue(id, out locator) == true)
                {
                    return locator?.Invoke();
                }
            }
            throw new Exception($"Dock host with {id} is not registered.");
        }

        /// <inheritdoc/>
        public virtual void Update(IDockWindow window, IView owner)
        {
            window.Host = GetHost(window.Id);
            window.Host.Window = window;
            window.Owner = owner;
            window.Factory = this;

            if (window.Layout != null)
            {
                Update(window.Layout, window.Layout.Parent);
            }
        }

        /// <inheritdoc/>
        public virtual void Update(IView view, IView parent)
        {
            if (GetContext(view.Id) is object context)
            {
                view.Context = context;
            }

            view.Parent = parent;

            if (view is IDock dock)
            {
                dock.Factory = this;

                if (dock.Views != null)
                {
                    foreach (var child in dock.Views)
                    {
                        Update(child, view);
                    }
                }

                if (dock.Windows != null)
                {
                    foreach (var child in dock.Windows)
                    {
                        Update(child, view);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public virtual void AddView(IDock dock, IView view)
        {
            Update(view, dock);
            if (dock.Views == null)
            {
                dock.Views = CreateList<IView>();
            }
            dock.Views.Add(view);
        }

        /// <inheritdoc/>
        public virtual void InsertView(IDock dock, IView view, int index)
        {
            if (index >= 0)
            {
                Update(view, dock);
                if (dock.Views == null)
                {
                    dock.Views = CreateList<IView>();
                }
                dock.Views.Insert(index, view); 
            }
            else
            {
                throw new IndexOutOfRangeException();
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
            Update(window, dock);
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
        public virtual void SetCurrentView(IView view)
        {
            if (view.Parent is IDock dock)
            {
                dock.CurrentView = view;

                view.OnSelected();
            }
        }

        /// <summary>
        /// Sets the IsActive flag.
        /// </summary>
        /// <param name="view">the view to try and set IsActive on.</param>
        /// <param name="active">value to set</param>
        private void SetIsActive(IView view, bool active)
        {
            if (view is IDock dock)
            {
                dock.IsActive = active;
            }
        }

        /// <inheritdoc />
        public void SetFocusedView(IDock dock, IView view)
        {
            if (dock.CurrentView != null && FindRoot(dock.CurrentView) is IDock root)
            {
                if (root.FocusedView != null)
                {
                    SetIsActive(root.FocusedView.Parent, false);
                }

                root.FocusedView = view;

                if (root.FocusedView != null)
                {
                    SetIsActive(root.FocusedView.Parent, true);
                }
            }
        }

        /// <inheritdoc/>
        public virtual IView FindRoot(IView view)
        {
            if (view.Parent == null)
            {
                return view;
            }
            return FindRoot(view.Parent);
        }

        /// <inheritdoc/>
        public virtual IView FindView(IDock dock, Func<IView, bool> predicate)
        {
            if (predicate(dock) == true)
            {
                return dock;
            }

            if (dock.Views != null)
            {
                foreach (var view in dock.Views)
                {
                    if (predicate(view) == true)
                    {
                        return view;
                    }

                    if (view is IDock childDock)
                    {
                        var result = FindView(childDock, predicate);
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

                        var result = FindView(window.Layout, predicate);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public virtual void PinView(IView view)
        {
            // TODO: Implement view pinning.

            if (view != null && FindRoot(view) is IRootDock root)
            {
                if (PinClosestPinDock(root, view) is IPinDock pinDock)
                {
                    PinView(pinDock, view);
                }
            }
        }

        private IPinDock PinClosestPinDock(IRootDock root, IView view)
        {
            // TODO: Find closest pin dock to the view (Top, Bottom, Left or Right).

            if (root.Left == null)
            {
                root.Left = CreatePinDock();
                root.Left.Alignment = Alignment.Left;
                root.Left.IsExpanded = false;
                root.Left.AutoHide = true;
            }

            return root.Left;
        }

        private void PinView(IPinDock pinDock, IView view)
        {
            RemoveView(view);

            if (pinDock.Views == null)
            {
                pinDock.Views = CreateList<IView>();
            }

            pinDock.Views.Add(view);
            pinDock.CurrentView = view;
        }

        private void Collapse(IDock dock)
        {
            if (dock.IsCollapsable && dock.Views.Count == 0)
            {
                if (dock.Parent is IDock parentDock)
                {
                    var toRemove = new List<IView>();
                    var dockIndex = parentDock.Views.IndexOf(dock);

                    if (dockIndex >= 0)
                    {
                        int indexSplitterPrevious = dockIndex - 1;
                        if (dockIndex > 0 && indexSplitterPrevious >= 0)
                        {
                            var previousView = parentDock.Views[indexSplitterPrevious];
                            if (previousView is ISplitterDock splitterPrevious)
                            {
                                toRemove.Add(splitterPrevious);
                            }
                        }

                        int indexSplitterNext = dockIndex + 1;
                        if (dockIndex < parentDock.Views.Count - 1 && indexSplitterNext >= 0)
                        {
                            var nextView = parentDock.Views[indexSplitterNext];
                            if (nextView is ISplitterDock splitterNext)
                            {
                                toRemove.Add(splitterNext);
                            }
                        }

                        foreach (var removeView in toRemove)
                        {
                            RemoveView(removeView);
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
                    RemoveView(dock);
                }
            }
        }

        /// <inheritdoc/>
        public virtual void RemoveView(IView view)
        {
            if (view?.Parent is IDock dock && dock.Views != null)
            {
                int index = dock.Views.IndexOf(view);
                if (index < 0)
                {
                    return;
                }
                dock.Views.Remove(view);
                int indexCurrentView = index > 0 ? index - 1 : 0;
                
                if(indexCurrentView < 0)
                {
                    return;
                }

                if (dock.Views.Count > 0)
                {
                    dock.CurrentView = dock.Views[indexCurrentView];
                }
                else
                {
                    dock.CurrentView = null;
                }
                Collapse(dock);
            }
        }

        /// <inheritdoc/>
        public virtual void CloseView(IView view)
        {
            if (view.OnClose())
            {
                RemoveView(view);
            }
        }

        /// <inheritdoc/>
        public virtual void MoveView(IDock dock, IView sourceView, IView targetView)
        {
            int sourceIndex = dock.Views.IndexOf(sourceView);
            int targetIndex = dock.Views.IndexOf(targetView);

            if (sourceIndex >= 0 && targetIndex >= 0)
            {
                dock.Views.RemoveAt(sourceIndex);
                dock.Views.Insert(targetIndex, sourceView);
                dock.CurrentView = sourceView; 
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        /// <inheritdoc/>
        public virtual void MoveView(IDock sourceDock, IDock targetDock, IView sourceView, IView targetView)
        {
            if (sourceView?.Parent is IDock dock && dock.Views != null)
            {
                int index = dock.Views.IndexOf(sourceView);
                if (index < 0)
                {
                    throw new IndexOutOfRangeException();
                }
                dock.Views.Remove(sourceView);
                int indexCurrentView = index > 0 ? index - 1 : 0;
                if (indexCurrentView < 0)
                {
                    throw new IndexOutOfRangeException();
                }
                if (dock.Views.Count > 0)
                {
                    dock.CurrentView = dock.Views[indexCurrentView];
                }
                else
                {
                    dock.CurrentView = null;
                }
                Collapse(dock);
            }

            if (targetDock.Views == null)
            {
                targetDock.Views = CreateList<IView>();
            }

            int targetIndex = targetDock.Views.IndexOf(targetView);
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
                throw new IndexOutOfRangeException();
            }

            targetDock.Views.Insert(targetIndex, sourceView);
            Update(sourceView, targetDock);
            targetDock.CurrentView = sourceView;
        }

        /// <inheritdoc/>
        public virtual void Move(IView first, IView second)
        {
            if (first.Parent is IDock sourceDock && second.Parent is IDock targetDock)
            {
                if (sourceDock.Views != null)
                {
                    int index = sourceDock.Views.IndexOf(first);
                    if (index < 0)
                    {
                        throw new IndexOutOfRangeException();
                    }
                    sourceDock.Views.Remove(first);
                    int indexCurrentView = index > 0 ? index - 1 : 0;
                    if (indexCurrentView < 0)
                    {
                        throw new IndexOutOfRangeException();
                    }
                    if (sourceDock.Views.Count > 0)
                    {
                        sourceDock.CurrentView = sourceDock.Views[indexCurrentView];
                    }
                    else
                    {
                        sourceDock.CurrentView = null;
                    }
                    Collapse(sourceDock);
                }

                targetDock.Views.Add(first);
                Update(first, second);
                targetDock.CurrentView = first;
            }
        }

        /// <inheritdoc/>
        public virtual void Swap(IView first, IView second)
        {
            if (first.Parent is IDock sourceDock && second.Parent is IDock targetDock)
            {
                var firstParent = first.Parent;
                var secondParent = second.Parent;

                int firstIndex = sourceDock.Views.IndexOf(first);
                int secondIndex = targetDock.Views.IndexOf(second);

                if (firstIndex >= 0 && secondIndex >= 0)
                {
                    sourceDock.Views.RemoveAt(firstIndex);
                    targetDock.Views.RemoveAt(secondIndex);

                    sourceDock.Views.Insert(firstIndex, second);
                    targetDock.Views.Insert(secondIndex, first);

                    Update(first, secondParent);
                    Update(second, firstParent);

                    sourceDock.CurrentView = second;
                    targetDock.CurrentView = first; 
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        /// <inheritdoc/>
        public virtual void SwapView(IDock dock, IView sourceView, IView targetView)
        {
            int sourceIndex = dock.Views.IndexOf(sourceView);
            int targetIndex = dock.Views.IndexOf(targetView);

            if (sourceIndex >= 0 && targetIndex >= 0)
            {
                var originalSourceView = dock.Views[sourceIndex];
                var originalTargetView = dock.Views[targetIndex];

                dock.Views[targetIndex] = originalSourceView;
                dock.Views[sourceIndex] = originalTargetView;
                dock.CurrentView = originalTargetView; 
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        /// <inheritdoc/>
        public virtual void SwapView(IDock sourceDock, IDock targetDock, IView sourceView, IView targetView)
        {
            int sourceIndex = sourceDock.Views.IndexOf(sourceView);
            int targetIndex = targetDock.Views.IndexOf(targetView);

            if (sourceIndex >= 0 && targetIndex >= 0)
            {
                var originalSourceView = sourceDock.Views[sourceIndex];
                var originalTargetView = targetDock.Views[targetIndex];
                sourceDock.Views[sourceIndex] = originalTargetView;
                targetDock.Views[targetIndex] = originalSourceView;

                Update(originalSourceView, targetDock);
                Update(originalTargetView, sourceDock);

                sourceDock.CurrentView = originalTargetView;
                targetDock.CurrentView = originalSourceView; 
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        /// <inheritdoc/>
        public virtual void Replace(IView source, IView destination)
        {
            if (source.Parent is IDock dock)
            {
                int index = dock.Views.IndexOf(source);
                if (index >= 0)
                {
                    dock.Views.RemoveAt(index);
                    dock.Views.Insert(index, destination); 
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        /// <inheritdoc/>
        public virtual IDock CreateSplitLayout(IDock dock, IView view, DockOperation operation)
        {
            IDock split = null;

            var containerProportion = dock.Proportion;
            dock.Proportion = double.NaN;

            if (view is IDock viewDock)
            {
                split = viewDock;
            }
            else
            {
                split = CreateLayoutDock();
                split.Id = nameof(ILayoutDock);
                split.Title = nameof(ILayoutDock);

                if (view != null)
                {
                    split.CurrentView = view;
                    split.Views = CreateList<IView>();
                    split.Views.Add(view);
                }
            }

            var layout = CreateLayoutDock();
            layout.Id = nameof(ILayoutDock);
            layout.Title = nameof(ILayoutDock);
            layout.CurrentView = null;
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

            layout.Views = CreateList<IView>();

            switch (operation)
            {
                case DockOperation.Left:
                case DockOperation.Top:
                    layout.Views.Add(split);
                    break;
                case DockOperation.Right:
                case DockOperation.Bottom:
                    layout.Views.Add(dock);
                    break;
            }

            layout.Views.Add(splitter);

            switch (operation)
            {
                case DockOperation.Left:
                case DockOperation.Top:
                    layout.Views.Add(dock);
                    break;
                case DockOperation.Right:
                case DockOperation.Bottom:
                    layout.Views.Add(split);
                    break;
            }

            return layout;
        }

        /// <inheritdoc/>
        public virtual void Split(IDock dock, IView view, DockOperation operation)
        {
            switch (operation)
            {
                case DockOperation.Left:
                case DockOperation.Right:
                case DockOperation.Top:
                case DockOperation.Bottom:
                    {
                        var layout = CreateSplitLayout(dock, view, operation);
                        Replace(dock, layout);
                        Update(layout, dock.Parent);
                    }
                    break;
                default:
                    throw new NotSupportedException($"Not support dock operation: {operation}.");
            }
        }

        /// <inheritdoc/>
        public virtual void SplitToFill(IDock dock)
        {
            // TODO: Split to fill.
        }

        /// <inheritdoc/>
        public virtual void SplitToLeft(IDock dock)
        {
            Split(dock, null, DockOperation.Left);
        }

        /// <inheritdoc/>
        public virtual void SplitToRight(IDock dock)
        {
            Split(dock, null, DockOperation.Right);
        }

        /// <inheritdoc/>
        public virtual void SplitToTop(IDock dock)
        {
            Split(dock, null, DockOperation.Top);
        }

        /// <inheritdoc/>
        public virtual void SplitToBottom(IDock dock)
        {
            Split(dock, null, DockOperation.Bottom);
        }

        /// <inheritdoc/>
        public virtual void SplitToWindow(IDock dock)
        {
            if (FindRoot(dock) is IDock root && root.CurrentView is IDock currentViewRoot)
            {
                if (dock?.Parent is IDock parentDock && dock.Views != null)
                {
                    int index = parentDock.Views.IndexOf(dock);
                    if (index >= 0)
                    {
                        parentDock.Views.Remove(dock);
                        int currentViewIndex = index > 0 ? index - 1 : 0;
                        if (currentViewIndex < 0)
                        {
                            throw new IndexOutOfRangeException();
                        }
                        if (parentDock.Views.Count > 0)
                        {
                            parentDock.CurrentView = parentDock.Views[currentViewIndex];
                        }
                        else
                        {
                            parentDock.CurrentView = null;
                        }
                        Collapse(parentDock); 
                    }
                    else
                    {
                        throw new IndexOutOfRangeException();
                    }
                }

                var window = CreateWindowFrom(dock);
                if (window != null)
                {
                    AddWindow(currentViewRoot, window);

                    window.X = 0;
                    window.Y = 0;
                    window.Width = 300;
                    window.Height = 400;
                    window.Present(false);
                }
            }
        }

        /// <inheritdoc/>
        public virtual IDockWindow CreateWindowFrom(IView view)
        {
            IView target = null;
            bool topmost = false;

            switch (view)
            {
                case IRootDock targetRoot:
                    {
                        target = targetRoot.CurrentView;
                        topmost = false;
                    }
                    break;
                case IToolTab targetToolTab:
                    {
                        target = CreateToolDock();
                        target.Id = nameof(IToolDock);
                        target.Title = nameof(IToolDock);

                        if (target is IDock dock)
                        {
                            dock.CurrentView = view;
                            dock.Views = CreateList<IView>();
                            dock.Views.Add(view);
                        }

                        topmost = true;
                    }
                    break;
                case IDocumentTab targetDocumentTab:
                    {
                        target = CreateDocumentDock();
                        target.Id = nameof(IDocumentDock);
                        target.Title = nameof(IDocumentDock);

                        if (target is IDock dock)
                        {
                            dock.CurrentView = view;
                            dock.Views = CreateList<IView>();
                            dock.Views.Add(view);
                        }

                        topmost = false;
                    }
                    break;
                case ILayoutDock targetLayout:
                    {
                        target = targetLayout;
                        topmost = false;
                    }
                    break;
                case IToolDock targetTool:
                    {
                        target = view;
                        topmost = true;
                    }
                    break;
                case IDocumentDock targetDocument:
                    {
                        target = view;
                        topmost = false;
                    }
                    break;
                default:
                    {
                        Logger.Log($"Not supported window source: {view}");
                        return null;
                    }
            }

            var root = CreateRootDock();
            root.Id = nameof(IRootDock);
            root.Title = nameof(IRootDock);
            root.CurrentView = target;
            root.DefaultView = target;
            root.Views = CreateList<IView>();
            root.Views.Add(target);
            root.Parent = FindRoot(view);

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
    }
}
