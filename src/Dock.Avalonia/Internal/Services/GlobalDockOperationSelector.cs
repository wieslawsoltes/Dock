// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

internal sealed class GlobalDockOperationSelector : IGlobalDockOperationSelector
{
    public static readonly IGlobalDockOperationSelector Instance = new GlobalDockOperationSelector();

    public bool ShouldUseGlobalOperation(bool hasLocalAdorner, DockOperation localOperation, DockOperation globalOperation)
    {
        if (globalOperation == DockOperation.None)
        {
            return false;
        }

        if (!hasLocalAdorner)
        {
            return true;
        }

        return localOperation == DockOperation.Window;
    }
}
