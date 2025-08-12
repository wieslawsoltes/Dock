// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.CaliburMicro.Core;

namespace Dock.Model.CaliburMicro.Controls;

/// <summary>
/// Dock dock.
/// </summary>
[DataContract(IsReference = true)]
public class DockDock : DockBase, IDockDock
{
    private bool _lastChildFill = true;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool LastChildFill
    {
        get => _lastChildFill;
        set => Set(ref _lastChildFill, value);
    }
}