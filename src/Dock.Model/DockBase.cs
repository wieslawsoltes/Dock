// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using System.Linq;

namespace Dock.Model
{
    /// <summary>
    /// Dock base class.
    /// </summary>
    public abstract class DockBase : ObservableObject, IDock
    {
        private string _dock;
        private double _width;
        private double _height;
        private string _title;
        private object _context;
        private IList<IDock> _views;
        private IDock _currentView;
        private IList<IDockWindow> _windows;
        private IDockFactory _factory;

        /// <inheritdoc/>
        public string Dock
        {
            get => _dock;
            set => Update(ref _dock, value);
        }

        /// <inheritdoc/>
        public double Width
        {
            get => _width;
            set => Update(ref _width, value);
        }

        /// <inheritdoc/>
        public double Height
        {
            get => _height;
            set => Update(ref _height, value);
        }

        /// <inheritdoc/>
        public string Title
        {
            get => _title;
            set => Update(ref _title, value);
        }

        /// <inheritdoc/>
        public object Context
        {
            get => _context;
            set => Update(ref _context, value);
        }

        /// <inheritdoc/>
        public IList<IDock> Views
        {
            get => _views;
            set => Update(ref _views, value);
        }

        /// <inheritdoc/>
        public IDock CurrentView
        {
            get => _currentView;
            set => Update(ref _currentView, value);
        }

        /// <inheritdoc/>
        public IList<IDockWindow> Windows
        {
            get => _windows;
            set => Update(ref _windows, value);
        }

        /// <inheritdoc/>
        public IDockFactory Factory
        {
            get => _factory;
            set => Update(ref _factory, value);
        }

        /// <inheritdoc/>
        public void RemoveView(IDock dock, int index)
        {
            dock.Views.RemoveAt(index);

            if (dock.Views.Count > 0)
            {
                dock.CurrentView = dock.Views[index > 0 ? index - 1 : 0];
            }
        }

        /// <inheritdoc/>
        public void MoveView(IDock dock, int sourceIndex, int targetIndex)
        {
            if (sourceIndex < targetIndex)
            {
                var item = dock.Views[sourceIndex];
                dock.Views.RemoveAt(sourceIndex);
                dock.Views.Insert(targetIndex, item);
                dock.CurrentView = item;
            }
            else
            {
                int removeIndex = sourceIndex;
                if (dock.Views.Count > removeIndex)
                {
                    var item = dock.Views[sourceIndex];
                    dock.Views.RemoveAt(removeIndex);
                    dock.Views.Insert(targetIndex, item);
                    dock.CurrentView = item;
                }
            }
        }

        /// <inheritdoc/>
        public void SwapView(IDock dock, int sourceIndex, int targetIndex)
        {
            var item1 = dock.Views[sourceIndex];
            var item2 = dock.Views[targetIndex];
            dock.Views[targetIndex] = item1;
            dock.Views[sourceIndex] = item2;
            dock.CurrentView = item2;
        }

        /// <inheritdoc/>
        public void MoveView(IDock sourceDock, IDock targetDock, int sourceIndex, int targetIndex)
        {
            var item = sourceDock.Views[sourceIndex];
            sourceDock.Views.RemoveAt(sourceIndex);
            targetDock.Views.Insert(targetIndex, item);

            if (sourceDock.Views.Count > 0)
            {
                sourceDock.CurrentView = sourceDock.Views[sourceIndex > 0 ? sourceIndex - 1 : 0];
            }

            if (targetDock.Views.Count > 0)
            {
                targetDock.CurrentView = targetDock.Views[targetIndex];
            }
        }

        /// <inheritdoc/>
        public void SwapView(IDock sourceDock, IDock targetDock, int sourceIndex, int targetIndex)
        {
            var item1 = sourceDock.Views[sourceIndex];
            var item2 = targetDock.Views[targetIndex];
            sourceDock.Views[sourceIndex] = item2;
            targetDock.Views[targetIndex] = item1;

            sourceDock.CurrentView = item2;
            targetDock.CurrentView = item1;
        }

        /// <inheritdoc/>
        public void OnChangeCurrentView(IDock view)
        {
            if (view != null && _currentView != null && view != _currentView)
            {
                _currentView.CloseWindows();
            }

            if (view != null && _currentView != view)
            {
                CurrentView = view;
            }

            if (_currentView != null)
            {
                _currentView.ShowWindows();
            }
        }

        /// <inheritdoc/>
        public void OnChangeCurrentView(string title)
        {
            OnChangeCurrentView(_views.FirstOrDefault(view => view.Title == title));
        }

        /// <inheritdoc/>
        public virtual void ShowWindows()
        {
            if (_windows != null)
            {
                foreach (var window in _windows)
                {
                    window.Present();
                }
            }
        }

        /// <inheritdoc/>
        public virtual void CloseWindows()
        {
            if (_windows != null)
            {
                foreach (var window in _windows)
                {
                    window.Destroy();
                }
            }
        }

        /// <inheritdoc/>
        public virtual void AddWindow(IDockWindow window)
        {
            _windows?.Add(window);
        }

        /// <inheritdoc/>
        public virtual void RemoveWindow(IDockWindow window)
        {
            _windows?.Remove(window);
        }

        /// <summary>
        /// Check whether the <see cref="Dock"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeDock() => !string.IsNullOrEmpty(_dock);

        /// <summary>
        /// Check whether the <see cref="Width"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeWidth() => true;

        /// <summary>
        /// Check whether the <see cref="Height"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeHeight() => true;

        /// <summary>
        /// Check whether the <see cref="Title"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeTitle() => !string.IsNullOrEmpty(_title);

        /// <summary>
        /// Check whether the <see cref="Context"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeContext() => false;

        /// <summary>
        /// Check whether the <see cref="Views"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeViews() => _views != null;

        /// <summary>
        /// Check whether the <see cref="CurrentView"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeCurrentView() => _currentView != null;

        /// <summary>
        /// Check whether the <see cref="Windows"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeWindows() => _windows != null;

        /// <summary>
        /// Check whether the <see cref="Factory"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeFactory() => false;
    }
}
