// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Linq;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Automation.Peers;

internal sealed class ToolControlAutomationPeer : ControlAutomationPeer, IInvokeProvider
{
    private readonly ToolControl _owner;

    internal ToolControlAutomationPeer(ToolControl owner)
        : base(owner)
    {
        _owner = owner;
    }

    protected override string GetClassNameCore()
    {
        return nameof(ToolControl);
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

        return DockAutomationPeerHelper.ResolveName(_owner, "Tool host", GetActiveDockable());
    }

    protected override string GetAutomationIdCore()
    {
        var dock = GetDock();
        var automationId = DockAutomationPeerHelper.ResolveAutomationId(_owner, dock);
        return !string.IsNullOrWhiteSpace(automationId) ? automationId : base.GetAutomationIdCore() ?? string.Empty;
    }

    protected override string GetHelpTextCore()
    {
        var dock = GetDock();
        var toolDock = dock as IToolDock;

        return DockAutomationPeerHelper.FormatState(
            ("VisibleDockables", dock?.VisibleDockables?.Count ?? 0),
            ("Expanded", toolDock?.IsExpanded ?? false),
            ("AutoHide", toolDock?.AutoHide ?? false),
            ("Alignment", toolDock?.Alignment),
            ("GripMode", toolDock?.GripMode),
            ("ActiveDockable", dock?.ActiveDockable?.Title ?? "none"));
    }

    public void Invoke()
    {
        var dockable = ResolveDockableToActivate();
        if (dockable is null || !DockAutomationPeerHelper.TryActivateDockable(dockable))
        {
            _owner.Focus();
        }
    }

    private IDock? GetDock()
    {
        return _owner.DataContext as IDock;
    }

    private IDockable? GetActiveDockable()
    {
        return GetDock()?.ActiveDockable;
    }

    private IDockable? ResolveDockableToActivate()
    {
        var dock = GetDock();
        if (dock is null)
        {
            return null;
        }

        if (dock.ActiveDockable is { } active)
        {
            return active;
        }

        return dock.VisibleDockables?.FirstOrDefault();
    }
}
