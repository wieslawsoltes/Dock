// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.CaliburMicro.Core;

namespace Dock.Model.CaliburMicro.Controls;

/// <summary>
/// Grid dock.
/// </summary>
[DataContract(IsReference = true)]
public class GridDock : DockBase, IGridDock
{
    private string? _columnDefinitions;
    private string? _rowDefinitions;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public string? ColumnDefinitions
    {
        get => _columnDefinitions;
        set => Set(ref _columnDefinitions, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public string? RowDefinitions
    {
        get => _rowDefinitions;
        set => Set(ref _rowDefinitions, value);
    }
}