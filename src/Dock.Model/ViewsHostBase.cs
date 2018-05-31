// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using System.Linq;

namespace Dock.Model
{
    /// <summary>
    /// Views host base class.
    /// </summary>
    public abstract class ViewsHostBase : ViewBase, IViewsHost
    {
        private readonly Stack<IView> _back = new Stack<IView>();
        private readonly Stack<IView> _forward = new Stack<IView>();
        private IList<IView> _views;
        private IView _currentView;
        private IView _defaultView;

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
            if (_currentView is IWindowsHost windows)
            {
                if (root == null)
                {
                    windows.HideWindows();
                    CurrentView = null;
                    ResetNavigation();
                }
                else if (root is IDock dock)
                {
                    if (dock != null && dock != _currentView)
                    {
                        windows.HideWindows();
                    }

                    if (dock != null && _currentView != dock)
                    {
                        if (_currentView != null && bSnapshot == true)
                        {
                            MakeSnapshot(_currentView);
                        }

                        CurrentView = dock;
                    }

                    windows.ShowWindows();
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
                        var views = _views.Flatten(v =>
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
        }

        /// <inheritdoc/>
        public void Navigate(object root)
        {
            NavigateImpl(root, true);
        }

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
        /// Check whether the <see cref="DefaultView"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeDefaultView() => _defaultView != null;

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
