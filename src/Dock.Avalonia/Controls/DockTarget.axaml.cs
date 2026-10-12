// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="DockTarget"/> xaml.
/// </summary>
public class DockTarget : DockTargetBase
{
    /// <summary>Defines the inherited theme used by local docking adorners for a surface.</summary>
    public static readonly AttachedProperty<ControlTheme?> TargetThemeProperty =
        AvaloniaProperty.RegisterAttached<DockTarget, Control, ControlTheme?>("TargetTheme", inherits: true);

    /// <summary>Gets the local docking-adorner theme inherited by a surface.</summary>
    /// <param name="surface">The adorned surface or one of its ancestors.</param>
    /// <returns>The supplied theme, or null to use the normal DockTarget theme.</returns>
    public static ControlTheme? GetTargetTheme(AvaloniaObject surface) => surface.GetValue(TargetThemeProperty);

    /// <summary>Sets the local docking-adorner theme inherited by a surface.</summary>
    /// <param name="surface">The adorned surface or one of its ancestors.</param>
    /// <param name="theme">The theme, or null to use the normal DockTarget theme.</param>
    public static void SetTargetTheme(AvaloniaObject surface, ControlTheme? theme) => surface.SetValue(TargetThemeProperty, theme);

    /// <inheritdoc />
    protected override DockOperation DefaultDockOperation => DockOperation.Window;
}
