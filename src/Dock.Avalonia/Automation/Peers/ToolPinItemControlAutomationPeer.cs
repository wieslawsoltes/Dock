// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Automation.Peers;

internal sealed class ToolPinItemControlAutomationPeer : ControlAutomationPeer, IInvokeProvider, ISelectionItemProvider
{
    private readonly ToolPinItemControl _owner;

    internal ToolPinItemControlAutomationPeer(ToolPinItemControl owner)
        : base(owner)
    {
        _owner = owner;
    }

    protected override string GetClassNameCore()
    {
        return nameof(ToolPinItemControl);
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.TabItem;
    }

    protected override string GetNameCore()
    {
        var baseName = base.GetNameCore();
        if (!string.IsNullOrWhiteSpace(baseName))
        {
            return baseName!;
        }

        var dockable = _owner.DataContext as IDockable;
        return DockAutomationPeerHelper.ResolveName(_owner, "Pinned tool", dockable);
    }

    protected override string GetAutomationIdCore()
    {
        var dockable = _owner.DataContext as IDockable;
        var automationId = DockAutomationPeerHelper.ResolveAutomationId(_owner, dockable);
        return !string.IsNullOrWhiteSpace(automationId) ? automationId : base.GetAutomationIdCore() ?? string.Empty;
    }

    protected override string GetHelpTextCore()
    {
        var dockable = _owner.DataContext as IDockable;

        return DockAutomationPeerHelper.FormatState(
            ("Orientation", _owner.Orientation),
            ("CanPin", dockable?.CanPin ?? false),
            ("CanClose", dockable?.CanClose ?? false),
            ("CanFloat", dockable?.CanFloat ?? false),
            ("DockingState", dockable?.DockingState));
    }

    public void Invoke()
    {
        var dockable = _owner.DataContext as IDockable;
        if (!DockAutomationPeerHelper.TryActivateDockable(dockable))
        {
            _owner.Focus();
        }
    }

    public bool IsSelected
    {
        get
        {
            if (_owner.DataContext is not IDockable { Owner: IDock owner } dockable)
            {
                return false;
            }

            return ReferenceEquals(owner.ActiveDockable, dockable);
        }
    }

    public ISelectionProvider? SelectionContainer
    {
        get
        {
            var pinnedControl = _owner.FindAncestorOfType<ToolPinnedControl>();
            if (pinnedControl is null)
            {
                return null;
            }

            var peer = ControlAutomationPeer.CreatePeerForElement(pinnedControl);
            return peer.GetProvider<ISelectionProvider>();
        }
    }

    public void AddToSelection()
    {
        Select();
    }

    public void RemoveFromSelection()
    {
        if (!IsSelected || _owner.DataContext is not IDockable { Owner: IDock owner } dockable)
        {
            return;
        }

        if (owner.VisibleDockables is not { } visibleDockables)
        {
            return;
        }

        for (var i = 0; i < visibleDockables.Count; i++)
        {
            if (visibleDockables[i] is not IDockable nextDockable || ReferenceEquals(nextDockable, dockable))
            {
                continue;
            }

            DockAutomationPeerHelper.TryActivateDockable(nextDockable);
            return;
        }
    }

    public void Select()
    {
        Invoke();
    }
}
