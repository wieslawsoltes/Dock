// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Docking panel dock.
/// </summary>
public class DockDock : DockBase, IDockDock
{        
    /// <summary>
    /// Defines the <see cref="LastChildFill"/> property.
    /// </summary>
    public static readonly DirectProperty<DockDock, bool> LastChildFillProperty =
        AvaloniaProperty.RegisterDirect<DockDock, bool>(nameof(LastChildFill), o => o.LastChildFill, (o, v) => o.LastChildFill = v, true);

    private bool _lastChildFill = true;

    /// <summary>
    /// Initializes new instance of the <see cref="DockDock"/> class.
    /// </summary>
    public DockDock()
    {
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("LastChildFill")]
    public bool LastChildFill
    {
        get => _lastChildFill;
        set => SetAndRaise(LastChildFillProperty, ref _lastChildFill, value);
    }
}
