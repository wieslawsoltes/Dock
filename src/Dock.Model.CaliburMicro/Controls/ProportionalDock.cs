// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.CaliburMicro.Core;

namespace Dock.Model.CaliburMicro.Controls;

/// <summary>
/// Proportional dock.
/// </summary>
[DataContract(IsReference = true)]
public class ProportionalDock : DockBase, IProportionalDock
{
    private Orientation _orientation = Orientation.Horizontal;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public Orientation Orientation
    {
        get => _orientation;
        set => Set(ref _orientation, value);
    }
}