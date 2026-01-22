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
/// Stack dock.
/// </summary>
public class StackDock : DockBase, IStackDock
{
    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly DirectProperty<StackDock, Orientation> OrientationProperty =
        AvaloniaProperty.RegisterDirect<StackDock, Orientation>(nameof(Orientation), o => o.Orientation, (o, v) => o.Orientation = v);

    /// <summary>
    /// Defines the <see cref="Spacing"/> property.
    /// </summary>
    public static readonly DirectProperty<StackDock, double> SpacingProperty =
        AvaloniaProperty.RegisterDirect<StackDock, double>(nameof(Spacing), o => o.Spacing, (o, v) => o.Spacing = v);

    private Orientation _orientation;
    private double _spacing;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Orientation")]
    public Orientation Orientation
    {
        get => _orientation;
        set => SetAndRaise(OrientationProperty, ref _orientation, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Spacing")]
    public double Spacing
    {
        get => _spacing;
        set => SetAndRaise(SpacingProperty, ref _spacing, value);
    }
}
