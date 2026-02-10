// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Automation.Peers;

internal sealed class DockCommandBarHostAutomationPeer : ControlAutomationPeer
{
    private readonly DockCommandBarHost _owner;

    internal DockCommandBarHostAutomationPeer(DockCommandBarHost owner)
        : base(owner)
    {
        _owner = owner;
    }

    protected override string GetClassNameCore()
    {
        return nameof(DockCommandBarHost);
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.ToolBar;
    }

    protected override string GetNameCore()
    {
        var baseName = base.GetNameCore();
        if (!string.IsNullOrWhiteSpace(baseName))
        {
            return baseName!;
        }

        return DockAutomationPeerHelper.ResolveName(_owner, "Dock command bars");
    }

    protected override string GetHelpTextCore()
    {
        return DockAutomationPeerHelper.FormatState(
            ("Visible", _owner.IsVisible),
            ("MenuBars", _owner.MenuBars?.Count ?? 0),
            ("ToolBars", _owner.ToolBars?.Count ?? 0),
            ("RibbonBars", _owner.RibbonBars?.Count ?? 0),
            ("BaseDefinitions", _owner.BaseCommandBars?.Count ?? 0));
    }
}
