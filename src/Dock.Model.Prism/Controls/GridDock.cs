// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Prism.Core;

namespace Dock.Model.Prism.Controls;

/// <summary>
/// Grid dock.
/// </summary>
public class GridDock : DockBase, IGridDock
{
    private string? _columnDefinitions;
    private string? _rowDefinitions;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public string? ColumnDefinitions
    {
        get => _columnDefinitions;
        set => SetProperty(ref _columnDefinitions, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public string? RowDefinitions
    {
        get => _rowDefinitions;
        set => SetProperty(ref _rowDefinitions, value);
    }
}
