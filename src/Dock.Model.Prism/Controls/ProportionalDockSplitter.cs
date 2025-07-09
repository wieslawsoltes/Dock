// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Adapters;
using Dock.Model.Controls;
using Dock.Model.Prism.Core;

namespace Dock.Model.Prism.Controls;

/// <summary>
/// Proportional dock splitter.
/// </summary>
[DataContract(IsReference = true)]
public class ProportionalDockSplitter : DockableBase, IProportionalDockSplitter
{
    private readonly IProportionalDockSplitterAdapter _adapter;
    private bool _canResize = true;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanResize
    {
        get => _canResize;
        set => SetProperty(ref _canResize, value);
    }

    /// <summary>
    /// Initializes new instance of the <see cref="ProportionalDockSplitter"/> class.
    /// </summary>
    public ProportionalDockSplitter()
    {
        _adapter = new ProportionalDockSplitterAdapter();
    }

    /// <inheritdoc/>
    public void ResetProportion() => _adapter.ResetProportion(this);

    /// <inheritdoc/>
    public void SetProportion(double proportion) => _adapter.SetProportion(this, proportion);
}
