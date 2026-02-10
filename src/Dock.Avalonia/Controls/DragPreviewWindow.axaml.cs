// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Dock.Avalonia.Automation.Peers;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Window used to display drag preview content.
/// </summary>
public class DragPreviewWindow : Window
{
    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new DragPreviewWindowAutomationPeer(this);
    }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(DragPreviewWindow);
}
