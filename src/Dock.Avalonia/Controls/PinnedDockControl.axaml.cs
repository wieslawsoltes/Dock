using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="PinnedDockControl"/> xaml.
/// </summary>
[TemplatePart("PART_PinnedDock", typeof(ContentControl)/*, IsRequired = true*/)]
[TemplatePart("PART_PinnedDockGrid", typeof(Grid)/*, IsRequired = true*/)]
public class PinnedDockControl : TemplatedControl
{
    /// <summary>
    /// Define the <see cref="PinnedDockAlignment"/> property.
    /// </summary>
    public static readonly StyledProperty<Alignment> PinnedDockAlignmentProperty = AvaloniaProperty.Register<PinnedDockControl, Alignment>(nameof(PinnedDockAlignment));

    /// <summary>
    /// Gets or sets pinned dock alignment
    /// </summary>
    public Alignment PinnedDockAlignment
    {
        get => GetValue(PinnedDockAlignmentProperty);
        set => SetValue(PinnedDockAlignmentProperty, value);
    }

    private Grid? _pinnedDockGrid;
    private ContentControl? _pinnedDock;

    static PinnedDockControl()
    {
        PinnedDockAlignmentProperty.Changed.AddClassHandler<PinnedDockControl>((control, e) => control.UpdateGrid());
    }

    private void UpdateGrid()
    {
        if (_pinnedDockGrid == null || _pinnedDock == null)
            return;

        _pinnedDockGrid.RowDefinitions.Clear();
        _pinnedDockGrid.ColumnDefinitions.Clear();
        switch (PinnedDockAlignment)
        {
            case Alignment.Unset:
            case Alignment.Left:
                _pinnedDockGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto) { MinWidth = 50 });
                _pinnedDockGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                _pinnedDockGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star) { MinWidth = 50 });
                Grid.SetColumn(_pinnedDock, 0);
                Grid.SetRow(_pinnedDock, 0);
                break;
            case Alignment.Bottom:
                _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star) { MinHeight = 50 });
                _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto) { MinHeight = 50 });
                Grid.SetColumn(_pinnedDock, 0);
                Grid.SetRow(_pinnedDock, 2);
                break;
            case Alignment.Right:
                _pinnedDockGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star) { MinWidth = 50 });
                _pinnedDockGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                _pinnedDockGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto) { MinWidth = 50 });
                Grid.SetColumn(_pinnedDock, 2);
                Grid.SetRow(_pinnedDock, 0);
                break;
            case Alignment.Top:
                _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto) { MinHeight = 50 });
                _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star) { MinHeight = 50 });
                Grid.SetColumn(_pinnedDock, 1);
                Grid.SetRow(_pinnedDock, 0);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _pinnedDockGrid = e.NameScope.Get<Grid>("PART_PinnedDockGrid");
        _pinnedDock = e.NameScope.Get<ContentControl>("PART_PinnedDock");
        UpdateGrid();
    }
}

