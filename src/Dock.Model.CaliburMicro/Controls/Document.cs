// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.CaliburMicro.Core;

namespace Dock.Model.CaliburMicro.Controls;

/// <summary>
/// Document.
/// </summary>
[DataContract(IsReference = true)]
public class Document : DockableBase, IMdiDocument
{
    private DockRect _mdiBounds;
    private MdiWindowState _mdiState = MdiWindowState.Normal;
    private int _mdiZIndex;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DockRect MdiBounds
    {
        get => _mdiBounds;
        set => Set(ref _mdiBounds, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public MdiWindowState MdiState
    {
        get => _mdiState;
        set => Set(ref _mdiState, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int MdiZIndex
    {
        get => _mdiZIndex;
        set => Set(ref _mdiZIndex, value);
    }
}
