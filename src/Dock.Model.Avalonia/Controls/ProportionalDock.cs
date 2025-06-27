// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Proportional dock.
/// </summary>
[DataContract(IsReference = true)]
public class ProportionalDock : DockBase, IProportionalDock
{
    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly DirectProperty<ProportionalDock, Orientation> OrientationProperty =
        AvaloniaProperty.RegisterDirect<ProportionalDock, Orientation>(nameof(Orientation), o => o.Orientation, (o, v) => o.Orientation = v);

    private Orientation _orientation;

    /// <summary>
    /// Initializes new instance of the <see cref="ProportionalDock"/> class.
    /// </summary>
    public ProportionalDock()
    {
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Orientation")]
    public Orientation Orientation
    {
        get => _orientation;
        set => SetAndRaise(OrientationProperty, ref _orientation, value);
    }
}
