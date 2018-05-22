// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dock.Model;

namespace Dock.Avalonia.Factories
{
    /// <summary>
    /// Dock factory base.
    /// </summary>
    public abstract class BaseDockFactory : IDockFactory
    {
        /// <inheritdoc/>
        public virtual IDictionary<string, Func<object>> ContextLocator { get; set; }

        /// <inheritdoc/>
        public virtual Func<IDockHost> HostLocator { get; set; }

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
        public virtual void Update(IDockWindow window, object context)
        {
            window.Context = GetContext(window.Id, context);
            window.Factory = this;

            if (window.Layout != null)
            {
                Update(window.Layout, context);
            }
        }

        /// <inheritdoc/>
        public virtual void Update(IList<IDockWindow> windows, object context)
        {
            foreach (var window in windows)
            {
                Update(window, context);
            }
        }

        /// <inheritdoc/>
        public virtual void Update(IDock view, object context)
        {
            view.Context = GetContext(view.Id, context);
            view.Factory = this;

            if (view.Windows != null)
            {
                Update(view.Windows, context);
            }

            if (view.Views != null)
            {
                Update(view.Views, context);
            }
        }

        /// <inheritdoc/>
        public virtual void Update(IList<IDock> views, object context)
        {
            foreach (var view in views)
            {
                Update(view, context);
            }
        }

        /// <inheritdoc/>
        public virtual IDockWindow CreateWindow(IDock layout, object context, IDock source, int viewIndex, double x, double y)
        {
            var view = source.Views[viewIndex];

            source.RemoveView(viewIndex);

            var dockLayout = new DockLayout
            {
                Id = "DockLayout",
                Dock = "",
                Title = "Window",
                CurrentView = view,
                Views = new ObservableCollection<IDock> { view }
            };

            var dockWindow = new DockWindow()
            {
                Id = "DockWindow",
                X = x,
                Y = y,
                Width = 300,
                Height = 400,
                Title = "Dock",
                Layout = dockLayout
            };

            Update(dockWindow, context);

            if (layout.CurrentView.Windows == null)
            {
                layout.CurrentView.Windows = new ObservableCollection<IDockWindow>();
            }
            layout.CurrentView.AddWindow(dockWindow);

            return dockWindow;
        }

        /// <inheritdoc/>
        public abstract IDock CreateDefaultLayout();
    }
}
