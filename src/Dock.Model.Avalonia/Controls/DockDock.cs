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

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Docking panel dock.
/// </summary>
[DataContract(IsReference = true)]
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
