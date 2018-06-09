// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dock.Model
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f)
        {
            return e.SelectMany(c =>
            {
                IEnumerable<T> r = f(c);
                if (r == null)
                    return Enumerable.Empty<T>();
                else
                    return r.Flatten(f);
            }).Concat(e);
        }
    }

    /// <summary>
    /// Navigate adapter for the <see cref="IDock"/>.
    /// </summary>
    public class NavigateAdapter
    {
        private readonly Stack<IView> _back;
        private readonly Stack<IView> _forward;
        private readonly IDock _dock;

        /// <summary>
        /// Initializes new instance of the <see cref="NavigateAdapter"/> class.
        /// </summary>
        /// <param name="dock">The dock instance.</param>
        public NavigateAdapter(IDock dock)
        {
            _back = new Stack<IView>();
            _forward = new Stack<IView>();
            _dock = dock;
        }

        /// <summary>
        /// Gets a value that indicates whether there is at least one entry in back navigation history.
        /// </summary>
        public bool CanGoBack => _back.Count > 0;

        /// <summary>
        /// Gets a value that indicates whether there is at least one entry in forward navigation history.
        /// </summary>
        public bool CanGoForward => _forward.Count > 0;

        /// <summary>
        /// Navigates to the most recent entry in back navigation history, if there is one.
        /// </summary>
        public void GoBack()
        {
            if (_back.Count > 0)
            {
                var root = _back.Pop();
                if (_dock.CurrentView != null)
                {
                    _forward.Push(_dock.CurrentView);
                }
                Navigate(root, false);
            }
        }

        /// <summary>
        /// Navigate to the most recent entry in forward navigation history, if there is one.
        /// </summary>
        public void GoForward()
        {
            if (_forward.Count > 0)
            {
                var root = _forward.Pop();
                if (_dock.CurrentView != null)
                {
                    _back.Push(_dock.CurrentView);
                }
                Navigate(root, false);
            }
        }

        /// <summary>
        /// Implementation of the <see cref="IViewsHost.Navigate(object)"/> method.
        /// </summary>
        /// <param name="root">An object that contains the content to navigate to.</param>
        /// <param name="bSnapshot">The lag indicating whether to make snapshot.</param>
        public void Navigate(object root, bool bSnapshot)
        {
            switch (root)
            {
                case null:
                    ResetCurrentView();
                    ResetBack();
                    break;
                case IView view:
                    NavigateTo(view, bSnapshot);
                    break;
                case string id:
                    NavigateToUseViews(id, bSnapshot);
                    break;
                default:
                    throw new ArgumentException($"Invalid root object type: {root.GetType()}");
            }
        }

        private void ResetCurrentView()
        {
            if (_dock.CurrentView is IWindowsHost currentViewWindows)
            {
                currentViewWindows.HideWindows();
                _dock.CurrentView = null;
            }
        }

        private void ResetBack()
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

        private void PushBack(IView view)
        {
            if (_forward.Count > 0)
                _forward.Clear();
            _back.Push(view);
        }

        private void NavigateTo(IView view, bool bSnapshot)
        {
            if (_dock.CurrentView is IWindowsHost currentViewWindows)
            {
                currentViewWindows.HideWindows();
            }

            if (view != null && _dock.CurrentView != view)
            {
                if (_dock.CurrentView != null && bSnapshot == true)
                {
                    PushBack(_dock.CurrentView);
                }

                _dock.CurrentView = view;
            }

            if (view is IWindowsHost dockWindows)
            {
                dockWindows.ShowWindows();
            }
        }

        private void NavigateToUseViews(string id, bool bSnapshot)
        {
            var result = _dock.Views.FirstOrDefault(v => v.Id == id);
            if (result != null)
            {
                Navigate(result, bSnapshot);
            }
            else
            {
                NavigateToUseAllViews(id, bSnapshot);
            }
        }

        private void NavigateToUseAllViews(string id, bool bSnapshot)
        {
            var views = _dock.Views.Flatten(v =>
            {
                if (v is IViewsHost n)
                {
                    return n.Views;
                }
                return null;
            });
            var result = views.FirstOrDefault(v => v.Id == id);
            if (result != null)
            {
                Navigate(result, bSnapshot);
            }
        }
    }
}
