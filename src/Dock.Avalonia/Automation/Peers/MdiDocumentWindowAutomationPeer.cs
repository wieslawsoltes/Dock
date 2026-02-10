// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Automation.Peers;

internal sealed class MdiDocumentWindowAutomationPeer : ControlAutomationPeer, IInvokeProvider
{
    private readonly MdiDocumentWindow _owner;

    internal MdiDocumentWindowAutomationPeer(MdiDocumentWindow owner)
        : base(owner)
    {
        _owner = owner;
    }

    protected override string GetClassNameCore()
    {
        return nameof(MdiDocumentWindow);
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Window;
    }

    protected override string GetNameCore()
    {
        var baseName = base.GetNameCore();
        if (!string.IsNullOrWhiteSpace(baseName))
        {
            return baseName!;
        }

        var dockable = _owner.DataContext as IDockable;
        return DockAutomationPeerHelper.ResolveName(_owner, "MDI document window", dockable);
    }

    protected override string GetAutomationIdCore()
    {
        var automationId = DockAutomationPeerHelper.ResolveAutomationId(_owner, _owner.DataContext as IDockable);
        return !string.IsNullOrWhiteSpace(automationId) ? automationId : base.GetAutomationIdCore() ?? string.Empty;
    }

    protected override string GetHelpTextCore()
    {
        var dockable = _owner.DataContext as IDockable;
        var mdiState = _owner.DataContext is IMdiDocument mdi ? mdi.MdiState : _owner.MdiState;

        return DockAutomationPeerHelper.FormatState(
            ("Active", _owner.IsActive),
            ("MdiState", mdiState),
            ("CanClose", dockable?.CanClose ?? false),
            ("CanDrag", dockable?.CanDrag ?? false));
    }

    public void Invoke()
    {
        if (!DockAutomationPeerHelper.TryActivateDockable(_owner.DataContext as IDockable))
        {
            _owner.Focus();
        }
    }
}
