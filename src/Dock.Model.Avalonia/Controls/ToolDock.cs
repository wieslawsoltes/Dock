/*
 * Dock A docking layout system.
 * Copyright (C) 2023  Wiesław Šoltés
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Tool dock.
/// </summary>
[DataContract(IsReference = true)]
public class ToolDock : DockBase, IToolDock
{
    /// <summary>
    /// Defines the <see cref="Alignment"/> property.
    /// </summary>
    public static readonly DirectProperty<ToolDock, Alignment> AlignmentProperty =
        AvaloniaProperty.RegisterDirect<ToolDock, Alignment>(nameof(Alignment), o => o.Alignment, (o, v) => o.Alignment = v, Alignment.Unset);

    /// <summary>
    /// Defines the <see cref="IsExpanded"/> property.
    /// </summary>
    public static readonly DirectProperty<ToolDock, bool> IsExpandedProperty =
        AvaloniaProperty.RegisterDirect<ToolDock, bool>(nameof(IsExpanded), o => o.IsExpanded, (o, v) => o.IsExpanded = v);

    /// <summary>
    /// Defines the <see cref="AutoHide"/> property.
    /// </summary>
    public static readonly DirectProperty<ToolDock, bool> AutoHideProperty =
        AvaloniaProperty.RegisterDirect<ToolDock, bool>(nameof(AutoHide), o => o.AutoHide, (o, v) => o.AutoHide = v, true);

    /// <summary>
    /// Defines the <see cref="GripMode"/> property.
    /// </summary>
    public static readonly DirectProperty<ToolDock, GripMode> GripModeProperty =
        AvaloniaProperty.RegisterDirect<ToolDock, GripMode>(nameof(GripMode), o => o.GripMode, (o, v) => o.GripMode = v);

    private Alignment _alignment = Alignment.Unset;
    private bool _isExpanded;
    private bool _autoHide = true;
    private GripMode _gripMode = GripMode.Visible;

    /// <summary>
    /// Initializes new instance of the <see cref="ToolDock"/> class.
    /// </summary>
    public ToolDock()
    {
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Alignment")]
    public Alignment Alignment
    {
        get => _alignment;
        set => SetAndRaise(AlignmentProperty, ref _alignment, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("IsExpanded")]
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetAndRaise(IsExpandedProperty, ref _isExpanded, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("AutoHide")]
    public bool AutoHide
    {
        get => _autoHide;
        set => SetAndRaise(AutoHideProperty, ref _autoHide, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("GripMode")]
    public GripMode GripMode
    {
        get => _gripMode;
        set => SetAndRaise(GripModeProperty, ref _gripMode, value);
    }
}
