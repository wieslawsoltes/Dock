// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.ReactiveProperty.Core;

namespace Dock.Model.ReactiveProperty.Controls;

/// <summary>
/// Uniform grid dock.
/// </summary>
[DataContract(IsReference = true)]
public class UniformGridDock : DockBase, IUniformGridDock
{
    private int _rows;
    private int _columns;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int Rows
    {
        get => _rows;
        set => SetProperty(ref _rows, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int Columns
    {
        get => _columns;
        set => SetProperty(ref _columns, value);
    }
}
