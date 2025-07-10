// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="GlobalDockTarget"/> xaml.
/// </summary>
public class GlobalDockTarget : DockTargetBase
{
    /// <inheritdoc />
    protected override DockOperation DefaultDockOperation => DockOperation.None;
}
