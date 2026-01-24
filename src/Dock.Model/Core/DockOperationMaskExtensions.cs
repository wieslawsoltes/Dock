// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace Dock.Model.Core;

/// <summary>
/// Helpers for working with <see cref="DockOperationMask"/>.
/// </summary>
public static class DockOperationMaskExtensions
{
    /// <summary>
    /// Returns true when the mask allows the supplied dock operation.
    /// </summary>
    /// <param name="mask">The mask to check.</param>
    /// <param name="operation">The dock operation.</param>
    /// <returns>True when the operation is allowed.</returns>
    public static bool Allows(this DockOperationMask mask, DockOperation operation)
    {
        return operation switch
        {
            DockOperation.Fill => mask.HasFlag(DockOperationMask.Fill),
            DockOperation.Left => mask.HasFlag(DockOperationMask.Left),
            DockOperation.Right => mask.HasFlag(DockOperationMask.Right),
            DockOperation.Top => mask.HasFlag(DockOperationMask.Top),
            DockOperation.Bottom => mask.HasFlag(DockOperationMask.Bottom),
            DockOperation.Window => mask.HasFlag(DockOperationMask.Window),
            _ => false
        };
    }
}
