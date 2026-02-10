// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Automation.Peers;

internal sealed class DockAdornerWindowAutomationPeer : WindowAutomationPeer
{
    private readonly DockAdornerWindow _owner;

    internal DockAdornerWindowAutomationPeer(DockAdornerWindow owner)
        : base(owner)
    {
        _owner = owner;
    }

    protected override string GetClassNameCore()
    {
        return nameof(DockAdornerWindow);
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

        return DockAutomationPeerHelper.ResolveName(_owner, "Dock adorner window");
    }

    protected override string GetHelpTextCore()
    {
        return DockAutomationPeerHelper.FormatState(
            ("Visible", _owner.IsVisible),
            ("WindowState", _owner.WindowState));
    }

    protected override bool IsControlElementCore()
    {
        // Dock adorner visuals are decorative and not user-invokable content.
        return false;
    }

    protected override bool IsContentElementCore()
    {
        return false;
    }
}
