// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Automation.Peers;

internal sealed class HostWindowTitleBarAutomationPeer : ControlAutomationPeer, IInvokeProvider
{
    private readonly HostWindowTitleBar _owner;

    internal HostWindowTitleBarAutomationPeer(HostWindowTitleBar owner)
        : base(owner)
    {
        _owner = owner;
    }

    protected override string GetClassNameCore()
    {
        return nameof(HostWindowTitleBar);
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

        return DockAutomationPeerHelper.ResolveName(_owner, "Host window title bar");
    }

    protected override string GetHelpTextCore()
    {
        return DockAutomationPeerHelper.FormatState(
            ("HasBackground", _owner.BackgroundControl is not null),
            ("Focusable", _owner.Focusable),
            ("Visible", _owner.IsVisible));
    }

    public void Invoke()
    {
        if (_owner.GetVisualRoot() is HostWindow hostWindow)
        {
            hostWindow.Activate();
            hostWindow.Focus();
        }
        else
        {
            _owner.Focus();
        }
    }
}
