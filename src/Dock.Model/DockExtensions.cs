// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dock.Model
{
    /// <summary>
    /// WIP: Defines the available layout split direction.
    /// </summary>
    public enum DockOperation
    {
        Fill,
        Left,
        Bottom,
        Right,
        Top,
        Window
    }

    /// <summary>
    /// Dock extension methods.
    /// </summary>
    public static class DockExtensions
    {
        /// <summary>
        /// Prints dock layout tree to console.
        /// </summary>
        /// <param name="dock">The views dock.</param>
        /// <param name="indent">The indent prefix.</param>
        /// <param name="bRecursive">The recursive flag.</param>
        public static void Print(this IDock dock, string indent = "", bool bRecursive = true)
        {
            Console.WriteLine($"{indent}- {dock}");
            Console.WriteLine($"{indent}  [Title]\t{dock.Title}");
            Console.WriteLine($"{indent}  [Dock]\t{dock.Dock}");
            Console.WriteLine($"{indent}  [Width]\t{dock.Width}");
            Console.WriteLine($"{indent}  [Height]\t{dock.Height}");

            if (dock.Views != null && bRecursive == true)
            {
                foreach (var view in dock.Views)
                {
                    Print(view, indent + "  ");
                }
            }
        }

        /// <summary>
        /// Resets dock layout size.
        /// </summary>
        /// <param name="dock">The views dock.</param>
        /// <param name="width">The default width.</param>
        /// <param name="height">The default height.</param>
        /// <param name="bRecursive">The recursive flag.</param>
        public static void Reset(this IDock dock, double width = double.NaN, double height = double.NaN, bool bRecursive = true)
        {
            dock.Width = width;
            dock.Height = height;

            if (dock.Views != null && bRecursive == true)
            {
                foreach (var view in dock.Views)
                {
                    Reset(view, width, height);
                }
            }
        }

        /// <summary>
        /// Remove view from the dock.
        /// </summary>
        /// <param name="dock">The views dock.</param>
        /// <param name="index">The source view index.</param>
        public static void RemoveView(this IDock dock, int index)
        {
            dock.Views.RemoveAt(index);

            if (dock.Views.Count > 0)
            {
                dock.CurrentView = dock.Views[index > 0 ? index - 1 : 0];
            }
        }

        /// <summary>
        /// Move views in the dock.
        /// </summary>
        /// <param name="dock">The views dock.</param>
        /// <param name="sourceIndex">The source view index.</param>
        /// <param name="targetIndex">The target view index.</param>
        public static void MoveView(this IDock dock, int sourceIndex, int targetIndex)
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

        /// <summary>
        /// Swap views in the dock.
        /// </summary>
        /// <param name="dock">The views dock.</param>
        /// <param name="sourceIndex">The source view index.</param>
        /// <param name="targetIndex">The target view index.</param>
        public static void SwapView(this IDock dock, int sourceIndex, int targetIndex)
        {
            var item1 = dock.Views[sourceIndex];
            var item2 = dock.Views[targetIndex];
            dock.Views[targetIndex] = item1;
            dock.Views[sourceIndex] = item2;
            dock.CurrentView = item2;
        }

        /// <summary>
        /// Move views into another dock.
        /// </summary>
        /// <param name="sourceDock">The source views dock.</param>
        /// <param name="targetDock">The target views dock.</param>
        /// <param name="sourceIndex">The source view index.</param>
        /// <param name="targetIndex">The target view index.</param>
        public static void MoveView(this IDock sourceDock, IDock targetDock, int sourceIndex, int targetIndex)
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

        /// <summary>
        /// Swap views into another dock.
        /// </summary>
        /// <param name="sourceDock">The source views dock.</param>
        /// <param name="targetDock">The target views dock.</param>
        /// <param name="sourceIndex">The source view index.</param>
        /// <param name="targetIndex">The target view index.</param>
        public static void SwapView(this IDock sourceDock, IDock targetDock, int sourceIndex, int targetIndex)
        {
            var item1 = sourceDock.Views[sourceIndex];
            var item2 = targetDock.Views[targetIndex];
            sourceDock.Views[sourceIndex] = item2;
            targetDock.Views[targetIndex] = item1;

            sourceDock.CurrentView = item2;
            targetDock.CurrentView = item1;
        }

        /// <summary>
        /// Adds window to dock windows list.
        /// </summary>
        /// <param name="dock">The views dock.</param>
        /// <param name="window">The window to add.</param>
        public static void AddWindow(this IDock dock, IDockWindow window)
        {
            if (dock.Windows == null)
            {
                dock.Windows = new ObservableCollection<IDockWindow>();
            }
            dock.Windows?.Add(window);
        }

        /// <summary>
        /// Removes window from windows list.
        /// </summary>
        /// <param name="dock">The views dock.</param>
        /// <param name="window">The window to remove.</param>
        public static void RemoveWindow(this IDock dock, IDockWindow window)
        {
            dock.Windows?.Remove(window);
        }

        /// <summary>
        /// Creates dock window from view.
        /// </summary>
        /// <param name="target">The target dock.</param>
        /// <param name="source">The source dock.</param>
        /// <param name="sourceIndex">The source view index.</param>
        /// <param name="context">The context object.</param>
        /// <returns>The new instance of the <see cref="IDockWindow"/> class.</returns>
        public static IDockWindow CreateWindow(this IDock target, IDock source, int sourceIndex, object context)
        {
            var view = source.Views[sourceIndex];

            source.RemoveView(sourceIndex);

            var dockStrip = new DockStrip
            {
                Id = nameof(DockStrip),
                CurrentView = view,
                Views = new ObservableCollection<IDock> { view }
            };

            var dockLayout = new DockLayout
            {
                Id = nameof(DockLayout),
                CurrentView = dockStrip,
                Views = new ObservableCollection<IDock> { dockStrip }
            };

            var window = new DockWindow()
            {
                Id = nameof(DockWindow),
                Layout = dockLayout
            };

            target.AddWindow(window);
            target.Factory?.Update(window, context);

            return window;
        }

        /// <summary>
        /// WIP: Replaces view.
        /// </summary>
        /// <param name="root">The root dock.</param>
        /// <param name="source">The source dock.</param>
        /// <param name="target">The target dock.</param>
        /// <returns>True when view was replaced, otherwise false.</returns>
        public static bool ReplaceView(this IDock root, IDock source, IDock target)
        {
            if (root.Views != null)
            {
                for (int i = 0; i < root.Views.Count; i++)
                {
                    if (root.Views[i] == source)
                    {
                        root.Views[i] = target;
                        return true;
                    }
                    if (ReplaceView(root.Views[i], source, target) == true)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// WIP: Creates a new split layout.
        /// </summary>
        /// <param name="target">The target dock.</param>
        /// <param name="context">The context object.</param>
        /// <param name="direction">The split direction.</param>
        /// <returns>The new instance of the <see cref="IDock"/> class.</returns>
        public static IDock SplitLayout(this IDock target, object context, DockOperation direction)
        {
            IDock layout1 = null;
            IDock layout2 = null;
            double width = double.NaN;
            double height = double.NaN;
            string dock = "";

            switch (direction)
            {
                case DockOperation.Left:
                case DockOperation.Right:
                    width = target.Width == double.NaN ? double.NaN : target.Width / 2.0;
                    height = target.Height == double.NaN ? double.NaN : target.Height;
                    break;
                case DockOperation.Top:
                case DockOperation.Bottom:
                    width = target.Width == double.NaN ? double.NaN : target.Width;
                    height = target.Height == double.NaN ? double.NaN : target.Height / 2.0;
                    dock = "Top";
                    break;
            }

            IDock emptyStrip = new DockStrip
            {
                Id = nameof(DockStrip),
                Width = width,
                Height = height,
                CurrentView = null,
                Views = new ObservableCollection<IDock>()
            };

            IDock targetStrip = new DockStrip
            {
                Id = target.Id,
                Width = width,
                Height = height,
                CurrentView = target.CurrentView,
                Views = target.Views
            };

            switch (direction)
            {
                case DockOperation.Left:
                case DockOperation.Top:
                    {
                        layout1 = new DockLayout
                        {
                            Id = nameof(DockLayout),
                            CurrentView = null,
                            Views = new ObservableCollection<IDock> { targetStrip }
                        };

                        layout2 = new DockLayout
                        {
                            Id = nameof(DockLayout),
                            CurrentView = null,
                            Views = new ObservableCollection<IDock> { emptyStrip }
                        };
                    }
                    break;
                case DockOperation.Right:
                case DockOperation.Bottom:
                    {
                        layout1 = new DockLayout
                        {
                            Id = nameof(DockLayout),
                            CurrentView = null,
                            Views = new ObservableCollection<IDock> { emptyStrip }
                        };

                        layout2 = new DockLayout
                        {
                            Id = nameof(DockLayout),
                            CurrentView = null,
                            Views = new ObservableCollection<IDock> { targetStrip }
                        };
                    }
                    break;
            }

            switch (direction)
            {
                case DockOperation.Left:
                case DockOperation.Right:
                    layout1.Dock = "Left";
                    layout2.Dock = "Right";
                    dock = "Left";
                    break;
                case DockOperation.Top:
                case DockOperation.Bottom:
                    layout1.Dock = "Top";
                    layout2.Dock = "Bottom";
                    dock = "Top";
                    break;
            }

            var splitLayout = new DockLayout
            {
                Id = nameof(DockLayout),
                Dock = "",
                Width = double.NaN,
                Height = double.NaN,
                Title = target.Title,
                CurrentView = null,
                Views = new ObservableCollection<IDock>
                {
                    layout1,
                    new DockSplitter()
                    {
                        Id = nameof(DockSplitter),
                        Dock = dock
                    },
                    layout2
                }
            };

            target.Factory?.Update(splitLayout, context);

            return splitLayout;
        }
    }
}
