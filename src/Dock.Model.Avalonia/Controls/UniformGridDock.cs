// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Uniform grid dock.
/// </summary>
public class UniformGridDock : DockBase, IUniformGridDock
{
    /// <summary>
    /// Defines the <see cref="Rows"/> property.
    /// </summary>
    public static readonly DirectProperty<UniformGridDock, int> RowsProperty =
        AvaloniaProperty.RegisterDirect<UniformGridDock, int>(nameof(Rows), o => o.Rows, (o, v) => o.Rows = v);

    /// <summary>
    /// Defines the <see cref="Columns"/> property.
    /// </summary>
    public static readonly DirectProperty<UniformGridDock, int> ColumnsProperty =
        AvaloniaProperty.RegisterDirect<UniformGridDock, int>(nameof(Columns), o => o.Columns, (o, v) => o.Columns = v);

    private int _rows;
    private int _columns;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Rows")]
    public int Rows
    {
        get => _rows;
        set => SetAndRaise(RowsProperty, ref _rows, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Columns")]
    public int Columns
    {
        get => _columns;
        set => SetAndRaise(ColumnsProperty, ref _columns, value);
    }
}
