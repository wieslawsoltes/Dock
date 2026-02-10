// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Dock.Avalonia.Automation.Peers;
using Dock.Model.CommandBars;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Hosts command bars merged from active dockables.
/// </summary>
public class DockCommandBarHost : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="MenuBars"/> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<Control>?> MenuBarsProperty =
        AvaloniaProperty.Register<DockCommandBarHost, IReadOnlyList<Control>?>(nameof(MenuBars));

    /// <summary>
    /// Defines the <see cref="ToolBars"/> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<Control>?> ToolBarsProperty =
        AvaloniaProperty.Register<DockCommandBarHost, IReadOnlyList<Control>?>(nameof(ToolBars));

    /// <summary>
    /// Defines the <see cref="RibbonBars"/> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<Control>?> RibbonBarsProperty =
        AvaloniaProperty.Register<DockCommandBarHost, IReadOnlyList<Control>?>(nameof(RibbonBars));

    /// <summary>
    /// Defines the <see cref="BaseCommandBars"/> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<DockCommandBarDefinition>?> BaseCommandBarsProperty =
        AvaloniaProperty.Register<DockCommandBarHost, IReadOnlyList<DockCommandBarDefinition>?>(nameof(BaseCommandBars));

    /// <summary>
    /// Gets or sets the menu bar controls.
    /// </summary>
    public IReadOnlyList<Control>? MenuBars
    {
        get => GetValue(MenuBarsProperty);
        set => SetValue(MenuBarsProperty, value);
    }

    /// <summary>
    /// Gets or sets the tool bar controls.
    /// </summary>
    public IReadOnlyList<Control>? ToolBars
    {
        get => GetValue(ToolBarsProperty);
        set => SetValue(ToolBarsProperty, value);
    }

    /// <summary>
    /// Gets or sets the ribbon bar controls.
    /// </summary>
    public IReadOnlyList<Control>? RibbonBars
    {
        get => GetValue(RibbonBarsProperty);
        set => SetValue(RibbonBarsProperty, value);
    }

    /// <summary>
    /// Gets or sets the base command bar definitions.
    /// </summary>
    public IReadOnlyList<DockCommandBarDefinition>? BaseCommandBars
    {
        get => GetValue(BaseCommandBarsProperty);
        set => SetValue(BaseCommandBarsProperty, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockCommandBarHost"/> class.
    /// </summary>
    public DockCommandBarHost()
    {
        SetCurrentValue(IsVisibleProperty, false);
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new DockCommandBarHostAutomationPeer(this);
    }
}
