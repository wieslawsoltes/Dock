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
            DockOperation.GlobalLeft => Alignment.Left,
            DockOperation.GlobalBottom => Alignment.Bottom,
            DockOperation.GlobalRight => Alignment.Right,
            DockOperation.GlobalTop => Alignment.Top,
            _ => Alignment.Unset
        };
    }

    public static DockOperation ToLocal(this DockOperation operation)
    {
        return operation switch
        {
            DockOperation.GlobalLeft => DockOperation.Left,
            DockOperation.GlobalBottom => DockOperation.Bottom,
            DockOperation.GlobalRight => DockOperation.Right,
            DockOperation.GlobalTop => DockOperation.Top,
            _ => operation
        };
    }
}
