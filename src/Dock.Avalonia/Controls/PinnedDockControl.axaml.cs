// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Dock.Model.Core;
using Dock.Model.Controls;

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
    public static readonly StyledProperty<Alignment> PinnedDockAlignmentProperty =
        AvaloniaProperty.Register<PinnedDockControl, Alignment>(nameof(PinnedDockAlignment));

    /// <summary>
    /// Defines the <see cref="LeftPinnedDockablesAlignment"/> property.
    /// </summary>
    public static readonly StyledProperty<Alignment> LeftPinnedDockablesAlignmentProperty =
        AvaloniaProperty.Register<PinnedDockControl, Alignment>(nameof(LeftPinnedDockablesAlignment), Alignment.Top);

    /// <summary>
    /// Defines the <see cref="RightPinnedDockablesAlignment"/> property.
    /// </summary>
    public static readonly StyledProperty<Alignment> RightPinnedDockablesAlignmentProperty =
        AvaloniaProperty.Register<PinnedDockControl, Alignment>(nameof(RightPinnedDockablesAlignment), Alignment.Top);

    /// <summary>
    /// Defines the <see cref="TopPinnedDockablesAlignment"/> property.
    /// </summary>
    public static readonly StyledProperty<Alignment> TopPinnedDockablesAlignmentProperty =
        AvaloniaProperty.Register<PinnedDockControl, Alignment>(nameof(TopPinnedDockablesAlignment), Alignment.Left);

    /// <summary>
    /// Defines the <see cref="BottomPinnedDockablesAlignment"/> property.
    /// </summary>
    public static readonly StyledProperty<Alignment> BottomPinnedDockablesAlignmentProperty =
        AvaloniaProperty.Register<PinnedDockControl, Alignment>(nameof(BottomPinnedDockablesAlignment), Alignment.Left);

    /// <summary>
    /// Gets or sets pinned dock alignment
    /// </summary>
    public Alignment PinnedDockAlignment
    {
        get => GetValue(PinnedDockAlignmentProperty);
        set => SetValue(PinnedDockAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets left pinned dockables alignment.
    /// </summary>
    public Alignment LeftPinnedDockablesAlignment
    {
        get => GetValue(LeftPinnedDockablesAlignmentProperty);
        set => SetValue(LeftPinnedDockablesAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets right pinned dockables alignment.
    /// </summary>
    public Alignment RightPinnedDockablesAlignment
    {
        get => GetValue(RightPinnedDockablesAlignmentProperty);
        set => SetValue(RightPinnedDockablesAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets top pinned dockables alignment.
    /// </summary>
    public Alignment TopPinnedDockablesAlignment
    {
        get => GetValue(TopPinnedDockablesAlignmentProperty);
        set => SetValue(TopPinnedDockablesAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets bottom pinned dockables alignment.
    /// </summary>
    public Alignment BottomPinnedDockablesAlignment
    {
        get => GetValue(BottomPinnedDockablesAlignmentProperty);
        set => SetValue(BottomPinnedDockablesAlignmentProperty, value);
    }

    private Grid? _pinnedDockGrid;
    private ContentControl? _pinnedDock;

    static PinnedDockControl()
    {
        PinnedDockAlignmentProperty.Changed.AddClassHandler<PinnedDockControl>((control, e) => control.UpdateGrid());
        LeftPinnedDockablesAlignmentProperty.Changed.AddClassHandler<PinnedDockControl>((control, e) => control.UpdateGrid());
        RightPinnedDockablesAlignmentProperty.Changed.AddClassHandler<PinnedDockControl>((control, e) => control.UpdateGrid());
        TopPinnedDockablesAlignmentProperty.Changed.AddClassHandler<PinnedDockControl>((control, e) => control.UpdateGrid());
        BottomPinnedDockablesAlignmentProperty.Changed.AddClassHandler<PinnedDockControl>((control, e) => control.UpdateGrid());
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
                _pinnedDockGrid.VerticalAlignment = LeftPinnedDockablesAlignment == Alignment.Bottom ?
                    VerticalAlignment.Bottom : VerticalAlignment.Top;
                _pinnedDockGrid.HorizontalAlignment = HorizontalAlignment.Left;
                break;
            case Alignment.Bottom:
                _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star) { MinHeight = 50 });
                _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto) { MinHeight = 50 });
                Grid.SetColumn(_pinnedDock, 0);
                Grid.SetRow(_pinnedDock, 2);
                _pinnedDockGrid.HorizontalAlignment = BottomPinnedDockablesAlignment == Alignment.Right ?
                    HorizontalAlignment.Right : HorizontalAlignment.Left;
                _pinnedDockGrid.VerticalAlignment = VerticalAlignment.Bottom;
                break;
            case Alignment.Right:
                _pinnedDockGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star) { MinWidth = 50 });
                _pinnedDockGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                _pinnedDockGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto) { MinWidth = 50 });
                Grid.SetColumn(_pinnedDock, 2);
                Grid.SetRow(_pinnedDock, 0);
                _pinnedDockGrid.VerticalAlignment = RightPinnedDockablesAlignment == Alignment.Bottom ?
                    VerticalAlignment.Bottom : VerticalAlignment.Top;
                _pinnedDockGrid.HorizontalAlignment = HorizontalAlignment.Right;
                break;
            case Alignment.Top:
                _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto) { MinHeight = 50 });
                _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star) { MinHeight = 50 });
                Grid.SetColumn(_pinnedDock, 1);
                Grid.SetRow(_pinnedDock, 0);
                _pinnedDockGrid.HorizontalAlignment = TopPinnedDockablesAlignment == Alignment.Right ?
                    HorizontalAlignment.Right : HorizontalAlignment.Left;
                _pinnedDockGrid.VerticalAlignment = VerticalAlignment.Top;
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

    /// <inheritdoc/>
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        UpdateGrid();
    }
}

