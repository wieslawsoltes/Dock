// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Dock.Avalonia.Controls;
using System;

namespace Dock.Avalonia.Automation.Peers;

internal sealed class DragPreviewControlAutomationPeer : ControlAutomationPeer, IValueProvider
{
    private readonly DragPreviewControl _owner;

    internal DragPreviewControlAutomationPeer(DragPreviewControl owner)
        : base(owner)
    {
        _owner = owner;
    }

    protected override string GetClassNameCore()
    {
        return nameof(DragPreviewControl);
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

        return DockAutomationPeerHelper.ResolveName(_owner, "Drag preview");
    }

    protected override string GetHelpTextCore()
    {
        return DockAutomationPeerHelper.FormatState(
            ("Status", _owner.Status),
            ("ShowContent", _owner.ShowContent),
            ("PreviewWidth", _owner.PreviewContentWidth),
            ("PreviewHeight", _owner.PreviewContentHeight));
    }

    public bool IsReadOnly => true;

    public string Value => _owner.Status ?? string.Empty;

    public void SetValue(string? value)
    {
        throw new InvalidOperationException("Drag preview status is read-only.");
    }

    internal void NotifyStatusChanged(string? oldValue, string? newValue)
    {
        var oldStatus = oldValue ?? string.Empty;
        var newStatus = newValue ?? string.Empty;

        if (!string.Equals(oldStatus, newStatus, StringComparison.Ordinal))
        {
            RaisePropertyChangedEvent(ValuePatternIdentifiers.ValueProperty, oldStatus, newStatus);
        }
    }
}
