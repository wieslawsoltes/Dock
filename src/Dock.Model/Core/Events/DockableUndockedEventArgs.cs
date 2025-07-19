// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Dockable undocked event args.
/// </summary>
public record DockableUndockedEventArgs(IDockable? Dockable, DockOperation Operation) : EventArgs;
