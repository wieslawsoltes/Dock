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
    private Thickness _targetMargin;

    /// <summary>Defines the inherited provider that selects an enclosing local preview host.</summary>
    public static readonly AttachedProperty<IDockAdornerHostProvider?> AdornerHostProviderProperty =
        AvaloniaProperty.RegisterAttached<DockTarget, Control, IDockAdornerHostProvider?>("AdornerHostProvider", inherits: true);

    /// <summary>Defines the read-only inset of the hovered surface within an expanded adorner.</summary>
    public static readonly DirectProperty<DockTarget, Thickness> TargetMarginProperty =
        AvaloniaProperty.RegisterDirect<DockTarget, Thickness>(nameof(TargetMargin), target => target.TargetMargin);

    /// <summary>Gets the inset, in adorner coordinates, that positions selectors and local indicators over the hovered surface.</summary>
    /// <remarks>The value is zero for an ordinary adorner. Expanded themes apply this inset to
    /// their local selector/indicator panels and leave region-wide previews uninset.</remarks>
    public Thickness TargetMargin => _targetMargin;

    /// <summary>Gets the local preview host provider inherited by a surface.</summary>
    /// <param name="surface">The docking surface or one of its ancestors.</param>
    /// <returns>The provider, or null for the usual surface-sized adorner.</returns>
    public static IDockAdornerHostProvider? GetAdornerHostProvider(AvaloniaObject surface) => surface.GetValue(AdornerHostProviderProperty);

    /// <summary>Sets the local preview host provider inherited by a surface.</summary>
    /// <param name="surface">The docking surface or one of its ancestors.</param>
    /// <param name="provider">The provider, or null for the usual surface-sized adorner.</param>
    public static void SetAdornerHostProvider(AvaloniaObject surface, IDockAdornerHostProvider? provider) => surface.SetValue(AdornerHostProviderProperty, provider);

    internal void SetTargetMargin(Thickness margin) => SetAndRaise(TargetMarginProperty, ref _targetMargin, margin);

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
