// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public virtual void CloseLayout(IView layout)
        {
            if (layout is IDock root)
            {
                root.HideWindows();
                if (root.CurrentView is IDock dock)
                {
                    dock.HideWindows();
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
            window.Context = GetContext(window.Id, context);
            window.Owner = owner;
            window.Factory = this;

            if (window.Layout != null)
            {
                Update(window.Layout, context, window.Layout.Parent);
            }
        }

        /// <inheritdoc/>
        public virtual void Update(IList<IDockWindow> windows, object context, IView owner)
        {
            foreach (var window in windows)
            {
                Update(window, context, owner);
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
        public virtual void SetCurrentView(IView view)
        {
            if (view.Parent is IDock dock)
            {
                dock.CurrentView = view;
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
        public virtual void RemoveView(IView view)
        {
            if (view?.Parent is IDock dock && dock.Views != null)
            {
                int index = dock.Views.IndexOf(view);
                dock.Views.Remove(view);
                dock.CurrentView = dock.Views.Count > 0 ? dock.Views[index > 0 ? index - 1 : 0] : null;
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
                targetDock.Views = new ObservableCollection<IView>();
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
                IView firstParent = first.Parent;
                IView secondParent = second.Parent;

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

        /// <inheritdoc/>
        public virtual IDock CreateSplitLayout(IDock dock, IView view, object context, DockOperation operation)
        {
            double width = double.NaN;
            double height = double.NaN;
            string originalDock = dock.Dock;
            double originalWidth = dock.Width;
            double originalHeight = dock.Height;

            switch (operation)
            {
                case DockOperation.Left:
                case DockOperation.Right:
                    width = originalWidth == double.NaN ? double.NaN : originalWidth / 2.0;
                    height = dock.Height == double.NaN ? double.NaN : dock.Height;
                    break;
                case DockOperation.Top:
                case DockOperation.Bottom:
                    width = originalWidth == double.NaN ? double.NaN : originalWidth;
                    height = originalHeight == double.NaN ? double.NaN : originalHeight / 2.0;
                    break;
            }

            if (view != null)
            {
                view.Width = double.NaN;
                view.Height = double.NaN;
            }

            IDock split = CreateLayoutDock();

            split.Id = nameof(ILayoutDock);
            split.Title = nameof(ILayoutDock);
            split.Width = width;
            split.Height = height;
            split.CurrentView = view ?? null;
            split.Views = view == null ? null : new ObservableCollection<IView> { view };

            switch (operation)
            {
                case DockOperation.Left:
                    split.Dock = "Left";
                    split.Width = width;
                    dock.Dock = "Right";
                    dock.Width = width;
                    break;
                case DockOperation.Right:
                    split.Dock = "Right";
                    split.Width = width;
                    dock.Dock = "Left";
                    dock.Width = width;
                    break;
                case DockOperation.Top:
                    split.Dock = "Top";
                    split.Height = height;
                    dock.Dock = "Bottom";
                    dock.Height = height;
                    break;
                case DockOperation.Bottom:
                    split.Dock = "Bottom";
                    split.Height = height;
                    dock.Dock = "Top";
                    dock.Height = height;
                    break;
            }

            var layout = CreateLayoutDock();
            layout.Id = nameof(ILayoutDock);
            layout.Dock = originalDock;
            layout.Width = originalWidth;
            layout.Height = originalHeight;
            layout.Title = nameof(ILayoutDock);
            layout.CurrentView = null;

            var splitter = CreateSplitterDock();
            splitter.Id = nameof(ISplitterDock);
            splitter.Title = nameof(ISplitterDock);
            splitter.Dock = (split.Dock == "Left" || split.Dock == "Right") ? "Left" : "Top";
            splitter.Width = double.NaN;
            splitter.Height = double.NaN;

            layout.Views = new ObservableCollection<IView>
            {
                (dock.Dock == "Left" || dock.Dock == "Top") ? dock : split,
                splitter,
                (dock.Dock == "Left" || dock.Dock == "Top") ? split : dock,
            };

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
                        var layout = CreateSplitLayout(dock, view, dock.Context, operation);

                        Replace(dock, layout);
                        Update(layout, dock.Context, dock.Parent);
                    }
                    break;
                default:
                    throw new NotSupportedException($"Not support dock operation: {operation}.");
            }
        }

        /// <inheritdoc/>
        public virtual void SplitToFill(IDock dock)
        {
            // TODO:
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

        private void InsertLayout(IDock dock, int index, object context)
        {
            var layout = CreateLayoutDock();
            layout.Id = nameof(ILayoutDock);
            layout.Title = nameof(ILayoutDock);
            layout.Width = double.NaN;
            layout.Height = double.NaN;

            Update(layout, context, dock);
            dock.Views.Insert(index, layout);
        }

        private void InsertRoot(IDock dock, int index, object context)
        {
            var root = CreateRootDock();
            root.Id = nameof(IRootDock);
            root.Title = nameof(IRootDock);
            root.Width = double.NaN;
            root.Height = double.NaN;

            Update(root, context, dock);
            dock.Views.Insert(index, root);
        }

        private void InsertSplitter(IDock dock, int index, object context)
        {
            var splitter = CreateSplitterDock();
            splitter.Id = nameof(ISplitterDock);
            splitter.Title = nameof(ISplitterDock);
            splitter.Width = double.NaN;
            splitter.Height = double.NaN;

            Update(splitter, context, dock);
            dock.Views.Insert(index, splitter);
        }

        private void InsertDocument(IDock dock, int index, object context)
        {
            var document = CreateDocumentDock();
            document.Id = nameof(IDocumentDock);
            document.Title = nameof(IDocumentDock);
            document.Width = double.NaN;
            document.Height = double.NaN;

            Update(document, context, dock);
            dock.Views.Insert(index, document);
        }

        private void InsertTool(IDock dock, int index, object context)
        {
            var tool = CreateToolDock();
            tool.Id = nameof(IToolDock);
            tool.Title = nameof(IToolDock);
            tool.Width = double.NaN;
            tool.Height = double.NaN;

            Update(tool, context, dock);
            dock.Views.Insert(index, tool);
        }

        private void InsertView(IDock dock, int index, object context)
        {
            var view = CreateView();
            view.Id = nameof(IView);
            view.Title = nameof(IView);
            view.Width = double.NaN;
            view.Height = double.NaN;

            Update(view, context, dock);
            dock.Views.Insert(index, view);
        }

        private void InsertToolTab(IDock dock, int index, object context)
        {
            var toolTab = CreateToolTab();
            toolTab.Id = nameof(IToolTab);
            toolTab.Title = nameof(IToolTab);
            toolTab.Width = double.NaN;
            toolTab.Height = double.NaN;

            Update(toolTab, context, dock);
            dock.Views.Insert(index, toolTab);
        }

        private void InsertDocumentTab(IDock dock, int index, object context)
        {
            var documentTab = CreateDocumentTab();
            documentTab.Id = nameof(IDocumentTab);
            documentTab.Title = nameof(IDocumentTab);
            documentTab.Width = double.NaN;
            documentTab.Height = double.NaN;

            Update(documentTab, context, dock);
            dock.Views.Insert(index, documentTab);
        }

        /// <inheritdoc/>
        public virtual void AddLayout(IDock dock)
        {
            if (dock.Views == null)
            {
                dock.Views = new ObservableCollection<IView>();
            }
            InsertLayout(dock, dock.Views.Count, dock.Context);
        }

        /// <inheritdoc/>
        public virtual void AddRoot(IDock dock)
        {
            if (dock.Views == null)
            {
                dock.Views = new ObservableCollection<IView>();
            }
            InsertRoot(dock, dock.Views.Count, dock.Context);
        }

        /// <inheritdoc/>
        public virtual void AddSplitter(IDock dock)
        {
            if (dock.Views == null)
            {
                dock.Views = new ObservableCollection<IView>();
            }
            InsertSplitter(dock, dock.Views.Count, dock.Context);
        }

        /// <inheritdoc/>
        public virtual void AddDocument(IDock dock)
        {
            if (dock.Views == null)
            {
                dock.Views = new ObservableCollection<IView>();
            }
            InsertDocument(dock, dock.Views.Count, dock.Context);
        }

        /// <inheritdoc/>
        public virtual void AddTool(IDock dock)
        {
            if (dock.Views == null)
            {
                dock.Views = new ObservableCollection<IView>();
            }
            InsertTool(dock, dock.Views.Count, dock.Context);
        }

        /// <inheritdoc/>
        public virtual void AddView(IDock dock)
        {
            if (dock.Views == null)
            {
                dock.Views = new ObservableCollection<IView>();
            }
            InsertView(dock, dock.Views.Count, dock.Context);
        }

        /// <inheritdoc/>
        public virtual void AddToolTab(IDock dock)
        {
            if (dock.Views == null)
            {
                dock.Views = new ObservableCollection<IView>();
            }
            InsertToolTab(dock, dock.Views.Count, dock.Context);
        }

        /// <inheritdoc/>
        public virtual void AddDocumentTab(IDock dock)
        {
            if (dock.Views == null)
            {
                dock.Views = new ObservableCollection<IView>();
            }
            InsertDocumentTab(dock, dock.Views.Count, dock.Context);
        }

        /// <inheritdoc/>
        public virtual void InsertLayoutBefore(IDock dock)
        {
            if (dock.Parent is IDock parent)
            {
                int index = parent.Views.IndexOf(dock);
                InsertLayout(parent, index, parent.Context);
            }
        }

        /// <inheritdoc/>
        public virtual void InsertRootBefore(IDock dock)
        {
            if (dock.Parent is IDock parent)
            {
                int index = parent.Views.IndexOf(dock);
                InsertRoot(parent, index, parent.Context);
            }
        }

        /// <inheritdoc/>
        public virtual void InsertSplitterBefore(IDock dock)
        {
            if (dock.Parent is IDock parent)
            {
                int index = parent.Views.IndexOf(dock);
                InsertSplitter(parent, index, parent.Context);
            }
        }

        /// <inheritdoc/>
        public virtual void InsertDocumentBefore(IDock dock)
        {
            if (dock.Parent is IDock parent)
            {
                int index = parent.Views.IndexOf(dock);
                InsertDocument(parent, index, parent.Context);
            }
        }

        /// <inheritdoc/>
        public virtual void InsertToolBefore(IDock dock)
        {
            if (dock.Parent is IDock parent)
            {
                int index = parent.Views.IndexOf(dock);
                InsertTool(parent, index, parent.Context);
            }
        }

        /// <inheritdoc/>
        public virtual void InsertViewBefore(IDock dock)
        {
            if (dock.Parent is IDock parent)
            {
                int index = parent.Views.IndexOf(dock);
                InsertView(parent, index, parent.Context);
            }
        }

        /// <inheritdoc/>
        public virtual void InsertToolTabBefore(IDock dock)
        {
            if (dock.Parent is IDock parent)
            {
                int index = parent.Views.IndexOf(dock);
                InsertToolTab(parent, index, parent.Context);
            }
        }

        /// <inheritdoc/>
        public virtual void InsertDocumentTabBefore(IDock dock)
        {
            if (dock.Parent is IDock parent)
            {
                int index = parent.Views.IndexOf(dock);
                InsertDocumentTab(parent, index, parent.Context);
            }
        }

        /// <inheritdoc/>
        public virtual void InsertLayoutAfter(IDock dock)
        {
            if (dock.Parent is IDock parent)
            {
                int index = parent.Views.IndexOf(dock) + 1;
                InsertLayout(parent, index, parent.Context);
            }
        }

        /// <inheritdoc/>
        public virtual void InsertRootAfter(IDock dock)
        {
            if (dock.Parent is IDock parent)
            {
                int index = parent.Views.IndexOf(dock) + 1;
                InsertRoot(parent, index, parent.Context);
            }
        }

        /// <inheritdoc/>
        public virtual void InsertSplitterAfter(IDock dock)
        {
            if (dock.Parent is IDock parent)
            {
                int index = parent.Views.IndexOf(dock) + 1;
                InsertSplitter(parent, index, parent.Context);
            }
        }

        /// <inheritdoc/>
        public virtual void InsertDocumentAfter(IDock dock)
        {
            if (dock.Parent is IDock parent)
            {
                int index = parent.Views.IndexOf(dock) + 1;
                InsertDocument(parent, index, parent.Context);
            }
        }

        /// <inheritdoc/>
        public virtual void InsertToolAfter(IDock dock)
        {
            if (dock.Parent is IDock parent)
            {
                int index = parent.Views.IndexOf(dock) + 1;
                InsertTool(parent, index, parent.Context);
            }
        }

        /// <inheritdoc/>
        public virtual void InsertViewAfter(IDock dock)
        {
            if (dock.Parent is IDock parent)
            {
                int index = parent.Views.IndexOf(dock) + 1;
                InsertView(parent, index, parent.Context);
            }
        }

        /// <inheritdoc/>
        public virtual void InsertToolTabAfter(IDock dock)
        {
            if (dock.Parent is IDock parent)
            {
                int index = parent.Views.IndexOf(dock) + 1;
                InsertToolTab(parent, index, parent.Context);
            }
        }

        /// <inheritdoc/>
        public virtual void InsertDocumentTabAfter(IDock dock)
        {
            if (dock.Parent is IDock parent)
            {
                int index = parent.Views.IndexOf(dock) + 1;
                InsertDocumentTab(parent, index, parent.Context);
            }
        }

        /// <summary>
        /// Copies properties from view to destination view.
        /// </summary>
        /// <param name="source">The source view.</param>
        /// <param name="destination">The destination view.</param>
        /// <param name="bCopyViews">The flag indicating whether to copy source views.</param>
        /// <param name="bCopyWindows">The flag indicating whether to copy source windows.</param>
        private void Copy(IView source, IView destination, bool bCopyViews, bool bCopyWindows)
        {
            destination.Id = source.Id;
            destination.Width = source.Width;
            destination.Height = source.Height;
            destination.Title = source.Title;

            if (source is IDock sourceDock && destination is IDock destinationDock)
            {
                destinationDock.Dock = sourceDock.Dock;

                if (bCopyViews)
                {
                    destinationDock.Views = sourceDock.Views;
                    destinationDock.CurrentView = sourceDock.CurrentView;
                    destinationDock.DefaultView = sourceDock.DefaultView;
                }

                if (bCopyWindows)
                {
                    destinationDock.Windows = sourceDock.Windows;
                }
            }
        }

        /// <inheritdoc/>
        public virtual void ConvertToLayout(IDock dock)
        {
            var layout = CreateLayoutDock();
            Copy(dock, layout, true, true);
            Update(layout, dock.Context, dock.Parent);
            Replace(dock, layout);
        }

        /// <inheritdoc/>
        public virtual void ConvertToRoot(IDock dock)
        {
            var root = CreateRootDock();
            Copy(dock, root, true, true);
            Update(root, dock.Context, dock.Parent);
            Replace(dock, root);
        }

        /// <inheritdoc/>
        public virtual void ConvertToSplitter(IDock dock)
        {
            var splitter = CreateSplitterDock();
            Copy(dock, splitter, false, false);
            Update(splitter, dock.Context, dock.Parent);
            Replace(dock, splitter);
        }

        /// <inheritdoc/>
        public virtual void ConvertToDocument(IDock dock)
        {
            var document = CreateDocumentDock();
            Copy(dock, document, true, false);
            Update(document, dock.Context, dock.Parent);
            Replace(dock, document);
        }

        /// <inheritdoc/>
        public virtual void ConvertToTool(IDock dock)
        {
            var tool = CreateToolDock();
            Copy(dock, tool, true, false);
            Update(tool, dock.Context, dock.Parent);
            Replace(dock, tool);
        }

        /// <inheritdoc/>
        public virtual void ConvertToView(IDock dock)
        {
            var view = CreateView();
            Copy(dock, view, false, true);
            Update(view, dock.Context, dock.Parent);
            Replace(dock, view);
        }

        /// <inheritdoc/>
        public virtual void ConvertToToolTab(IDock dock)
        {
            var toolTab = CreateToolTab();
            Copy(dock, toolTab, false, true);
            Update(toolTab, dock.Context, dock.Parent);
            Replace(dock, toolTab);
        }

        /// <inheritdoc/>
        public virtual void ConvertToDocumentTab(IDock dock)
        {
            var documentTab = CreateDocumentTab();
            Copy(dock, documentTab, false, true);
            Update(documentTab, dock.Context, dock.Parent);
            Replace(dock, documentTab);
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
                            dock.Views = new ObservableCollection<IView> { view };
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
                            dock.Views = new ObservableCollection<IView> { view };
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
                        Console.WriteLine($"Not supported window source: {view}");
                        return null;
                    }
            }

            if (target is IDock targetDock)
            {
                targetDock.Dock = "";
            }

            target.Width = double.NaN;
            target.Height = double.NaN;

            var root = CreateRootDock();
            root.Id = nameof(IRootDock);
            root.Title = nameof(IRootDock);
            root.Width = double.NaN;
            root.Height = double.NaN;
            root.CurrentView = target;
            root.DefaultView = target;
            root.Views = new ObservableCollection<IView> { target };
            root.Parent = FindRoot(view);

            var window = CreateDockWindow();
            window.Id = nameof(IDockWindow);
            window.Title = nameof(IDockWindow);
            window.Width = double.NaN;
            window.Height = double.NaN;
            window.Layout = root;

            return window;
        }

        /// <inheritdoc/>
        public virtual void AddWindow(IDock dock, IDockWindow window, object context)
        {
            if (dock.Windows == null)
            {
                dock.Windows = new ObservableCollection<IDockWindow>();
            }
            dock.Windows.Add(window);
            Update(window, context, dock);
        }

        /// <inheritdoc/>
        public virtual void RemoveWindow(IDockWindow window)
        {
            if (window?.Owner is IDock dock)
            {
                window.Destroy();
                dock.Windows?.Remove(window);
            }
        }
    }
}
