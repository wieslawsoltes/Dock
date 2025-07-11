// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Prism.Core;
using Dock.Model.Core;

namespace Dock.Model.Prism.Controls;

/// <summary>
/// Grid dock splitter.
/// </summary>
[DataContract(IsReference = true)]
public class GridDockSplitter : DockableBase, IGridDockSplitter
{
    private GridResizeDirection _resizeDirection;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public GridResizeDirection ResizeDirection
    {
        get => _resizeDirection;
        set => SetProperty(ref _resizeDirection, value);
    }
}
