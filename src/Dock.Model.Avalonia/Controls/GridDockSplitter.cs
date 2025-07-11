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
/// Grid dock splitter.
/// </summary>
[DataContract(IsReference = true)]
public class GridDockSplitter : DockableBase, IGridDockSplitter
{
    /// <summary>
    /// Defines the <see cref="ResizeDirection"/> property.
    /// </summary>
    public static readonly DirectProperty<GridDockSplitter, GridResizeDirection> ResizeDirectionProperty =
        AvaloniaProperty.RegisterDirect<GridDockSplitter, GridResizeDirection>(nameof(ResizeDirection), o => o.ResizeDirection, (o, v) => o.ResizeDirection = v);

    private GridResizeDirection _resizeDirection;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("ResizeDirection")]
    public GridResizeDirection ResizeDirection
    {
        get => _resizeDirection;
        set => SetAndRaise(ResizeDirectionProperty, ref _resizeDirection, value);
    }
}
