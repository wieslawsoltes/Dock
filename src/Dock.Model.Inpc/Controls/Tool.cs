// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Inpc.Core;

namespace Dock.Model.Inpc.Controls;

/// <summary>
/// Tool.
/// </summary>
[DataContract(IsReference = true)]
public class Tool : DockableBase, ITool, IDocument, IMdiDocument, IDockingWindowState
{
    private DockRect _mdiBounds;
    private MdiWindowState _mdiState = MdiWindowState.Normal;
    private int _mdiZIndex;
    private bool _isOpen;
    private bool _isActive;
    private bool _isSelected;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DockRect MdiBounds
    {
        get => _mdiBounds;
        set => SetProperty(ref _mdiBounds, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public MdiWindowState MdiState
    {
        get => _mdiState;
        set => SetProperty(ref _mdiState, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int MdiZIndex
    {
        get => _mdiZIndex;
        set => SetProperty(ref _mdiZIndex, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (_isOpen == value)
            {
                return;
            }

            SetProperty(ref _isOpen, value);
            NotifyDockingWindowStateChanged(DockingWindowStateProperty.IsOpen);
        }
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive == value)
            {
                return;
            }

            SetProperty(ref _isActive, value);
            NotifyDockingWindowStateChanged(DockingWindowStateProperty.IsActive);
        }
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
            {
                return;
            }

            SetProperty(ref _isSelected, value);
            NotifyDockingWindowStateChanged(DockingWindowStateProperty.IsSelected);
        }
    }
}
