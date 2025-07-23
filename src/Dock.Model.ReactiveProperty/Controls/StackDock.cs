// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveProperty.Core;

namespace Dock.Model.ReactiveProperty.Controls;

/// <summary>
/// Stack dock.
/// </summary>
[DataContract(IsReference = true)]
public class StackDock : DockBase, IStackDock
{
    private Orientation _orientation;
    private double _spacing;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public Orientation Orientation
    {
        get => _orientation;
        set => SetProperty(ref _orientation, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double Spacing
    {
        get => _spacing;
        set => SetProperty(ref _spacing, value);
    }
}
