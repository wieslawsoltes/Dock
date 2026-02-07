// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Dock.Avalonia.Internal;

internal static class TabStripMouseWheelScrollHelper
{
    public static bool TryHandle(ScrollViewer? scrollViewer, Orientation orientation, Vector delta)
    {
        if (scrollViewer is null)
        {
            return false;
        }

        var deltaY = delta.Y;
        if (Math.Abs(deltaY) <= double.Epsilon)
        {
            return false;
        }

        var steps = Math.Max(1, (int)Math.Ceiling(Math.Abs(deltaY)));

        switch (orientation)
        {
            case Orientation.Horizontal:
                if (scrollViewer.Extent.Width <= scrollViewer.Viewport.Width)
                {
                    return false;
                }

                var initialHorizontalOffset = scrollViewer.Offset;
                for (var i = 0; i < steps; i++)
                {
                    if (deltaY > 0)
                    {
                        scrollViewer.LineLeft();
                    }
                    else
                    {
                        scrollViewer.LineRight();
                    }
                }

                return !scrollViewer.Offset.Equals(initialHorizontalOffset);
            case Orientation.Vertical:
                if (scrollViewer.Extent.Height <= scrollViewer.Viewport.Height)
                {
                    return false;
                }

                var initialVerticalOffset = scrollViewer.Offset;
                for (var i = 0; i < steps; i++)
                {
                    if (deltaY > 0)
                    {
                        scrollViewer.LineUp();
                    }
                    else
                    {
                        scrollViewer.LineDown();
                    }
                }

                return !scrollViewer.Offset.Equals(initialVerticalOffset);
            default:
                return false;
        }
    }
}
