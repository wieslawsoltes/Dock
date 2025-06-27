// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.ReactiveUI.Core;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// Proportional dock splitter.
/// </summary>
[DataContract(IsReference = true)]
public partial class ProportionalDockSplitter : DockableBase, IProportionalDockSplitter
{
}
