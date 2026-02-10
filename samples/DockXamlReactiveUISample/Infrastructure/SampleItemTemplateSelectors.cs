using System;
using Avalonia.Controls;
using Dock.Model.Core;
using DockXamlReactiveUISample.Models;

namespace DockXamlReactiveUISample.Infrastructure;

public sealed class SampleDocumentItemTemplateSelector : IDocumentItemTemplateSelector
{
    public object? SelectTemplate(IItemsSourceDock dock, object item, int index)
    {
        if (item is not DocumentItem documentItem || index != 0)
        {
            return null;
        }

        return new Func<IServiceProvider, object>(_ =>
        {
            var panel = new StackPanel
            {
                Margin = new Avalonia.Thickness(12),
                Spacing = 6
            };

            panel.Children.Add(new TextBlock
            {
                Text = $"Selector template for {documentItem.Title}",
                FontSize = 16,
                FontWeight = Avalonia.Media.FontWeight.SemiBold
            });

            panel.Children.Add(new TextBlock
            {
                Text = documentItem.Content,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            });

            panel.Children.Add(new TextBlock
            {
                Text = "All other documents use DocumentDock.DocumentTemplate.",
                Opacity = 0.75
            });

            panel.DataContext = documentItem;
            return panel;
        });
    }
}

public sealed class SampleToolItemTemplateSelector : IToolItemTemplateSelector
{
    public object? SelectTemplate(IToolItemsSourceDock dock, object item, int index)
    {
        if (item is not ToolItem toolItem || index != 0)
        {
            return null;
        }

        return new Func<IServiceProvider, object>(_ =>
        {
            var panel = new StackPanel
            {
                Margin = new Avalonia.Thickness(12),
                Spacing = 6
            };

            panel.Children.Add(new TextBlock
            {
                Text = $"Selector template for {toolItem.Title}",
                FontSize = 15,
                FontWeight = Avalonia.Media.FontWeight.SemiBold
            });

            panel.Children.Add(new TextBlock
            {
                Text = toolItem.Description,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            });

            panel.Children.Add(new TextBlock
            {
                Text = "All other tools use ToolDock.ToolTemplate.",
                Opacity = 0.75
            });

            panel.DataContext = toolItem;
            return panel;
        });
    }
}
