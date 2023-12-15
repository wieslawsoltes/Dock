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
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="HostWindowTitleBar"/> xaml.
/// </summary>
public class HostWindowTitleBar : TitleBar
{
    internal Control? BackgroundControl { get; private set; }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(HostWindowTitleBar);

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        BackgroundControl = e.NameScope.Find<Control>("PART_Background");
    }
}
