// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core.Events;

/// <summary>
/// Window opened event args.
/// </summary>
public record WindowOpenedEventArgs(IDockWindow? Window) : EventArgs;
