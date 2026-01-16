// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.ReactiveUI.Core;
using ReactiveUI.SourceGenerators;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// Uniform grid dock.
/// </summary>
[DataContract(IsReference = true)]
public partial class UniformGridDock : DockBase, IUniformGridDock
{
    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial int Rows { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial int Columns { get; set; }
}
