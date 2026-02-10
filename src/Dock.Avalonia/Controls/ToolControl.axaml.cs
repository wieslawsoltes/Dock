// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Avalonia.Automation.Peers;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="ToolControl"/> xaml.
/// </summary>
public class ToolControl : TemplatedControl
{
    /// <summary>
    /// Define the <see cref="IconTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> IconTemplateProperty = 
        AvaloniaProperty.Register<ToolControl, object?>(nameof(IconTemplate));

    /// <summary>
    /// Define the <see cref="HeaderTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> HeaderTemplateProperty =
        AvaloniaProperty.Register<ToolControl, IDataTemplate>(nameof(HeaderTemplate));

    /// <summary>
    /// Define the <see cref="ModifiedTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> ModifiedTemplateProperty = 
        AvaloniaProperty.Register<ToolControl, IDataTemplate>(nameof(ModifiedTemplate));

    /// <summary>
    /// Gets or sets tab icon template.
    /// </summary>
    public object? IconTemplate
    {
        get => GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets tab header template.
    /// </summary>
    public IDataTemplate HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets tab modified template.
    /// </summary>
    public IDataTemplate ModifiedTemplate
    {
        get => GetValue(ModifiedTemplateProperty);
        set => SetValue(ModifiedTemplateProperty, value);
    }

    /// <inheritdoc/>
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new ToolControlAutomationPeer(this);
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        AddHandler(PointerPressedEvent, PressedHandler, RoutingStrategies.Tunnel);
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        RemoveHandler(PointerPressedEvent, PressedHandler);
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
}
