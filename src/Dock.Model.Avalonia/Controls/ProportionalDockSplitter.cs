// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Globalization;
using System.Runtime.Serialization;
using Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Adapters;
using Dock.Model.Core;
using System.Text.Json.Serialization;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Proportional dock splitter.
/// </summary>
[DataContract(IsReference = true)]
public class ProportionalDockSplitter : DockBase, IProportionalDockSplitter
{
    /// <summary>
    /// Defines the <see cref="CanResize"/> property.
    /// </summary>
    public static readonly DirectProperty<ProportionalDockSplitter, bool> CanResizeProperty =
        AvaloniaProperty.RegisterDirect<ProportionalDockSplitter, bool>(nameof(CanResize), o => o.CanResize, (o, v) => o.CanResize = v, true);

    private readonly IProportionalDockSplitterAdapter _adapter;
    private bool _canResize = true;
    /// <summary>
    /// Initializes new instance of the <see cref="ProportionalDockSplitter"/> class.
    /// </summary>
    public ProportionalDockSplitter()
    {
        _adapter = new ProportionalDockSplitterAdapter();
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("CanResize")]
    public bool CanResize
    {
        get => _canResize;
        set => SetAndRaise(CanResizeProperty, ref _canResize, value);
    }

    /// <inheritdoc/>
    public void ResetProportion() => _adapter.ResetProportion(this);

    /// <inheritdoc/>
    public void SetProportion(object proportion) => _adapter.SetProportion(this, double.Parse((string)proportion, CultureInfo.InvariantCulture));
}
