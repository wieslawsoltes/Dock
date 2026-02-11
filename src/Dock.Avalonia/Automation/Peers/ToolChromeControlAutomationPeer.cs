// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Core;

namespace Dock.Avalonia.Automation.Peers;

internal sealed class ToolChromeControlAutomationPeer : ContentControlAutomationPeer, IInvokeProvider, IExpandCollapseProvider
{
    private readonly ToolChromeControl _owner;

    internal ToolChromeControlAutomationPeer(ToolChromeControl owner)
        : base(owner)
    {
        _owner = owner;
    }

    protected override string GetClassNameCore()
    {
        return nameof(ToolChromeControl);
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.TitleBar;
    }

    protected override string GetNameCore()
    {
        var baseName = base.GetNameCore();
        if (!string.IsNullOrWhiteSpace(baseName))
        {
            return baseName!;
        }

        var dockable = GetActiveDockable();
        if (!string.IsNullOrWhiteSpace(_owner.Title))
        {
            return _owner.Title;
        }

        return DockAutomationPeerHelper.ResolveName(_owner, "Tool window chrome", dockable);
    }

    protected override string GetAutomationIdCore()
    {
        var automationId = DockAutomationPeerHelper.ResolveAutomationId(_owner, GetActiveDockable());
        return !string.IsNullOrWhiteSpace(automationId) ? automationId : base.GetAutomationIdCore() ?? string.Empty;
    }

    protected override string GetHelpTextCore()
    {
        var dockable = GetActiveDockable();

        return DockAutomationPeerHelper.FormatState(
            ("Active", _owner.IsActive),
            ("Pinned", _owner.IsPinned),
            ("Floating", _owner.IsFloating),
            ("Maximized", _owner.IsMaximized),
            ("HasMenu", _owner.ToolFlyout is not null),
            ("CanClose", IsCapabilityEnabled(dockable, DockCapability.Close)));
    }

    public void Invoke()
    {
        if (!DockAutomationPeerHelper.TryActivateDockable(GetActiveDockable()))
        {
            _owner.Focus();
        }
    }

    public ExpandCollapseState ExpandCollapseState =>
        _owner.ToolFlyout?.IsOpen == true ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;

    public bool ShowsMenu => true;

    public void Expand()
    {
        if (_owner.ToolFlyout is { IsOpen: false } flyout)
        {
            flyout.ShowAt(_owner);
        }
    }

    public void Collapse()
    {
        if (_owner.ToolFlyout is { IsOpen: true } flyout)
        {
            flyout.Hide();
        }
    }

    private IDockable? GetActiveDockable()
    {
        return _owner.DataContext is IDock dock ? dock.ActiveDockable : null;
    }

    private static bool IsCapabilityEnabled(IDockable? dockable, DockCapability capability)
    {
        if (dockable is null)
        {
            return false;
        }

        return DockCapabilityResolver.IsEnabled(
            dockable,
            capability,
            DockCapabilityResolver.ResolveOperationDock(dockable));
    }
}
