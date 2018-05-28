// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dock.Model
{
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
        /// Searches for root layout.
        /// </summary>
        /// <param name="dock">The dock to find root for.</param>
        /// <returns>The root layout instance or null if root layout was not found.</returns>
        public static IDock FindRootLayout(this IDock dock)
        {
            if (dock.Parent == null)
            {
                return dock;
            }

            if (dock.Views != null)
            {
                foreach (var view in dock.Views)
                {
                    if (view.Parent == null)
                    {
                        return view;
                    }

                    var result = view.FindRootLayout();
                    if (result == null)
                    {
                        return result;
                    }
                }
            }

            return null;
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

            if (target.Factory is IDockFactory factory)
            {
                factory.Update(window, context, target);
            }

            return window;
        }

        /// <summary>
        /// Creates a new split layout.
        /// </summary>
        /// <param name="target">The target dock.</param>
        /// <param name="context">The context object.</param>
        /// <param name="operation">The dock operation.</param>
        /// <returns>The new instance of the <see cref="IDock"/> class.</returns>
        public static IDock SplitLayout(this IDock target, object context, DockOperation operation)
        {
            double width = double.NaN;
            double height = double.NaN;
            string originalDock = target.Dock;
            double originalWidth = target.Width;
            double originalHeight = target.Height;

            switch (operation)
            {
                case DockOperation.Left:
                case DockOperation.Right:
                    width = originalWidth == double.NaN ? double.NaN : originalWidth / 2.0;
                    height = target.Height == double.NaN ? double.NaN : target.Height;
                    break;
                case DockOperation.Top:
                case DockOperation.Bottom:
                    width = originalWidth == double.NaN ? double.NaN : originalWidth;
                    height = originalHeight == double.NaN ? double.NaN : originalHeight / 2.0;
                    break;
            }

            IDock split = new DockLayout
            {
                Id = nameof(DockLayout),
                Title = nameof(DockLayout),
                Width = width,
                Height = height,
                CurrentView = null,
                Views = null
            };

            switch (operation)
            {
                case DockOperation.Left:
                    target.Dock = "Left";
                    target.Width = width;
                    split.Dock = "Right";
                    split.Width = width;
                    break;
                case DockOperation.Right:
                    target.Dock = "Right";
                    target.Width = width;
                    split.Dock = "Left";
                    split.Width = width;
                    break;
                case DockOperation.Top:
                    target.Dock = "Top";
                    target.Height = height;
                    split.Dock = "Bottom";
                    split.Height = height;
                    break;
                case DockOperation.Bottom:
                    target.Dock = "Bottom";
                    target.Height = height;
                    split.Dock = "Top";
                    split.Height = height;
                    break;
            }

            var layout = new DockLayout
            {
                Id = nameof(DockLayout),
                Dock = originalDock,
                Width = originalWidth,
                Height = originalHeight,
                Title = nameof(DockLayout),
                CurrentView = null,
                Views = new ObservableCollection<IDock>
                {
                    (target.Dock == "Left" || target.Dock == "Top") ? target : split,
                    new DockSplitter()
                    {
                        Id = nameof(DockSplitter),
                        Title = nameof(DockSplitter),
                        Dock = (target.Dock == "Left" || target.Dock == "Right") ? "Left" : "Top",
                        Width = double.NaN,
                        Height = double.NaN,
                    },
                    (target.Dock == "Right" || target.Dock == "Bottom") ? target : split,
                }
            };

            return layout;
        }
    }
}
