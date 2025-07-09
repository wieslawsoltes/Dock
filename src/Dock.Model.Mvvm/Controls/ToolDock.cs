// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Core;

namespace Dock.Model.Mvvm.Controls;

/// <summary>
/// Tool dock.
/// </summary>
[DataContract(IsReference = true)]
public class ToolDock : DockBase, IToolDock
{
    private Alignment _alignment = Alignment.Unset;
    private bool _isExpanded;
    private bool _autoHide = true;
    private GripMode _gripMode = GripMode.Visible;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public Alignment Alignment
    {
        get => _alignment;
        set => SetProperty(ref _alignment, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool AutoHide
    {
        get => _autoHide;
        set => SetProperty(ref _autoHide, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public GripMode GripMode
    {
        get => _gripMode;
        set => SetProperty(ref _gripMode, value);
    }
}
