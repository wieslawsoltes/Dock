// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Linq;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Automation.Peers;

internal sealed class MdiDocumentControlAutomationPeer : ControlAutomationPeer, IInvokeProvider
{
    private readonly MdiDocumentControl _owner;

    internal MdiDocumentControlAutomationPeer(MdiDocumentControl owner)
        : base(owner)
    {
        _owner = owner;
    }

    protected override string GetClassNameCore()
    {
        return nameof(MdiDocumentControl);
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Pane;
    }

    protected override string GetNameCore()
    {
        var baseName = base.GetNameCore();
        if (!string.IsNullOrWhiteSpace(baseName))
        {
            return baseName!;
        }

        return DockAutomationPeerHelper.ResolveName(_owner, "MDI document host", GetActiveDockable());
    }

    protected override string GetAutomationIdCore()
    {
        var dock = GetDock();
        var automationId = DockAutomationPeerHelper.ResolveAutomationId(_owner, dock);
        return !string.IsNullOrWhiteSpace(automationId) ? automationId : base.GetAutomationIdCore() ?? string.Empty;
    }

    protected override string GetHelpTextCore()
    {
        var dock = GetDock();

        return DockAutomationPeerHelper.FormatState(
            ("Active", _owner.IsActive),
            ("HasVisibleDocuments", _owner.HasVisibleDocuments),
            ("VisibleDockables", dock?.VisibleDockables?.Count ?? 0),
            ("LayoutManager", _owner.LayoutManager?.GetType().Name ?? "none"),
            ("ActiveDockable", dock?.ActiveDockable?.Title ?? "none"));
    }

    public void Invoke()
    {
        var dockable = ResolveDockableToActivate();
        if (dockable is null || !DockAutomationPeerHelper.TryActivateDockable(dockable))
        {
            _owner.Focus();
        }
    }

    private IDock? GetDock()
    {
        return _owner.DataContext as IDock;
    }

    private IDockable? GetActiveDockable()
    {
        return GetDock()?.ActiveDockable;
    }

    private IDockable? ResolveDockableToActivate()
    {
        var dock = GetDock();
        if (dock is null)
        {
            return null;
        }

        if (dock.ActiveDockable is { } active)
        {
            return active;
        }

        return dock.VisibleDockables?.OfType<IDockable>().FirstOrDefault();
    }
}
