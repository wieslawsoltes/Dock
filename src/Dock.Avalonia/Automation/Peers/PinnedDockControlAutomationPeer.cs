// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;

namespace Dock.Avalonia.Automation.Peers;

internal sealed class PinnedDockControlAutomationPeer : ControlAutomationPeer, IExpandCollapseProvider
{
    private readonly PinnedDockControl _owner;

    internal PinnedDockControlAutomationPeer(PinnedDockControl owner)
        : base(owner)
    {
        _owner = owner;
    }

    protected override string GetClassNameCore()
    {
        return nameof(PinnedDockControl);
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

        var pinnedDock = GetPinnedDock();
        var explicitName = AutomationProperties.GetName(_owner);
        if (!string.IsNullOrWhiteSpace(explicitName))
        {
            return explicitName!;
        }

        var activeTitle = pinnedDock?.ActiveDockable?.Title;
        return !string.IsNullOrWhiteSpace(activeTitle) ? activeTitle! : "Pinned dock host";
    }

    protected override string GetAutomationIdCore()
    {
        var explicitId = AutomationProperties.GetAutomationId(_owner);
        if (!string.IsNullOrWhiteSpace(explicitId))
        {
            return explicitId!;
        }

        var pinnedDock = GetPinnedDock();
        if (pinnedDock is { Id: { } pinnedId } && !string.IsNullOrWhiteSpace(pinnedId))
        {
            return pinnedId;
        }

        return base.GetAutomationIdCore() ?? string.Empty;
    }

    protected override string GetHelpTextCore()
    {
        var pinnedDock = GetPinnedDock();

        return DockAutomationPeerHelper.FormatState(
            ("Alignment", _owner.PinnedDockAlignment),
            ("DisplayMode", _owner.PinnedDockDisplayMode),
            ("HasPinnedDock", pinnedDock is not null),
            ("Expanded", pinnedDock?.IsExpanded ?? false),
            ("VisibleDockables", pinnedDock?.VisibleDockables?.Count ?? 0),
            ("ActiveDockable", pinnedDock?.ActiveDockable?.Title ?? "none"));
    }

    public ExpandCollapseState ExpandCollapseState
    {
        get
        {
            var pinnedDock = GetPinnedDock();
            if (pinnedDock is null || pinnedDock.VisibleDockables is null || pinnedDock.VisibleDockables.Count == 0)
            {
                return ExpandCollapseState.Collapsed;
            }

            return pinnedDock.IsExpanded ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;
        }
    }

    public bool ShowsMenu => false;

    public void Expand()
    {
        var pinnedDock = GetPinnedDock();
        if (pinnedDock is not null)
        {
            pinnedDock.IsExpanded = true;
        }
    }

    public void Collapse()
    {
        var pinnedDock = GetPinnedDock();
        if (pinnedDock is not null)
        {
            pinnedDock.IsExpanded = false;
        }
    }

    private IToolDock? GetPinnedDock()
    {
        return (_owner.DataContext as IRootDock)?.PinnedDock;
    }
}
