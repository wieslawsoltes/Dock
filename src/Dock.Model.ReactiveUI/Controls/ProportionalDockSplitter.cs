// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.ReactiveUI.Core;
using ReactiveUI.SourceGenerators;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// Proportional dock splitter.
/// </summary>
public partial class ProportionalDockSplitter : DockableBase, IProportionalDockSplitter
{
    /// <summary>
    /// Initializes new instance of the <see cref="ProportionalDockSplitter"/> class.
    /// </summary>
    public ProportionalDockSplitter()
    {
        _canResize = true;
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool CanResize { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool ResizePreview { get; set; }
}
