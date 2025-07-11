// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.VisualTree;
using Dock.Model.Core;

namespace Dock.Avalonia.Contract;

/// <summary>
/// Represents a delegate used to evaluate dock operations.
/// </summary>
/// <param name="point">Pointer location relative to <paramref name="relativeTo"/>.</param>
/// <param name="operation">The dock operation being evaluated.</param>
/// <param name="dragAction">Current drag action.</param>
/// <param name="relativeTo">The visual relative to which the point is measured.</param>
/// <returns><c>true</c> when the operation is valid.</returns>
public delegate bool DockOperationHandler(Point point, DockOperation operation, DragAction dragAction, Visual relativeTo);
