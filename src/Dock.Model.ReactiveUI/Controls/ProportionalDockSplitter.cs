// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.ReactiveUI.Core;
using Dock.Model.Core;
using Dock.Model.Adapters;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// Proportional dock splitter.
/// </summary>
[DataContract(IsReference = true)]
public partial class ProportionalDockSplitter : DockableBase, IProportionalDockSplitter
{
    private readonly IProportionalDockSplitterAdapter _adapter;
    /// <summary>
    /// Initializes new instance of the <see cref="ProportionalDockSplitter"/> class.
    /// </summary>
    public ProportionalDockSplitter()
    {
        _adapter = new ProportionalDockSplitterAdapter();
        _canResize = true;
    }
    
    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool CanResize { get; set; }

    /// <inheritdoc/>
    public void ResetProportion() => _adapter.ResetProportion(this);

    /// <inheritdoc/>
    public void SetProportion(double proportion) => _adapter.SetProportion(this, proportion);
}
