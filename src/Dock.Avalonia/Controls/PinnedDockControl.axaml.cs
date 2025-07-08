// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using Dock.Model.Core;
using Dock.Model.Controls;
using Dock.Settings;

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
    private PinnedDockWindow? _window;

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

        if (DockSettings.UsePinnedDockWindow)
        {
            LayoutUpdated += OnLayoutUpdated;
            this.AttachedToVisualTree += OnAttached;
            this.DetachedFromVisualTree += OnDetached;
        }
    }

    private void OnAttached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        UpdateWindow();
    }

    private void OnDetached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        CloseWindow();
        LayoutUpdated -= OnLayoutUpdated;
        this.AttachedToVisualTree -= OnAttached;
        this.DetachedFromVisualTree -= OnDetached;
    }

    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        UpdateWindow();
    }

    private void UpdateWindow()
    {
        if (!DockSettings.UsePinnedDockWindow || _pinnedDockGrid is null || _pinnedDock is null)
        {
            return;
        }

        if (DataContext is not IRootDock root || root.PinnedDock is null)
        {
            CloseWindow();
            return;
        }

        if (!root.PinnedDock.IsEmpty)
        {
            if (_window is null)
            {
                _window = new PinnedDockWindow
                {
                    Content = new ToolDockControl
                    {
                        DataContext = root.PinnedDock
                    }
                };

                if (this.GetVisualRoot() is Window owner)
                {
                    _window.Show(owner);
                }
            }

            var point = _pinnedDock.PointToScreen(new Point());
            _window.Position = new PixelPoint(point.X, point.Y);
            _window.Width = _pinnedDock.Bounds.Width;
            _window.Height = _pinnedDock.Bounds.Height;

            if (_pinnedDockGrid.IsVisible)
            {
                _pinnedDockGrid.IsVisible = false;
            }
        }
        else
        {
            CloseWindow();
        }
    }

    private void CloseWindow()
    {
        if (_window is not null)
        {
            _window.Close();
            _window = null;
        }

        if (_pinnedDockGrid is { IsVisible: false })
        {
            _pinnedDockGrid.IsVisible = true;
        }
    }
}

