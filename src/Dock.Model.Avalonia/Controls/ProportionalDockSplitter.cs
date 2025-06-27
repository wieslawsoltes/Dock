// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Runtime.Serialization;
using Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Proportional dock splitter.
/// </summary>
[DataContract(IsReference = true)]
public class ProportionalDockSplitter : DockBase, IProportionalDockSplitter
{
    /// <summary>
    /// Initializes new instance of the <see cref="ProportionalDockSplitter"/> class.
    /// </summary>
    public ProportionalDockSplitter()
    {
    }
}
