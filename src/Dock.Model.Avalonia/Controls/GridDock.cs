// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Grid dock.
/// </summary>
public class GridDock : DockBase, IGridDock
{
    /// <summary>
    /// Defines the <see cref="ColumnDefinitions"/> property.
    /// </summary>
    public static readonly DirectProperty<GridDock, string?> ColumnDefinitionsProperty =
        AvaloniaProperty.RegisterDirect<GridDock, string?>(nameof(ColumnDefinitions), o => o.ColumnDefinitions, (o, v) => o.ColumnDefinitions = v);

    /// <summary>
    /// Defines the <see cref="RowDefinitions"/> property.
    /// </summary>
    public static readonly DirectProperty<GridDock, string?> RowDefinitionsProperty =
        AvaloniaProperty.RegisterDirect<GridDock, string?>(nameof(RowDefinitions), o => o.RowDefinitions, (o, v) => o.RowDefinitions = v);

    private string? _columnDefinitions;
    private string? _rowDefinitions;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("ColumnDefinitions")]
    public string? ColumnDefinitions
    {
        get => _columnDefinitions;
        set => SetAndRaise(ColumnDefinitionsProperty, ref _columnDefinitions, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("RowDefinitions")]
    public string? RowDefinitions
    {
        get => _rowDefinitions;
        set => SetAndRaise(RowDefinitionsProperty, ref _rowDefinitions, value);
    }
}
