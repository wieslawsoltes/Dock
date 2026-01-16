// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using Dock.Model.CommandBars;

namespace Dock.Avalonia.CommandBars;

/// <summary>
/// Default adapter that renders menus and toolbars from command bar definitions.
/// </summary>
public sealed class DefaultDockCommandBarAdapter : IDockCommandBarAdapter
{
    /// <inheritdoc/>
    public IReadOnlyList<Control> BuildBars(DockCommandBarKind kind, IReadOnlyList<DockCommandBarDefinition> definitions)
    {
        var bars = new List<Control>();
        foreach (var definition in definitions.OrderBy(definition => definition.Order))
        {
            if (definition.Content is Control control)
            {
                bars.Add(control);
                continue;
            }

            if (definition.Items is null || definition.Items.Count == 0)
            {
                if (definition.Content is not null)
                {
                    bars.Add(new ContentControl { Content = definition.Content });
                }
                continue;
            }

            switch (kind)
            {
                case DockCommandBarKind.Menu:
                    bars.Add(BuildMenu(definition.Items));
                    break;
                case DockCommandBarKind.ToolBar:
                    bars.Add(BuildToolBar(definition.Items));
                    break;
                case DockCommandBarKind.Ribbon:
                    bars.Add(BuildRibbon(definition.Items));
                    break;
            }
        }

        return bars;
    }

    private static Control BuildMenu(IReadOnlyList<DockCommandBarItem> items)
    {
        var menu = new Menu
        {
            ItemsSource = BuildMenuItems(items)
        };

        return menu;
    }

    private static IList<object> BuildMenuItems(IReadOnlyList<DockCommandBarItem> items)
    {
        var list = new List<object>();
        foreach (var item in items.OrderBy(child => child.Order))
        {
            if (item.IsSeparator)
            {
                list.Add(new Separator());
                continue;
            }

            var menuItem = new MenuItem
            {
                Header = item.Header,
                Icon = item.Icon,
                Command = item.Command,
                CommandParameter = item.CommandParameter
            };

            if (item.Items is { Count: > 0 })
            {
                menuItem.ItemsSource = BuildMenuItems(item.Items);
            }

            list.Add(menuItem);
        }

        return list;
    }

    private static Control BuildToolBar(IReadOnlyList<DockCommandBarItem> items)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4
        };

        foreach (var item in items.OrderBy(child => child.Order))
        {
            if (item.IsSeparator)
            {
                panel.Children.Add(new Separator());
                continue;
            }

            var content = BuildButtonContent(item);
            var button = new Button
            {
                Content = content,
                Command = item.Command,
                CommandParameter = item.CommandParameter
            };
            panel.Children.Add(button);
        }

        return panel;
    }

    private static Control BuildRibbon(IReadOnlyList<DockCommandBarItem> items)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6
        };

        foreach (var item in items.OrderBy(child => child.Order))
        {
            if (item.IsSeparator)
            {
                panel.Children.Add(new Separator());
                continue;
            }

            panel.Children.Add(new ContentControl { Content = item.Header });
        }

        return panel;
    }

    private static object? BuildButtonContent(DockCommandBarItem item)
    {
        if (item.Icon is null)
        {
            return item.Header;
        }

        if (item.Header is null)
        {
            return item.Icon;
        }

        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4
        };

        panel.Children.Add(new ContentControl { Content = item.Icon });
        panel.Children.Add(new ContentControl { Content = item.Header });
        return panel;
    }
}
