// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using System.Collections.Generic;

namespace Dock.Avalonia.Automation.Peers;

internal sealed class ToolPinnedControlAutomationPeer : ControlAutomationPeer, ISelectionProvider
{
    private readonly ToolPinnedControl _owner;

    internal ToolPinnedControlAutomationPeer(ToolPinnedControl owner)
        : base(owner)
    {
        _owner = owner;
    }

    protected override string GetClassNameCore()
    {
        return nameof(ToolPinnedControl);
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Tab;
    }

    protected override string GetNameCore()
    {
        var baseName = base.GetNameCore();
        if (!string.IsNullOrWhiteSpace(baseName))
        {
            return baseName!;
        }

        return DockAutomationPeerHelper.ResolveName(_owner, "Pinned tool tabs");
    }

    protected override string GetHelpTextCore()
    {
        return DockAutomationPeerHelper.FormatState(
            ("Orientation", _owner.Orientation),
            ("Items", _owner.Items.Count),
            ("HasSelection", GetSelectedDockable() is not null));
    }

    public bool CanSelectMultiple => false;

    public bool IsSelectionRequired => false;

    public IReadOnlyList<AutomationPeer> GetSelection()
    {
        var selectedDockable = GetSelectedDockable();
        var selectedPeer = selectedDockable is null
            ? null
            : DockAutomationPeerHelper.TryGetContainerPeer(_owner, selectedDockable);

        return DockAutomationPeerHelper.ToSelectionList(selectedPeer);
    }

    private IDockable? GetSelectedDockable()
    {
        for (var i = 0; i < _owner.Items.Count; i++)
        {
            if (_owner.Items[i] is not IDockable dockable || dockable.Owner is not IDock owner)
            {
                continue;
            }

            if (owner.ActiveDockable is not IDockable activeDockable)
            {
                continue;
            }

            for (var index = 0; index < _owner.Items.Count; index++)
            {
                if (_owner.Items[index] is IDockable candidate && ReferenceEquals(candidate, activeDockable))
                {
                    return activeDockable;
                }
            }
        }

        return null;
    }
}
