// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;

namespace Dock.Avalonia.Themes;

/// <summary>
/// Defines theme and preset switching operations for Dock samples and applications.
/// </summary>
public interface IDockThemeManager
{
    /// <summary>
    /// Switches the application light/dark variant by index.
    /// </summary>
    /// <param name="index">The variant index (0 = Light, 1 = Dark).</param>
    void Switch(int index);

    /// <summary>
    /// Gets the available preset display names.
    /// </summary>
    IReadOnlyList<string> PresetNames { get; }

    /// <summary>
    /// Gets the currently active preset index.
    /// </summary>
    int CurrentPresetIndex { get; }

    /// <summary>
    /// Switches the currently active preset by index.
    /// </summary>
    /// <param name="index">The preset index.</param>
    void SwitchPreset(int index);
}
