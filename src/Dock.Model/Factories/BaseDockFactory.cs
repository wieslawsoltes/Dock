// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;

namespace Dock.Model.Factories
{
    /// <summary>
    /// Dock factory base.
    /// </summary>
    public abstract class BaseDockFactory : IDockFactory
    {
        /// <inheritdoc/>
        public virtual IDictionary<string, Func<object>> ContextLocator { get; set; }

        /// <inheritdoc/>
        public virtual IDictionary<string, Func<IDockHost>> HostLocator { get; set; }

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
        public virtual void Update(IDockWindow window, object context, IDock owner)
        {
            window.Host = GetHost(window.Id);
            window.Context = GetContext(window.Id, context);
            window.Owner = owner;
            window.Factory = this;

            if (window.Layout != null)
            {
                Update(window.Layout, context, null);
            }
        }

        /// <inheritdoc/>
        public virtual void Update(IList<IDockWindow> windows, object context, IDock owner)
        {
            foreach (var window in windows)
            {
                Update(window, context, owner);
            }
        }

        /// <inheritdoc/>
        public virtual void Update(IDock view, object context, IDock parent)
        {
            view.Context = GetContext(view.Id, context);
            view.Parent = parent;
            view.Factory = this;

            if (view.Windows != null)
            {
                Update(view.Windows, context, view);
            }

            if (view.Views != null)
            {
                Update(view.Views, context, view);
            }
        }

        /// <inheritdoc/>
        public virtual void Update(IList<IDock> views, object context, IDock parent)
        {
            foreach (var view in views)
            {
                Update(view, context, parent);
            }
        }

        /// <inheritdoc/>
        public void Remove(IDock dock)
        {
            if (dock.Parent != null)
            {
                dock.Parent.Views?.Remove(dock);
            }
        }

        /// <inheritdoc/>
        public void Move(IDock dock, IDock parent)
        {
            IDock orignalParent = dock.Parent;
            int index = orignalParent.Views.IndexOf(dock);

            orignalParent.Views.Remove(dock);
            parent.Views.Add(dock);

            Update(dock, dock.Context, parent);

            if (orignalParent.Views.Count > 0)
            {
                orignalParent.CurrentView = orignalParent.Views[index > 0 ? index - 1 : 0];
            }

            parent.CurrentView = dock;
        }

        /// <inheritdoc/>
        public void Replace(IDock source, IDock destination)
        {
            if (source.Parent is IDock parent)
            {
                int index = parent.Views.IndexOf(source);
                parent.Views.RemoveAt(index);
                parent.Views.Insert(index, destination);
            }
        }

        /// <inheritdoc/>
        public void Split(IDock dock, DockOperation operation)
        {
            switch (operation)
            {
                case DockOperation.Left:
                case DockOperation.Right:
                case DockOperation.Top:
                case DockOperation.Bottom:
                    {
                        var layout = dock.SplitLayout(dock.Context, operation);
                        if (layout != null)
                        {
                            Replace(dock, layout);
                            Update(layout, dock.Context, dock.Parent);
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException($"Not support dock operation: {operation}.");
            }
        }

        /// <inheritdoc/>
        public void SplitToFill(IDock dock)
        {
            // TODO:
        }

        /// <inheritdoc/>
        public void SplitToLeft(IDock dock)
        {
            Split(dock, DockOperation.Left);
        }

        /// <inheritdoc/>
        public void SplitToRight(IDock dock)
        {
            Split(dock, DockOperation.Left);
        }

        /// <inheritdoc/>
        public void SplitToTop(IDock dock)
        {
            Split(dock, DockOperation.Left);
        }

        /// <inheritdoc/>
        public void SplitToBottom(IDock dock)
        {
            Split(dock, DockOperation.Left);
        }

        /// <inheritdoc/>
        public void SplitToWindow(IDock dock)
        {
            // TODO:
        }

        /// <summary>
        /// Copies properties from source to destination dock.
        /// </summary>
        /// <param name="source">The source dock.</param>
        /// <param name="destination">The destination dock.</param>
        /// <param name="bCopyViews">The flag indicating whether to copy source views.</param>
        /// <param name="bCopyWindows">The flag indicating whether to copy source windows.</param>
        private void Copy(IDock source, IDock destination, bool bCopyViews, bool bCopyWindows)
        {
            destination.Id = source.Id;
            destination.Dock = source.Dock;
            destination.Width = source.Width;
            destination.Height = source.Height;
            destination.Title = source.Title;

            if (bCopyViews)
            {
                destination.Views = source.Views;
                destination.CurrentView = source.CurrentView;
                destination.DefaultView = source.DefaultView;
            }

            if (bCopyWindows)
            {
                destination.Windows = source.Windows;
            }
        }

        /// <inheritdoc/>
        public void ConvertToLayout(IDock dock)
        {
            var layout = new DockLayout();
            Copy(dock, layout, true, true);
            Update(layout, dock.Context, dock.Parent);
            Replace(dock, layout);
        }

        /// <inheritdoc/>
        public void ConvertToRoot(IDock dock)
        {
            var layout = new DockRoot();
            Copy(dock, layout, true, true);
            Update(layout, dock.Context, dock.Parent);
            Replace(dock, layout);
        }

        /// <inheritdoc/>
        public void ConvertToSplitter(IDock dock)
        {
            var layout = new DockSplitter();
            Copy(dock, layout, false, false);
            Update(layout, dock.Context, dock.Parent);
            Replace(dock, layout);
        }

        /// <inheritdoc/>
        public void ConvertToStrip(IDock dock)
        {
            var layout = new DockStrip();
            Copy(dock, layout, true, false);
            Update(layout, dock.Context, dock.Parent);
            Replace(dock, layout);
        }

        /// <inheritdoc/>
        public void ConvertToView(IDock dock)
        {
            var layout = new DockView();
            Copy(dock, layout, false, true);
            Update(layout, dock.Context, dock.Parent);
            Replace(dock, layout);
        }

        /// <inheritdoc/>
        public abstract IDock CreateLayout();

        /// <inheritdoc/>
        public abstract void InitLayout(IDock layout, object context);
    }
}
