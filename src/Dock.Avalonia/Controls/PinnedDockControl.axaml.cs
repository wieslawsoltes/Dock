// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Threading;
using Dock.Model.Core;
using Dock.Model.Controls;
using Dock.Settings;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="PinnedDockControl"/> xaml.
/// </summary>
[TemplatePart("PART_PinnedDock", typeof(ContentControl)/*, IsRequired = true*/)]
[TemplatePart("PART_PinnedDockGrid", typeof(Grid)/*, IsRequired = true*/)]
[TemplatePart("PART_PinnedDockSplitter", typeof(GridSplitter))]
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
    private GridSplitter? _pinnedDockSplitter;
    private PinnedDockWindow? _window;
    private Window? _ownerWindow;

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
                if (_pinnedDockSplitter != null)
                {
                    Grid.SetRow(_pinnedDockSplitter, 0);
                    Grid.SetColumn(_pinnedDockSplitter, 1);
                }
                break;
            case Alignment.Bottom:
                _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star) { MinHeight = 50 });
                _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto) { MinHeight = 50 });
                Grid.SetColumn(_pinnedDock, 0);
                Grid.SetRow(_pinnedDock, 2);
                if (_pinnedDockSplitter != null)
                {
                    Grid.SetRow(_pinnedDockSplitter, 1);
                    Grid.SetColumn(_pinnedDockSplitter, 0);
                }
                break;
            case Alignment.Right:
                _pinnedDockGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star) { MinWidth = 50 });
                _pinnedDockGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                _pinnedDockGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto) { MinWidth = 50 });
                Grid.SetColumn(_pinnedDock, 2);
                Grid.SetRow(_pinnedDock, 0);
                if (_pinnedDockSplitter != null)
                {
                    Grid.SetRow(_pinnedDockSplitter, 0);
                    Grid.SetColumn(_pinnedDockSplitter, 1);
                }
                break;
            case Alignment.Top:
                _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto) { MinHeight = 50 });
                _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                _pinnedDockGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star) { MinHeight = 50 });
                Grid.SetColumn(_pinnedDock, 1);
                Grid.SetRow(_pinnedDock, 0);
                if (_pinnedDockSplitter != null)
                {
                    Grid.SetRow(_pinnedDockSplitter, 1);
                    Grid.SetColumn(_pinnedDockSplitter, 0);
                }
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
        _pinnedDockSplitter = e.NameScope.Find<GridSplitter>("PART_PinnedDockSplitter");
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
                    _ownerWindow = owner;
                    _ownerWindow.PositionChanged += OwnerWindow_PositionChanged;
                    _ownerWindow.Resized += OwnerWindow_Resized;
                    _ownerWindow.AddHandler(PointerPressedEvent, OwnerWindow_PointerPressed, RoutingStrategies.Tunnel);
                    _ownerWindow.Deactivated += OwnerWindow_Deactivated;
                    _window.Show(owner);
                }
            }

            var point = _pinnedDock.PointToScreen(new Point());
            _window.Position = new PixelPoint(point.X, point.Y);
            _window.Width = _pinnedDock.Bounds.Width;
            _window.Height = _pinnedDock.Bounds.Height;

            if (_pinnedDock.Opacity != 0)
            {
                _pinnedDock.Opacity = 0;
                _pinnedDock.IsHitTestVisible = false;
            }

            // Keep the splitter visible so the user can resize the docked area
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

        if (_ownerWindow is not null)
        {
            _ownerWindow.PositionChanged -= OwnerWindow_PositionChanged;
            _ownerWindow.Resized -= OwnerWindow_Resized;
            _ownerWindow.RemoveHandler(PointerPressedEvent, OwnerWindow_PointerPressed);
            _ownerWindow.Deactivated -= OwnerWindow_Deactivated;
            _ownerWindow = null;
        }

        if (_pinnedDock is { Opacity: 0 })
        {
            _pinnedDock.ClearValue(OpacityProperty);
            _pinnedDock.ClearValue(IsHitTestVisibleProperty);
        }

    }

    private void OwnerWindow_PositionChanged(object? sender, PixelPointEventArgs e)
    {
        CloseWindow();
        UpdateWindow();
    }

    private void OwnerWindow_Resized(object? sender, EventArgs e)
    {
        UpdateWindow();
    }

    private bool IsPointerInsidePinnedDock(PointerEventArgs e)
    {
        if (_pinnedDockGrid is null)
            return false;

        var point = e.GetPosition(_pinnedDockGrid);
        return point.X >= 0 && point.Y >= 0 &&
               point.X <= _pinnedDockGrid.Bounds.Width &&
               point.Y <= _pinnedDockGrid.Bounds.Height;
    }

    private bool ShouldKeepPinnedDockableVisible()
    {
        if (DataContext is not IRootDock rootDock)
        {
            return false;
        }

        if (rootDock.PinnedDock?.VisibleDockables is null)
        {
            return false;
        }

        foreach (var dockable in rootDock.PinnedDock.VisibleDockables)
        {
            if (dockable.KeepPinnedDockableVisible)
            {
                return true;
            }
        }

        return false;
    }

    private void OwnerWindow_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ShouldKeepPinnedDockableVisible())
        {
            return;
        }

        if (!IsPointerInsidePinnedDock(e))
        {
            CloseWindow();
        }
    }

    private void OwnerWindow_Deactivated(object? sender, EventArgs e)
    {
        if (ShouldKeepPinnedDockableVisible())
        {
            return;
        }

        CloseWindow();
    }
}
