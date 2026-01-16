// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Core;
using ReactiveUI.SourceGenerators;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// Stack dock.
/// </summary>
[DataContract(IsReference = true)]
public partial class StackDock : DockBase, IStackDock
{
    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial Orientation Orientation { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial double Spacing { get; set; }
}
