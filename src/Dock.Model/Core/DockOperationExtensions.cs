// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace Dock.Model.Core;

internal static class DockOperationExtensions
{
    public static Alignment ToAlignment(this DockOperation operation)
    {
        return operation switch
        {
            DockOperation.Left => Alignment.Left,
            DockOperation.Bottom => Alignment.Bottom,
            DockOperation.Right => Alignment.Right,
            DockOperation.Top => Alignment.Top,
            DockOperation.RootLeft => Alignment.Left,
            DockOperation.RootBottom => Alignment.Bottom,
            DockOperation.RootRight => Alignment.Right,
            DockOperation.RootTop => Alignment.Top,
            _ => Alignment.Unset
        };
    }

    public static DockOperation WithoutRoot(this DockOperation operation)
    {
        return operation switch
        {
            DockOperation.RootLeft => DockOperation.Left,
            DockOperation.RootRight => DockOperation.Right,
            DockOperation.RootTop => DockOperation.Top,
            DockOperation.RootBottom => DockOperation.Bottom,
            _ => operation
        };
    }

    public static bool IsRootOperation(this DockOperation operation)
    {
        return operation is DockOperation.RootLeft
            or DockOperation.RootRight
            or DockOperation.RootTop
            or DockOperation.RootBottom;
    }
}
