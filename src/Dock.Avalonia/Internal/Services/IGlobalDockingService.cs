// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

internal interface IGlobalDockingService
{
    IDock? ResolveGlobalTargetDock(Control? dropControl);
    bool ShouldUseGlobalOperation(bool hasLocalAdorner, DockOperation localOperation, DockOperation globalOperation);
    bool TryApplyGlobalDockingProportion(IDockable sourceDockable, IDockable? sourceRoot, IDockable? targetRoot, double proportion);
}
