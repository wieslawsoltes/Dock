// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Automation.Peers;

internal sealed class DockControlAutomationPeer : ControlAutomationPeer
{
    private readonly DockControl _owner;

    internal DockControlAutomationPeer(DockControl owner)
        : base(owner)
    {
        _owner = owner;
    }

    protected override string GetClassNameCore()
    {
        return nameof(DockControl);
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

        return DockAutomationPeerHelper.ResolveName(_owner, "Dock host", _owner.Layout);
    }

    protected override string GetAutomationIdCore()
    {
        var automationId = DockAutomationPeerHelper.ResolveAutomationId(_owner, _owner.Layout);
        return !string.IsNullOrWhiteSpace(automationId) ? automationId : base.GetAutomationIdCore() ?? string.Empty;
    }

    protected override string GetHelpTextCore()
    {
        var layout = _owner.Layout;

        return DockAutomationPeerHelper.FormatState(
            ("Layout", layout?.Id ?? "none"),
            ("DockingEnabled", _owner.IsDockingEnabled),
            ("Dragging", _owner.IsDraggingDock),
            ("SelectorOpen", _owner.IsOpen),
            ("ManagedWindowLayer", _owner.EnableManagedWindowLayer),
            ("VisibleDockables", layout?.VisibleDockables?.Count ?? 0),
            ("GlobalDocking", layout?.EnableGlobalDocking ?? false));
    }
}
