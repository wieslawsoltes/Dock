// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Dock.Avalonia.Selectors;
using Dock.Avalonia.Controls;
using System;
using System.Collections.Generic;

namespace Dock.Avalonia.Automation.Peers;

internal sealed class DockSelectorOverlayAutomationPeer : ControlAutomationPeer, IExpandCollapseProvider, ISelectionProvider, IScrollProvider, IValueProvider
{
    private readonly DockSelectorOverlay _owner;

    internal DockSelectorOverlayAutomationPeer(DockSelectorOverlay owner)
        : base(owner)
    {
        _owner = owner;
    }

    protected override string GetClassNameCore()
    {
        return nameof(DockSelectorOverlay);
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.List;
    }

    protected override string GetNameCore()
    {
        var baseName = base.GetNameCore();
        if (!string.IsNullOrWhiteSpace(baseName))
        {
            return baseName!;
        }

        return DockAutomationPeerHelper.ResolveName(_owner, $"{_owner.Mode} selector");
    }

    protected override string GetHelpTextCore()
    {
        var selected = _owner.SelectedItem?.Title;

        return DockAutomationPeerHelper.FormatState(
            ("Open", _owner.IsOpen),
            ("Mode", _owner.Mode),
            ("ItemCount", _owner.Items?.Count ?? 0),
            ("Selected", string.IsNullOrWhiteSpace(selected) ? "none" : selected));
    }

    public ExpandCollapseState ExpandCollapseState =>
        _owner.IsOpen ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;

    public bool ShowsMenu => false;

    public void Expand()
    {
        _owner.IsOpen = true;
    }

    public void Collapse()
    {
        _owner.IsOpen = false;
    }

    public bool CanSelectMultiple => false;

    public bool IsSelectionRequired => false;

    public IReadOnlyList<AutomationPeer> GetSelection()
    {
        var selectionProvider = GetSelectionProvider();
        if (selectionProvider is not null)
        {
            var selection = selectionProvider.GetSelection();
            if (selection.Count > 0)
            {
                return selection;
            }
        }

        return BuildSelection(_owner.SelectedItem);
    }

    public bool HorizontallyScrollable => GetScrollProvider()?.HorizontallyScrollable ?? false;

    public double HorizontalScrollPercent => GetScrollProvider()?.HorizontalScrollPercent ?? ScrollPatternIdentifiers.NoScroll;

    public double HorizontalViewSize => GetScrollProvider()?.HorizontalViewSize ?? 100.0;

    public bool VerticallyScrollable => GetScrollProvider()?.VerticallyScrollable ?? false;

    public double VerticalScrollPercent => GetScrollProvider()?.VerticalScrollPercent ?? ScrollPatternIdentifiers.NoScroll;

    public double VerticalViewSize => GetScrollProvider()?.VerticalViewSize ?? 100.0;

    public void Scroll(ScrollAmount horizontalAmount, ScrollAmount verticalAmount)
    {
        GetScrollProvider()?.Scroll(horizontalAmount, verticalAmount);
    }

    public void SetScrollPercent(double horizontalPercent, double verticalPercent)
    {
        GetScrollProvider()?.SetScrollPercent(horizontalPercent, verticalPercent);
    }

    public bool IsReadOnly => true;

    public string Value => _owner.SelectedItem?.Title ?? string.Empty;

    public void SetValue(string? value)
    {
        throw new InvalidOperationException("Dock selector value is read-only.");
    }

    internal void NotifyIsOpenChanged(bool oldValue, bool newValue)
    {
        var oldState = oldValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;
        var newState = newValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;

        if (oldState != newState)
        {
            RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, oldState, newState);
        }
    }

    internal void NotifySelectedItemChanged(DockSelectorItem? oldItem, DockSelectorItem? newItem)
    {
        var oldSelection = BuildSelection(oldItem);
        var newSelection = BuildSelection(newItem);
        RaisePropertyChangedEvent(SelectionPatternIdentifiers.SelectionProperty, oldSelection, newSelection);

        var oldValue = oldItem?.Title ?? string.Empty;
        var newValue = newItem?.Title ?? string.Empty;
        if (!string.Equals(oldValue, newValue, StringComparison.Ordinal))
        {
            RaisePropertyChangedEvent(ValuePatternIdentifiers.ValueProperty, oldValue, newValue);
        }
    }

    internal void NotifyItemsChanged()
    {
        RaiseChildrenChangedEvent();
    }

    internal void NotifyModeChanged(DockSelectorMode oldMode, DockSelectorMode newMode)
    {
        var oldName = DockAutomationPeerHelper.ResolveName(_owner, $"{oldMode} selector");
        var newName = DockAutomationPeerHelper.ResolveName(_owner, $"{newMode} selector");

        if (!string.Equals(oldName, newName, StringComparison.Ordinal))
        {
            RaisePropertyChangedEvent(AutomationElementIdentifiers.NameProperty, oldName, newName);
        }
    }

    private IReadOnlyList<AutomationPeer> BuildSelection(DockSelectorItem? item)
    {
        return DockAutomationPeerHelper.ToSelectionList(ResolveSelectionPeer(item));
    }

    private IScrollProvider? GetScrollProvider()
    {
        if (_owner.ItemsList is not { } itemsList)
        {
            return null;
        }

        var peer = ControlAutomationPeer.CreatePeerForElement(itemsList);
        return peer.GetProvider<IScrollProvider>();
    }

    private ISelectionProvider? GetSelectionProvider()
    {
        if (_owner.ItemsList is not { } itemsList)
        {
            return null;
        }

        var peer = ControlAutomationPeer.CreatePeerForElement(itemsList);
        return peer.GetProvider<ISelectionProvider>();
    }

    private AutomationPeer? ResolveSelectionPeer(DockSelectorItem? item)
    {
        if (_owner.ItemsList is not { } itemsList || item is null)
        {
            return null;
        }

        var peer = DockAutomationPeerHelper.TryGetContainerPeer(itemsList, item);
        if (peer is not null)
        {
            return peer;
        }

        itemsList.ScrollIntoView(item);
        itemsList.UpdateLayout();
        return DockAutomationPeerHelper.TryGetContainerPeer(itemsList, item);
    }
}
