// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Dock.Avalonia.Themes;

namespace Dock.Avalonia.Themes.Fluent;

/// <summary>
/// Provides light/dark and preset switching for Dock Fluent theme resources.
/// </summary>
public sealed class DockFluentThemeManager : DockPresetThemeManagerBase
{
    private const string PresetUriPrefix = "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/";

    private static readonly string[] s_presetNames =
    [
        "Default",
        "VS Code Light",
        "VS Code Dark",
        "Rider Light",
        "Rider Dark"
    ];

    private static readonly Uri[] s_presetUris =
    [
        new("avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/Default.axaml"),
        new("avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/VsCodeLight.axaml"),
        new("avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/VsCodeDark.axaml"),
        new("avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/RiderLight.axaml"),
        new("avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/RiderDark.axaml")
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="DockFluentThemeManager"/> class.
    /// </summary>
    public DockFluentThemeManager()
        : base(s_presetNames, s_presetUris, PresetUriPrefix)
    {
    }

    /// <inheritdoc />
    protected override bool TryGetDefaultPresetOwner(Application application, out IResourceDictionary? owner)
    {
        return TryGetDefaultPresetOwner(application.Styles, out owner);
    }

    private static bool TryGetDefaultPresetOwner(IEnumerable<IStyle> styles, out IResourceDictionary? owner)
    {
        foreach (var style in styles)
        {
            switch (style)
            {
                case DockFluentTheme { Resources: { } resources }:
                    owner = resources;
                    return true;
                case Styles nestedStyles when TryGetDefaultPresetOwner(nestedStyles, out owner):
                    return true;
            }
        }

        owner = null;
        return false;
    }
}
