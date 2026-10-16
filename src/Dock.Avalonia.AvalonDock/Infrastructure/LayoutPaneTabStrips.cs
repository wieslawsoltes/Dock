using System;
using Avalonia.Controls;
using Dock.Avalonia.Controls;

namespace AvalonDock.Infrastructure;

// AvalonDock panes allow an explicit empty selection. Ordinary Dock strips
// retain their AlwaysSelected default; these reuse all their input and themes.
internal sealed class LayoutDocumentTabStrip : DocumentTabStrip
{
    static LayoutDocumentTabStrip() => SelectionModeProperty.OverrideDefaultValue<LayoutDocumentTabStrip>(SelectionMode.Single);
    protected override Type StyleKeyOverride => typeof(DocumentTabStrip);
}

internal sealed class LayoutAnchorableTabStrip : ToolTabStrip
{
    static LayoutAnchorableTabStrip() => SelectionModeProperty.OverrideDefaultValue<LayoutAnchorableTabStrip>(SelectionMode.Single);
    protected override Type StyleKeyOverride => typeof(ToolTabStrip);
}
