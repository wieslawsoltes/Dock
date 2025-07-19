// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Active dockable changed event args.
/// </summary>
public record ActiveDockableChangedEventArgs(IDockable? Dockable) : EventArgs;
