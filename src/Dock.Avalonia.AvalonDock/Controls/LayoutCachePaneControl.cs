using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AvalonDock.Controls;

/// <summary>Base tab control for AvalonDock panes, suppressing notifications for an empty selection.</summary>
public class LayoutCachePaneControl : TabControl
{
    static LayoutCachePaneControl()
    {
        SelectionModeProperty.OverrideDefaultValue<LayoutCachePaneControl>(SelectionMode.Single);
        SelectionChangedEvent.AddClassHandler<LayoutCachePaneControl>(static (control, args) =>
        {
            if (ReferenceEquals(args.Source, control)) control.OnSelectionChanged(args);
        });
    }

    /// <summary>Creates a pane tab control.</summary>
    public LayoutCachePaneControl() { }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(LayoutCachePaneControl);

    /// <summary>Marks an empty-selection notification handled before it bubbles to a parent.</summary>
    protected virtual void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        if (SelectedIndex < 0) e.Handled = true;
    }
}
