// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Runtime.Serialization;
using Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
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

    /// <summary>
    /// Defines the <see cref="ResizePreview"/> property.
    /// </summary>
    public static readonly DirectProperty<ProportionalDockSplitter, bool> ResizePreviewProperty =
        AvaloniaProperty.RegisterDirect<ProportionalDockSplitter, bool>(nameof(ResizePreview), o => o.ResizePreview, (o, v) => o.ResizePreview = v);

    private bool _canResize = true;
    private bool _resizePreview;
    /// <summary>
    /// Initializes new instance of the <see cref="ProportionalDockSplitter"/> class.
    /// </summary>
    public ProportionalDockSplitter()
    {
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
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("ResizePreview")]
    public bool ResizePreview
    {
        get => _resizePreview;
        set => SetAndRaise(ResizePreviewProperty, ref _resizePreview, value);
    }
}
