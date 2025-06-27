// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.ReactiveUI.Core;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// Docking panel dock.
/// </summary>
[DataContract(IsReference = true)]
public partial class DockDock : DockBase, IDockDock
{
    /// <summary>
    /// Initializes new instance of the <see cref="DockDock"/> class.
    /// </summary>
    public DockDock()
    {
        _lastChildFill = true;
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool LastChildFill { get; set; }
}
