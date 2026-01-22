// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Core;
using ReactiveUI.SourceGenerators;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// Wrap dock.
/// </summary>
public partial class WrapDock : DockBase, IWrapDock
{
    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial Orientation Orientation { get; set; }
}
