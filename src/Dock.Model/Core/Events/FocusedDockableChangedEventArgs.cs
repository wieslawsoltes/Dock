// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Focused dockable changed event args.
/// </summary>
public record FocusedDockableChangedEventArgs(IDockable? Dockable) : EventArgs;
