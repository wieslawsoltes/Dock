// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using System.Linq;

namespace Dock.Model
{
    /// <summary>
    /// Dock base class.
    /// </summary>
    public abstract class DockBase : ViewBase, IDock
    {
        private readonly Stack<IView> _back = new Stack<IView>();
        private readonly Stack<IView> _forward = new Stack<IView>();
        private IList<IView> _views;
        private IView _currentView;
        private IView _defaultView;
        private IList<IDockWindow> _windows;
        private string _dock;
        private IDockFactory _factory;

        /// <inheritdoc/>
        public IList<IView> Views
        {
            get => _views;
            set => Update(ref _views, value);
        }

        /// <inheritdoc/>
        public IView CurrentView
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
        public IView DefaultView
        {
            get => _defaultView;
            set => Update(ref _defaultView, value);
        }

        /// <inheritdoc/>
        public bool CanGoBack => _back.Count > 0;

        /// <inheritdoc/>
        public bool CanGoForward => _forward.Count > 0;

        /// <inheritdoc/>
        public IList<IDockWindow> Windows
        {
            get => _windows;
            set => Update(ref _windows, value);
        }

        /// <inheritdoc/>
        public string Dock
        {
            get => _dock;
            set => Update(ref _dock, value);
        }

        /// <inheritdoc/>
        public IDockFactory Factory
        {
            get => _factory;
            set => Update(ref _factory, value);
        }


        /// <inheritdoc/>
        public void GoBack()
        {
            if (_back.Count > 0)
            {
                var root = _back.Pop();
                if (CurrentView != null)
                {
                    _forward.Push(CurrentView);
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
                if (CurrentView != null)
                {
                    _back.Push(CurrentView);
                }
                NavigateImpl(root, false);
            }
        }

        /// <summary>
        /// Resets navigation history.
        /// </summary>
        private void ResetNavigation()
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

        /// <summary>
        /// Makes snapshot of most recent dock.
        /// </summary>
        /// <param name="view">The view to make snapshot from.</param>
        private void MakeSnapshot(IView view)
        {
            if (_forward.Count > 0)
                _forward.Clear();
            _back.Push(view);
        }

        /// <summary>
        /// Implementation of the <see cref="IViewsHost.Navigate(object)"/> method.
        /// </summary>
        /// <param name="root">An object that contains the content to navigate to.</param>
        /// <param name="bSnapshot">The lag indicating whether to make snapshot.</param>
        public void NavigateImpl(object root, bool bSnapshot)
        {
            if (root == null)
            {
                if (CurrentView is IWindowsHost currentViewWindows)
                {
                    currentViewWindows.HideWindows();
                    CurrentView = null;
                    ResetNavigation();
                }
            }
            else if (root is IDock dock)
            {
                if (CurrentView is IWindowsHost currentViewWindows)
                {
                    currentViewWindows.HideWindows();
                }

                if (dock != null && CurrentView != dock)
                {
                    if (CurrentView != null && bSnapshot == true)
                    {
                        MakeSnapshot(CurrentView);
                    }

                    CurrentView = dock;
                }

                if (dock is IWindowsHost dockWindows)
                {
                    dockWindows.ShowWindows();
                }
            }
            else if (root is string id)
            {
                var result1 = Views.FirstOrDefault(v => v.Id == id);
                if (result1 != null)
                {
                    NavigateImpl(result1, bSnapshot);
                }
                else
                {
                    var views = Views.Flatten(v =>
                    {
                        if (v is IViewsHost n)
                        {
                            return n.Views;
                        }
                        return null;
                    });
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
            if (Windows != null)
            {
                foreach (var window in Windows)
                {
                    window.Present(false);
                }
            }
        }

        /// <inheritdoc/>
        public virtual void HideWindows()
        {
            if (Windows != null)
            {
                foreach (var window in Windows)
                {
                    window.Destroy();
                }
            }
        }

        /// <summary>
        /// Check whether the <see cref="Views"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeViews() => Views != null;

        /// <summary>
        /// Check whether the <see cref="CurrentView"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeCurrentView() => CurrentView != null;

        /// <summary>
        /// Check whether the <see cref="DefaultView"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeDefaultView() => DefaultView != null;

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

        /// <summary>
        /// Check whether the <see cref="Windows"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeWindows() => Windows != null;

        /// <summary>
        /// Check whether the <see cref="Dock"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeDock() => !string.IsNullOrEmpty(Dock);

        /// <summary>
        /// Check whether the <see cref="Factory"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeFactory() => false;
    }
}
