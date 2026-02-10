// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Automation.Peers;

internal sealed class HostWindowAutomationPeer : WindowAutomationPeer, IInvokeProvider
{
    private readonly HostWindow _owner;

    internal HostWindowAutomationPeer(HostWindow owner)
        : base(owner)
    {
        _owner = owner;
    }

    protected override string GetClassNameCore()
    {
        return nameof(HostWindow);
    }

    protected override string GetNameCore()
    {
        var baseName = base.GetNameCore();
        if (!string.IsNullOrWhiteSpace(baseName))
        {
            return baseName!;
        }

        return DockAutomationPeerHelper.ResolveName(_owner, "Dock host window", _owner.Window?.Layout);
    }

    protected override string GetAutomationIdCore()
    {
        var automationId = DockAutomationPeerHelper.ResolveAutomationId(_owner, _owner.Window?.Layout);
        return !string.IsNullOrWhiteSpace(automationId) ? automationId : base.GetAutomationIdCore() ?? string.Empty;
    }

    protected override string GetHelpTextCore()
    {
        return DockAutomationPeerHelper.FormatState(
            ("Tracked", _owner.IsTracked),
            ("ToolWindow", _owner.IsToolWindow),
            ("WindowState", _owner.WindowState),
            ("HasDockWindow", _owner.Window is not null),
            ("DockContent", _owner.Window?.Layout?.Id ?? "none"));
    }

    public void Invoke()
    {
        if (!_owner.IsVisible)
        {
            _owner.Show();
        }

        _owner.Activate();
        _owner.Focus();
    }
}
