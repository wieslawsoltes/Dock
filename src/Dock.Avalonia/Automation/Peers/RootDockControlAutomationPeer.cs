// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;

namespace Dock.Avalonia.Automation.Peers;

internal sealed class RootDockControlAutomationPeer : ControlAutomationPeer
{
    private readonly RootDockControl _owner;

    internal RootDockControlAutomationPeer(RootDockControl owner)
        : base(owner)
    {
        _owner = owner;
    }

    protected override string GetClassNameCore()
    {
        return nameof(RootDockControl);
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Pane;
    }

    protected override string GetNameCore()
    {
        var baseName = base.GetNameCore();
        if (!string.IsNullOrWhiteSpace(baseName))
        {
            return baseName!;
        }

        var rootDock = _owner.DataContext as IRootDock;
        return DockAutomationPeerHelper.ResolveName(_owner, "Root dock", rootDock);
    }

    protected override string GetAutomationIdCore()
    {
        var rootDock = _owner.DataContext as IRootDock;
        var automationId = DockAutomationPeerHelper.ResolveAutomationId(_owner, rootDock);
        return !string.IsNullOrWhiteSpace(automationId) ? automationId : base.GetAutomationIdCore() ?? string.Empty;
    }

    protected override string GetHelpTextCore()
    {
        if (_owner.DataContext is not IRootDock rootDock)
        {
            return DockAutomationPeerHelper.FormatState(("HasRoot", false));
        }

        var pinnedDock = rootDock.PinnedDock;

        return DockAutomationPeerHelper.FormatState(
            ("HasRoot", true),
            ("FocusableRoot", rootDock.IsFocusableRoot),
            ("VisibleDockables", rootDock.VisibleDockables?.Count ?? 0),
            ("HiddenDockables", rootDock.HiddenDockables?.Count ?? 0),
            ("PinnedLeft", rootDock.LeftPinnedDockables?.Count ?? 0),
            ("PinnedRight", rootDock.RightPinnedDockables?.Count ?? 0),
            ("PinnedTop", rootDock.TopPinnedDockables?.Count ?? 0),
            ("PinnedBottom", rootDock.BottomPinnedDockables?.Count ?? 0),
            ("PinnedVisible", pinnedDock?.VisibleDockables?.Count ?? 0),
            ("ActiveDockable", rootDock.ActiveDockable?.Title ?? "none"));
    }
}
