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
        public abstract IView CreateView();

        /// <inheritdoc/>
        public abstract IDock CreateDock();

        /// <inheritdoc/>
        public virtual void InitLayout(IView layout, object context)
        {
            Update(layout, context, null);
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
        public abstract IDock CreateLayout();

        /// <inheritdoc/>
        public virtual object GetContext(string id, object context)
        {
            if (!string.IsNullOrEmpty(id))
            {
                Func<object> locator = null;
                if (ContextLocator?.TryGetValue(id, out locator) == true)
                {
                    return locator?.Invoke();
                }
            }
            return context;
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
        public virtual void Update(IDockWindow window, object context, IView owner)
        {
            window.Host = GetHost(window.Id);
            window.Host.Window = window;
            window.Context = GetContext(window.Id, context);
            window.Owner = owner;
            window.Factory = this;

            if (window.Layout != null)
            {
                Update(window.Layout, context, window.Layout.Parent);
            }
        }

        /// <inheritdoc/>
        public virtual void Update(IView view, object context, IView parent)
        {
            view.Context = GetContext(view.Id, context);
            view.Parent = parent;

            if (view is IDock dock)
            {
                dock.Factory = this;

                if (dock.Views != null)
                {
                    foreach (var child in dock.Views)
                    {
                        Update(child, context, view);
                    }
                }

                if (dock.Windows != null)
                {
                    foreach (var child in dock.Windows)
                    {
                        Update(child, context, view);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public virtual void AddView(IDock dock, IView view, object context)
        {
            Update(view, context, dock);
            if (dock.Views == null)
            {
                dock.Views = CreateList<IView>();
            }
            dock.Views.Add(view);
        }

        /// <inheritdoc/>
        public virtual void InsertView(IDock dock, IView view, int index, object context)
        {
            Update(view, context, dock);
            if (dock.Views == null)
            {
                dock.Views = CreateList<IView>();
            }
            dock.Views.Insert(index, view);
        }

        /// <inheritdoc/>
        public virtual void AddWindow(IDock dock, IDockWindow window, object context)
        {
            if (dock.Windows == null)
            {
                dock.Windows = CreateList<IDockWindow>();
            }
            dock.Windows.Add(window);
            Update(window, context, dock);
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

        private void Collapse(IDock dock)
        {
            if (dock.Views.Count == 0)
            {
                if (dock.Parent is IDock parentDock)
                {
                    var toRemove = new List<IView>();
                    var dockIndex = parentDock.Views.IndexOf(dock);

                    if (dockIndex > 0
                        && parentDock.Views[dockIndex - 1] is ISplitterDock splitterPrevious)
                    {
                        toRemove.Add(splitterPrevious);
                    }

                    if (dockIndex < parentDock.Views.Count - 1
                        && parentDock.Views[dockIndex + 1] is ISplitterDock splitterNext)
                    {
                        toRemove.Add(splitterNext);
                    }

                    foreach (var removeView in toRemove)
                    {
                        RemoveView(removeView);
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
                if (view.OnClose())
                {
                    int index = dock.Views.IndexOf(view);
                    dock.Views.Remove(view);
                    dock.CurrentView = dock.Views.Count > 0 ? dock.Views[index > 0 ? index - 1 : 0] : null;
                    Collapse(dock);
                }
            }
        }

        /// <inheritdoc/>
        public virtual void MoveView(IDock dock, IView sourceView, IView targetView)
        {
            int sourceIndex = dock.Views.IndexOf(sourceView);
            int targetIndex = dock.Views.IndexOf(targetView);

            dock.Views.RemoveAt(sourceIndex);
            dock.Views.Insert(targetIndex, sourceView);
            dock.CurrentView = sourceView;
        }

        /// <inheritdoc/>
        public virtual void MoveView(IDock sourceDock, IDock targetDock, IView sourceView, IView targetView)
        {
            RemoveView(sourceView);

            if (targetDock.Views == null)
            {
                targetDock.Views = CreateList<IView>();
            }

            int targetIndex = targetDock.Views.IndexOf(targetView);
            if (targetIndex < 0)
                targetIndex = 0;
            else
                targetIndex += 1;

            targetDock.Views.Insert(targetIndex, sourceView);
            Update(sourceView, sourceView.Context, targetDock);
            targetDock.CurrentView = sourceView;
        }

        /// <inheritdoc/>
        public virtual void Move(IView first, IView second)
        {
            if (first.Parent is IDock sourceDock && second.Parent is IDock targetDock)
            {
                RemoveView(first);

                targetDock.Views.Add(first);
                Update(first, first.Context, second);
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

                sourceDock.Views.RemoveAt(firstIndex);
                targetDock.Views.RemoveAt(secondIndex);

                sourceDock.Views.Insert(firstIndex, second);
                targetDock.Views.Insert(secondIndex, first);

                Update(first, first.Context, secondParent);
                Update(second, second.Context, firstParent);

                sourceDock.CurrentView = second;
                targetDock.CurrentView = first;
            }
        }

        /// <inheritdoc/>
        public virtual void SwapView(IDock dock, IView sourceView, IView targetView)
        {
            int sourceIndex = dock.Views.IndexOf(sourceView);
            int targetIndex = dock.Views.IndexOf(targetView);

            var originalSourceView = dock.Views[sourceIndex];
            var originalTargetView = dock.Views[targetIndex];

            dock.Views[targetIndex] = originalSourceView;
            dock.Views[sourceIndex] = originalTargetView;
            dock.CurrentView = originalTargetView;
        }

        /// <inheritdoc/>
        public virtual void SwapView(IDock sourceDock, IDock targetDock, IView sourceView, IView targetView)
        {
            int sourceIndex = sourceDock.Views.IndexOf(sourceView);
            int targetIndex = targetDock.Views.IndexOf(targetView);

            var originalSourceView = sourceDock.Views[sourceIndex];
            var originalTargetView = targetDock.Views[targetIndex];
            sourceDock.Views[sourceIndex] = originalTargetView;
            targetDock.Views[targetIndex] = originalSourceView;

            Update(originalSourceView, originalSourceView.Context, targetDock);
            Update(originalTargetView, originalTargetView.Context, sourceDock);

            sourceDock.CurrentView = originalTargetView;
            targetDock.CurrentView = originalSourceView;
        }

        /// <inheritdoc/>
        public virtual void Replace(IView source, IView destination)
        {
            if (source.Parent is IDock dock)
            {
                int index = dock.Views.IndexOf(source);
                dock.Views.RemoveAt(index);
                dock.Views.Insert(index, destination);
            }
        }

        public virtual void SplitExistingLayout (IDock dock, IView view, DockOperation operation)
        {
            IDock split;

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

            var splitter = CreateSplitterDock();
            splitter.Id = nameof(ISplitterDock);
            splitter.Title = nameof(ISplitterDock);
            var layout = dock.Parent as ILayoutDock;

            switch (operation)
            {
                case DockOperation.Left:
                case DockOperation.Top:
                    layout.Views.Insert(0, splitter);
                    layout.Views.Insert(0, split);
                    break;
                case DockOperation.Right:
                case DockOperation.Bottom:
                    layout.Views.Add(splitter);
                    layout.Views.Add(split);

                    splitter.Parent = layout;
                    split.Context = layout.Context;
                    break;
            }
        }

        /// <inheritdoc/>
        public virtual IDock CreateSplitLayout(IDock dock, IView view, object context, DockOperation operation)
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

            var splitter = CreateSplitterDock();
            splitter.Id = nameof(ISplitterDock);
            splitter.Title = nameof(ISplitterDock);

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

        private bool NewLayoutRequired(IDock dock, DockOperation operation)
        {
            var layout = dock.Parent as ILayoutDock;

            bool result = layout == null ? true : false;

            if (!result)
            {
                switch (operation)
                {
                    case DockOperation.Left:
                    case DockOperation.Right:
                        if (layout.Orientation != Orientation.Horizontal)
                        {
                            result = true;
                        }
                        break;

                    case DockOperation.Top:
                    case DockOperation.Bottom:
                        if (layout.Orientation != Orientation.Vertical)
                        {
                            result = true;
                        }
                        break;
                }
            }

            return result;
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
                        if (NewLayoutRequired(dock, operation))
                        {
                            var layout = CreateSplitLayout(dock, view, dock.Context, operation);

                            Replace(dock, layout);
                            Update(layout, dock.Context, dock.Parent);
                        }
                        else
                        {
                            SplitExistingLayout(dock, view, operation);
                        }
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
                RemoveView(dock);

                var window = CreateWindowFrom(dock);
                if (window != null)
                {
                    AddWindow(currentViewRoot, window, dock.Context);

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

            switch (view)
            {
                case IRootDock targetRoot:
                    {
                        target = targetRoot.CurrentView;
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
                    }
                    break;
                case ILayoutDock targetLayout:
                    {
                        target = targetLayout;
                    }
                    break;
                case IToolDock targetTool:
                    {
                        target = view;
                    }
                    break;
                case IDocumentDock targetDocument:
                    {
                        target = view;
                    }
                    break;
                default:
                    {
#if DEBUG
                        Console.WriteLine($"Not supported window source: {view}");
#endif
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
            window.Layout = root;

            root.Window = window;

            return window;
        }
    }
}
