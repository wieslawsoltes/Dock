// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Automation.Peers;

internal sealed class DockTargetAutomationPeer : ControlAutomationPeer, IInvokeProvider
{
    private readonly DockTargetBase _owner;

    internal DockTargetAutomationPeer(DockTargetBase owner)
        : base(owner)
    {
        _owner = owner;
    }

    protected override string GetClassNameCore()
    {
        return _owner is GlobalDockTarget ? nameof(GlobalDockTarget) : nameof(DockTarget);
    }

    protected override string GetNameCore()
    {
        var baseName = base.GetNameCore();
        if (!string.IsNullOrWhiteSpace(baseName))
        {
            return baseName!;
        }

        return DockAutomationPeerHelper.ResolveName(
            _owner,
            _owner is GlobalDockTarget ? "Global dock target" : "Dock target");
    }

    protected override string GetAutomationIdCore()
    {
        var automationId = DockAutomationPeerHelper.ResolveAutomationId(_owner);
        return !string.IsNullOrWhiteSpace(automationId) ? automationId : base.GetAutomationIdCore() ?? string.Empty;
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Pane;
    }

    protected override string GetHelpTextCore()
    {
        return DockAutomationPeerHelper.FormatState(
            ("Horizontal", _owner.ShowHorizontalTargets),
            ("Vertical", _owner.ShowVerticalTargets),
            ("GlobalAvailable", _owner.IsGlobalDockAvailable),
            ("GlobalActive", _owner.IsGlobalDockActive));
    }

    protected override bool IsControlElementCore()
    {
        return _owner.IsVisible;
    }

    protected override bool IsContentElementCore()
    {
        return _owner.IsVisible;
    }

    public void Invoke()
    {
        _owner.Focus();
    }
}
