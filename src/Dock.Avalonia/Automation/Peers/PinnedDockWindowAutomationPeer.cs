// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Automation.Peers;

internal sealed class PinnedDockWindowAutomationPeer : WindowAutomationPeer, IInvokeProvider
{
    private readonly PinnedDockWindow _owner;

    internal PinnedDockWindowAutomationPeer(PinnedDockWindow owner)
        : base(owner)
    {
        _owner = owner;
    }

    protected override string GetClassNameCore()
    {
        return nameof(PinnedDockWindow);
    }

    protected override string GetNameCore()
    {
        var baseName = base.GetNameCore();
        if (!string.IsNullOrWhiteSpace(baseName))
        {
            return baseName!;
        }

        return DockAutomationPeerHelper.ResolveName(_owner, "Pinned dock window");
    }

    protected override string GetHelpTextCore()
    {
        return DockAutomationPeerHelper.FormatState(
            ("WindowState", _owner.WindowState),
            ("Visible", _owner.IsVisible),
            ("ShowInTaskbar", _owner.ShowInTaskbar));
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
