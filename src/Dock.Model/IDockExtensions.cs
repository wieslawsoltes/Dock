// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;

namespace Dock.Model
{
    public static class IDockExtensions
    {
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
