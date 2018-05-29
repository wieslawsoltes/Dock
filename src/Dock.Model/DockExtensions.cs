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
            return FindRootLayout(dock.Parent);
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
            else
            {
                dock.CurrentView = null;
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

            if (targetDock.Views == null)
            {
                targetDock.Views = new ObservableCollection<IDock>();
            }
            targetDock.Views.Insert(targetIndex, item);

            if (item.Factory is IDockFactory factory)
            {
                factory.Update(item, item.Context, targetDock);
            }

            if (sourceDock.Views.Count > 0)
            {
                sourceDock.CurrentView = sourceDock.Views[sourceIndex > 0 ? sourceIndex - 1 : 0];
            }
            else
            {
                sourceDock.CurrentView = null;
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

            if (item1.Factory is IDockFactory factory1)
            {
                factory1.Update(item1, item1.Context, targetDock);
            }

            if (item2.Factory is IDockFactory factory2)
            {
                factory2.Update(item2, item2.Context, sourceDock);
            }

            sourceDock.CurrentView = item2;
            targetDock.CurrentView = item1;
        }
    }
}
