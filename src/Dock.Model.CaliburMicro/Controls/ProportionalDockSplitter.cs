// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.CaliburMicro.Core;

namespace Dock.Model.CaliburMicro.Controls;

/// <summary>
/// Proportional dock splitter.
/// </summary>
[DataContract(IsReference = true)]
public class ProportionalDockSplitter : DockableBase, IProportionalDockSplitter
{
    private bool _canResize = true;
    private bool _resizePreview = false;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanResize
    {
        get => _canResize;
        set => Set(ref _canResize, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool ResizePreview
    {
        get => _resizePreview;
        set => Set(ref _resizePreview, value);
    }
}