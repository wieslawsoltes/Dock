// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.VisualTree;
using Avalonia.Controls.Primitives;
using Dock.Model.Core;

namespace Dock.Avalonia.Automation.Peers;

internal abstract class DockTabStripItemAutomationPeer<TTabStripItem> : ListItemAutomationPeer, IInvokeProvider, ISelectionItemProvider
    where TTabStripItem : TabStripItem
{
    protected DockTabStripItemAutomationPeer(TTabStripItem owner)
        : base(owner)
    {
        OwnerControl = owner;
    }

    protected TTabStripItem OwnerControl { get; }

    protected abstract string ClassName { get; }

    protected abstract string FallbackName { get; }

    protected abstract string BuildStateText(IDockable? dockable);

    protected override string GetClassNameCore()
    {
        return ClassName;
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

        var dockable = OwnerControl.DataContext as IDockable;
        return DockAutomationPeerHelper.ResolveName(OwnerControl, FallbackName, dockable);
    }

    protected override string GetAutomationIdCore()
    {
        var dockable = OwnerControl.DataContext as IDockable;
        var automationId = DockAutomationPeerHelper.ResolveAutomationId(OwnerControl, dockable);
        return !string.IsNullOrWhiteSpace(automationId) ? automationId : base.GetAutomationIdCore() ?? string.Empty;
    }

    protected override string GetHelpTextCore()
    {
        var dockable = OwnerControl.DataContext as IDockable;
        return BuildStateText(dockable);
    }

    public virtual void Invoke()
    {
        var dockable = OwnerControl.DataContext as IDockable;
        DockAutomationPeerHelper.TryActivateDockable(dockable);
        OwnerControl.IsSelected = true;
        OwnerControl.Focus();
    }

    bool ISelectionItemProvider.IsSelected => OwnerControl.IsSelected;

    ISelectionProvider? ISelectionItemProvider.SelectionContainer
    {
        get
        {
            var tabStrip = OwnerControl.FindAncestorOfType<TabStrip>();
            if (tabStrip is null)
            {
                return null;
            }

            var peer = ControlAutomationPeer.CreatePeerForElement(tabStrip);
            return peer.GetProvider<ISelectionProvider>();
        }
    }

    void ISelectionItemProvider.AddToSelection()
    {
        SelectCore();
    }

    void ISelectionItemProvider.RemoveFromSelection()
    {
        if (!OwnerControl.IsSelected)
        {
            return;
        }

        OwnerControl.IsSelected = false;
    }

    void ISelectionItemProvider.Select()
    {
        SelectCore();
    }

    private void SelectCore()
    {
        Invoke();
    }

    internal void NotifyIsSelectedChanged(bool oldValue, bool newValue)
    {
        if (oldValue == newValue)
        {
            return;
        }

        RaisePropertyChangedEvent(SelectionItemPatternIdentifiers.IsSelectedProperty, oldValue, newValue);
    }
}
