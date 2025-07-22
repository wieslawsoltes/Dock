using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Internal;

internal class TabStripDragAdorner : Canvas
{
    private readonly TabStrip _tabStrip;
    private readonly Dictionary<TabStripItem, TabStripItem> _map = new();

    public TabStripDragAdorner(TabStrip tabStrip)
    {
        _tabStrip = tabStrip;
        IsHitTestVisible = false;
    }

    public void Show()
    {
        for (var i = 0; i < _tabStrip.Items.Count; i++)
        {
            if (_tabStrip.ItemContainerGenerator.ContainerFromIndex(i) is not TabStripItem item)
            {
                continue;
            }

            var clone = new DocumentTabStripItem
            {
                DataContext = item.DataContext,
                Template = item.Template,
                Width = item.Bounds.Width,
                Height = item.Bounds.Height,
                IsSelected = item.IsSelected,
                IsEnabled = item.IsEnabled
            };

            Children.Add(clone);
            SetLeft(clone, item.Bounds.X);
            SetTop(clone, item.Bounds.Y);
            _map[item] = clone;
            item.Opacity = 0;
        }
    }

    public void Hide()
    {
        foreach (var pair in _map)
        {
            pair.Key.Opacity = 1;
        }

        Children.Clear();
        _map.Clear();
    }

    public void Move(TabStripItem item, Vector delta)
    {
        if (_map.TryGetValue(item, out var clone))
        {
            SetLeft(clone, item.Bounds.X + delta.X);
            SetTop(clone, item.Bounds.Y + delta.Y);
        }
    }

    public double GetCenter(TabStripItem item)
    {
        if (_map.TryGetValue(item, out var clone))
        {
            return GetLeft(clone) + clone.Bounds.Width / 2;
        }

        return item.Bounds.X + item.Bounds.Width / 2;
    }

    public void UpdatePositions()
    {
        foreach (var pair in _map)
        {
            SetLeft(pair.Value, pair.Key.Bounds.X);
            SetTop(pair.Value, pair.Key.Bounds.Y);
        }
    }
}
