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
    public static class IDockExtensions
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

            var window = new DockWindow()
            {
                Id = nameof(DockWindow),
                Layout = new DockLayout
                {
                    Id = nameof(DockLayout),
                    CurrentView = view,
                    Views = new ObservableCollection<IDock> { view }
                }
            };

            target.AddWindow(window);
            target.Factory?.Update(window, context);

            return window;
        }
    }
}
