// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using Dock.Model.Controls;

namespace Dock.Model
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<T>? Flatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>?> f)
        {
            return e.SelectMany(c =>
            {
                var r = f(c);
                if (r is null)
                    return Enumerable.Empty<T>();
                else
                    return r.Flatten(f);
            }).Concat(e);
        }
    }

    /// <summary>
    /// Navigate adapter for the <see cref="IDock"/>.
    /// </summary>
    public class NavigateAdapter : INavigateAdapter
    {
        private readonly Stack<IDockable> _back;
        private readonly Stack<IDockable> _forward;
        private readonly IDock _dock;

        /// <summary>
        /// Initializes new instance of the <see cref="NavigateAdapter"/> class.
        /// </summary>
        /// <param name="dock">The dock instance.</param>
        public NavigateAdapter(IDock dock)
        {
            _back = new Stack<IDockable>();
            _forward = new Stack<IDockable>();
            _dock = dock;
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
                if (!(_dock.ActiveDockable is null))
                {
                    _forward.Push(_dock.ActiveDockable);
                }
                Navigate(root, false);
            }
        }

        /// <inheritdoc/>
        public void GoForward()
        {
            if (_forward.Count > 0)
            {
                var root = _forward.Pop();
                if (!(_dock.ActiveDockable is null))
                {
                    _back.Push(_dock.ActiveDockable);
                }
                Navigate(root, false);
            }
        }

        /// <inheritdoc/>
        public void Navigate(object root, bool bSnapshot)
        {
            switch (root)
            {
                case null:
                    ResetActiveDockable();
                    ResetBack();
                    break;
                case IDockable dockable:
                    NavigateTo(dockable, bSnapshot);
                    break;
                case string id:
                    NavigateToUseVisible(id, bSnapshot);
                    break;
                default:
                    throw new ArgumentException($"Invalid root object type: {root.GetType()}");
            }
        }

        private void ResetActiveDockable()
        {
            if (_dock.ActiveDockable is IDock activeDockableWindows)
            {
                activeDockableWindows.Close();
                _dock.ActiveDockable = null;
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

        private void PushBack(IDockable dockable)
        {
            if (_forward.Count > 0)
                _forward.Clear();
            _back.Push(dockable);
        }

        private void NavigateTo(IDockable dockable, bool bSnapshot)
        {
            if (_dock.ActiveDockable is IDock previousDock)
            {
                previousDock.Close();
            }

            if (!(dockable is null) && _dock.ActiveDockable != dockable)
            {
                if (!(_dock.ActiveDockable is null) && bSnapshot == true)
                {
                    PushBack(_dock.ActiveDockable);
                }
                if (_dock.Factory is IFactory factory)
                {
                    factory.SetActiveDockable(dockable);
                }
                else
                {
                    _dock.ActiveDockable = dockable;
                }
            }

            if (dockable is IRootDock nextDock)
            {
                nextDock.ShowWindows();
            }
        }

        private void NavigateToUseVisible(string id, bool bSnapshot)
        {
            var result = _dock.VisibleDockables.FirstOrDefault(v => v.Id == id);
            if (!(result is null))
            {
                Navigate(result, bSnapshot);
            }
            else
            {
                NavigateToUseAllVisible(id, bSnapshot);
            }
        }

        private void NavigateToUseAllVisible(string id, bool bSnapshot)
        {
            if (_dock.VisibleDockables is null)
            {
                return;
            }

            var visible = _dock.VisibleDockables.Flatten(v =>
            {
                if (v is IDock n)
                {
                    return n.VisibleDockables;
                }
                return null;
            });

            var result = visible?.FirstOrDefault(v => v.Id == id);
            if (!(result is null))
            {
                Navigate(result, bSnapshot);
            }
        }

        /// <inheritdoc/>
        public void ShowWindows()
        {
            if (_dock is IRootDock rootDock && !(rootDock.Windows is null))
            {
                foreach (var window in rootDock.Windows)
                {
                    window.Present(false);
                }
            }
            if (_dock.ActiveDockable is IRootDock activeRootDockWindows)
            {
                activeRootDockWindows.ShowWindows();
            }
        }

        /// <inheritdoc/>
        public void ExitWindows()
        {
            if (_dock is IRootDock rootDock && !(rootDock.Windows is null))
            {
                foreach (var window in rootDock.Windows)
                {
                    window.Save();
                    window.Exit();
                }
            }
            if (_dock.ActiveDockable is IRootDock activeRootDockWindows)
            {
                activeRootDockWindows.ExitWindows();
            }
        }

        /// <inheritdoc/>
        public void Close()
        {
            if (_dock is IRootDock rootDock)
            {
                rootDock.ExitWindows();
            }
        }
    }
}
