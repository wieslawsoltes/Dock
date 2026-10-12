// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace Dock.Avalonia.Internal;

// A resource-backed flyout remembers its last button even before it is opened.
// Release that reference when chrome unloads, while preserving its binding and
// the menu instance for reattachment.
internal sealed class ToolMenuLifetime
{
    private Button? _button;
    private FlyoutBase? _detachedFlyout;

    public void SetButton(Button? button)
    {
        Detach();
        _button = button;
        _detachedFlyout = null;
    }

    public void Attach()
    {
        if (_button is { Flyout: null } button && _detachedFlyout is { } flyout)
        {
            if (BindingOperations.GetBindingExpressionBase(button, Button.FlyoutProperty) is { } binding)
                binding.UpdateTarget();
            else
                button.SetCurrentValue(Button.FlyoutProperty, flyout);
        }
        _detachedFlyout = null;
    }

    public void Detach()
    {
        if (_button is not { Flyout: { } flyout } button) return;
        _detachedFlyout = flyout;
        button.SetCurrentValue(Button.FlyoutProperty, null);
    }
}
