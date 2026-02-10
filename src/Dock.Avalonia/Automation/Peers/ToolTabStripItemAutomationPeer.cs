// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Automation.Peers;

internal sealed class ToolTabStripItemAutomationPeer : DockTabStripItemAutomationPeer<ToolTabStripItem>
{
    internal ToolTabStripItemAutomationPeer(ToolTabStripItem owner)
        : base(owner)
    {
    }

    protected override string ClassName => nameof(ToolTabStripItem);

    protected override string FallbackName => "Tool tab";

    protected override string BuildStateText(IDockable? dockable)
    {
        return DockAutomationPeerHelper.FormatState(
            ("Selected", OwnerControl.IsSelected),
            ("CanClose", dockable?.CanClose ?? false),
            ("CanFloat", dockable?.CanFloat ?? false),
            ("DockingState", dockable?.DockingState ?? DockingWindowState.None));
    }
}
