// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.CaliburMicro.Core;

namespace Dock.Model.CaliburMicro.Controls;

/// <summary>
/// Uniform grid dock.
/// </summary>
[DataContract(IsReference = true)]
public class UniformGridDock : DockBase, IUniformGridDock
{
    private int _columns;
    private int _rows;
    private int _firstColumn;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int Columns
    {
        get => _columns;
        set => Set(ref _columns, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int Rows
    {
        get => _rows;
        set => Set(ref _rows, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int FirstColumn
    {
        get => _firstColumn;
        set => Set(ref _firstColumn, value);
    }
}