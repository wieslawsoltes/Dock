// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls.Primitives;
using Dock.Model.Core;

namespace Dock.Avalonia.Automation.Peers;

internal abstract class DockTabStripAutomationPeer<TTabStrip> : SelectingItemsControlAutomationPeer
    where TTabStrip : TabStrip
{
    protected DockTabStripAutomationPeer(TTabStrip owner)
        : base(owner)
    {
        OwnerControl = owner;
    }

    protected TTabStrip OwnerControl { get; }

    protected abstract string ClassName { get; }

    protected abstract string FallbackName { get; }

    protected abstract string BuildStateText(IDock? dock);

    protected override string GetClassNameCore()
    {
        return ClassName;
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

        return DockAutomationPeerHelper.ResolveName(OwnerControl, FallbackName, OwnerControl.DataContext as IDockable);
    }

    protected override string GetAutomationIdCore()
    {
        var automationId = DockAutomationPeerHelper.ResolveAutomationId(OwnerControl, OwnerControl.DataContext as IDockable);
        return !string.IsNullOrWhiteSpace(automationId) ? automationId : base.GetAutomationIdCore() ?? string.Empty;
    }

    protected override string GetHelpTextCore()
    {
        return BuildStateText(OwnerControl.DataContext as IDock);
    }
}
