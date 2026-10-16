// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Dock.Avalonia.Automation.Peers;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Window used to display dock adorners when using floating overlay.
/// </summary>
public class DockAdornerWindow : Window
{
    /// <summary>Defines an inherited factory for floating docking overlay windows.</summary>
    public static readonly AttachedProperty<IDockAdornerWindowFactory?> FactoryProperty =
        AvaloniaProperty.RegisterAttached<DockAdornerWindow, StyledElement, IDockAdornerWindowFactory?>("Factory", inherits: true);

    /// <summary>Gets the optional floating overlay window factory for a visual scope.</summary>
    public static IDockAdornerWindowFactory? GetFactory(StyledElement element) => element.GetValue(FactoryProperty);

    /// <summary>Sets the optional floating overlay window factory for a visual scope.</summary>
    public static void SetFactory(StyledElement element, IDockAdornerWindowFactory? value) => element.SetValue(FactoryProperty, value);

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new DockAdornerWindowAutomationPeer(this);
    }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(DockAdornerWindow);
}
