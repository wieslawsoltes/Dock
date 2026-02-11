// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Core;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// Tool.
/// </summary>
public partial class Tool : DockableBase, ITool, IDocument, IMdiDocument, IDockingWindowState
{
    private bool _isOpen;
    private bool _isActive;
    private bool _isSelected;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial DockRect MdiBounds { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial MdiWindowState MdiState { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial int MdiZIndex { get; set; }

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

            this.RaiseAndSetIfChanged(ref _isOpen, value);
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

            this.RaiseAndSetIfChanged(ref _isActive, value);
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

            this.RaiseAndSetIfChanged(ref _isSelected, value);
            NotifyDockingWindowStateChanged(DockingWindowStateProperty.IsSelected);
        }
    }
}
