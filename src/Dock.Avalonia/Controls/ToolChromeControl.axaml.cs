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
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Dock tool chrome content control.
/// </summary>
[PseudoClasses(":floating", ":active")]
public class ToolChromeControl : ContentControl
{
    /// <summary>
    /// Define <see cref="Title"/> property.
    /// </summary>
    public static readonly StyledProperty<string> TitleProprty =
        AvaloniaProperty.Register<ToolChromeControl, string>(nameof(Title));

    /// <summary>
    /// Define the <see cref="IsActive"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<ToolChromeControl, bool>(nameof(IsActive));

    /// <summary>
    /// Initialize the new instance of the <see cref="ToolChromeControl"/>.
    /// </summary>
    public ToolChromeControl()
    {
        UpdatePseudoClasses(IsActive);
    }

    /// <summary>
    /// Gets or sets chrome tool title.
    /// </summary>
    public string Title
    {
        get => GetValue(TitleProprty);
        set => SetValue(TitleProprty, value);
    }

    /// <summary>
    /// Gets or sets if this is the currently active Tool.
    /// </summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    internal Control? Grip { get; private set; }

    internal Button? CloseButton { get; private set; }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AddHandler(PointerPressedEvent, PressedHandler, RoutingStrategies.Tunnel);
    }
        
    private void PressedHandler(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is IDock {Factory: { } factory} dock && dock.ActiveDockable is { })
        {
            if (factory.FindRoot(dock.ActiveDockable, _ => true) is { } root)
            {
                factory.SetFocusedDockable(root, dock.ActiveDockable);
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        //On linux we dont attach to the HostWindow because of inconsistent drag behaviour
        if (VisualRoot is HostWindow window 
            && (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)))
        {
            Grip = e.NameScope.Find<Control>("PART_Grip");
            CloseButton = e.NameScope.Find<Button>("PART_CloseButton");

            window.AttachGrip(this);

            PseudoClasses.Set(":floating", true);
        }
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsActiveProperty)
        {
            UpdatePseudoClasses(change.GetNewValue<bool>());
        }
    }

    private void UpdatePseudoClasses(bool isActive)
    {
        PseudoClasses.Set(":active", isActive);
    }
}
