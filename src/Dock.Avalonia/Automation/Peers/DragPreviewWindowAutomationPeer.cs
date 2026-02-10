// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Automation.Peers;

internal sealed class DragPreviewWindowAutomationPeer : WindowAutomationPeer
{
    private readonly DragPreviewWindow _owner;

    internal DragPreviewWindowAutomationPeer(DragPreviewWindow owner)
        : base(owner)
    {
        _owner = owner;
    }

    protected override string GetClassNameCore()
    {
        return nameof(DragPreviewWindow);
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

        return DockAutomationPeerHelper.ResolveName(_owner, "Drag preview window");
    }

    protected override string GetHelpTextCore()
    {
        return DockAutomationPeerHelper.FormatState(
            ("Visible", _owner.IsVisible),
            ("WindowState", _owner.WindowState));
    }

    protected override bool IsControlElementCore()
    {
        // Drag preview windows are transient and should not be announced as interactive UI.
        return false;
    }

    protected override bool IsContentElementCore()
    {
        return false;
    }
}
