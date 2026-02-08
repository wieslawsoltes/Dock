// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

internal interface IGlobalDockOperationSelector
{
    bool ShouldUseGlobalOperation(bool hasLocalAdorner, DockOperation localOperation, DockOperation globalOperation);
}
