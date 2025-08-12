// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.CaliburMicro.Core;

namespace Dock.Model.CaliburMicro.Controls;

/// <summary>
/// Tool dock.
/// </summary>
[DataContract(IsReference = true)]
public class ToolDock : DockBase, IToolDock
{
    private Alignment _alignment = Alignment.Unset;
    private bool _isExpanded = true;
    private bool _autoHide = true;
    private GripMode _gripMode = GripMode.Visible;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public Alignment Alignment
    {
        get => _alignment;
        set => Set(ref _alignment, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsExpanded
    {
        get => _isExpanded;
        set => Set(ref _isExpanded, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool AutoHide
    {
        get => _autoHide;
        set => Set(ref _autoHide, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public GripMode GripMode
    {
        get => _gripMode;
        set => Set(ref _gripMode, value);
    }

    /// <inheritdoc/>
    public void AddTool(IDockable tool)
    {
        // Implementation would be provided by the factory or dock manager
    }
}