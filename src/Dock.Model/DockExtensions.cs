// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;

namespace Dock.Model
{
    /// <summary>
    /// Dock extension methods.
    /// </summary>
    public static class DockExtensions
    {
        /// <summary>
        /// Prints dock tree to console.
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
        /// Resets dock size.
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
    }
}
