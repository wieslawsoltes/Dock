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
        private readonly Stack<IDock> _back = new Stack<IDock>();
        private readonly Stack<IDock> _forward = new Stack<IDock>();
        private string _id;
        private string _dock;
        private double _width;
        private double _height;
        private string _title;
        private object _context;
        private IDockFactory _factory;
        private IList<IDock> _views;
        private IDock _currentView;
        private IList<IDockWindow> _windows;

        /// <inheritdoc/>
        public string Id
        {
            get => _id;
            set => Update(ref _id, value);
        }

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
        public IDockFactory Factory
        {
            get => _factory;
            set => Update(ref _factory, value);
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
            set
            {
                Update(ref _currentView, value);

                Notify(nameof(CanGoBack));
                Notify(nameof(CanGoForward));
            }
        }

        /// <inheritdoc/>
        public IList<IDockWindow> Windows
        {
            get => _windows;
            set => Update(ref _windows, value);
        }

        /// <inheritdoc/>
        public bool CanGoBack => _back.Count > 0;

        /// <inheritdoc/>
        public bool CanGoForward => _forward.Count > 0;

        /// <inheritdoc/>
        public void GoBack()
        {
            if (_back.Count > 0)
            {
                var root = _back.Pop();
                if (_currentView != null)
                {
                    _forward.Push(_currentView);
                }
                NavigateImpl(root, false);
            }
        }

        /// <inheritdoc/>
        public void GoForward()
        {
            if (_forward.Count > 0)
            {
                var root = _forward.Pop();
                if (_currentView != null)
                {
                    _back.Push(_currentView);
                }
                NavigateImpl(root, false);
            }
        }

        private void NavigateReset()
        {
            if (_back.Count > 0)
            {
                _back.Clear();
            }

            if (_forward.Count > 0)
            {
                _forward.Clear();
            }
        }

        private void NavigateSnapshot(IDock dock)
        {
            if (_forward.Count > 0)
                _forward.Clear();
            _back.Push(dock);
        }

        public void NavigateImpl(object root, bool bSnapshot)
        {
            if (root == null)
            {
                if (_currentView != null)
                {
                    _currentView.HideWindows();
                }

                CurrentView = null;
                NavigateReset();
            }
            else if (root is IDock dock)
            {
                if (dock != null && _currentView != null && dock != _currentView)
                {
                    _currentView.HideWindows();
                }

                if (dock != null && _currentView != dock)
                {
                    if (_currentView != null && bSnapshot == true)
                    {
                        NavigateSnapshot(_currentView);
                    }

                    CurrentView = dock;
                }

                if (_currentView != null)
                {
                    _currentView.ShowWindows();
                }
            }
            else if (root is string id)
            {
                var result1 = _views.FirstOrDefault(v => v.Id == id);
                if (result1 != null)
                {
                    NavigateImpl(result1, bSnapshot);
                }
                else
                {
                    var views = _views.Flatten(v => v.Views);
                    var result2 = views.FirstOrDefault(v => v.Id == id);
                    if (result2 != null)
                    {
                        NavigateImpl(result2, bSnapshot);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void Navigate(object root)
        {
            NavigateImpl(root, true);
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
        public virtual void HideWindows()
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
        /// Check whether the <see cref="Id"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeId() => !string.IsNullOrEmpty(_id);

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
        /// Check whether the <see cref="Factory"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeFactory() => false;

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
        /// Check whether the <see cref="CanGoBack"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeCanGoBack() => false;

        /// <summary>
        /// Check whether the <see cref="Factory"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeCanGoForward() => false;
    }
}
