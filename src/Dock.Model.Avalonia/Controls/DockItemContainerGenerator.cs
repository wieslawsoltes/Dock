// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Default ItemsSource container generator for document and tool docks.
/// </summary>
public class DockItemContainerGenerator : IDockItemContainerGenerator
{
    /// <summary>
    /// Gets the shared default container generator instance.
    /// </summary>
    public static DockItemContainerGenerator Default { get; } = new();

    /// <inheritdoc />
    public virtual IDockable? CreateDocumentContainer(IItemsSourceDock dock, object item, int index)
    {
        var hasDocumentTemplate = dock is IDocumentDockContent { DocumentTemplate: not null };
        if (!hasDocumentTemplate)
        {
            var selectorContent = SelectDocumentTemplateContent(dock, item, index);
            if (selectorContent is null)
            {
                return null;
            }

            return new Document
            {
                Id = Guid.NewGuid().ToString(),
                Content = selectorContent
            };
        }

        return new Document
        {
            Id = Guid.NewGuid().ToString()
        };
    }

    /// <inheritdoc />
    public virtual void PrepareDocumentContainer(IItemsSourceDock dock, IDockable container, object item, int index)
    {
        container.Title = GetItemTitle(item);
        container.Context = item;
        container.CanClose = GetItemCanClose(item);
        ApplyGeneratedContainerMetadata(container, dock.DocumentItemContainerTheme, dock.DocumentItemTemplateSelector);

        if (container is not IDocumentContent content)
        {
            return;
        }

        if (dock is not IDocumentDockContent { DocumentTemplate: not null } && content.Content != null)
        {
            return;
        }

        var selectorContent = SelectDocumentTemplateContent(dock, item, index);
        if (selectorContent != null)
        {
            content.Content = selectorContent;
            return;
        }

        if (dock is not IDocumentDockContent { DocumentTemplate: var template } || template is null)
        {
            content.Content = null;
            return;
        }

        if (template.Content != null)
        {
            content.Content = template.Content;
            return;
        }

        content.Content = new Func<IServiceProvider, object>(_ => CreateDocumentFallbackContent(container, item));
    }

    /// <inheritdoc />
    public virtual void ClearDocumentContainer(IItemsSourceDock dock, IDockable container, object? item)
    {
        container.Context = null;
        ClearGeneratedContainerMetadata(container);

        if (container is IDocumentContent content)
        {
            content.Content = null;
        }
    }

    /// <inheritdoc />
    public virtual IDockable? CreateToolContainer(IToolItemsSourceDock dock, object item, int index)
    {
        var hasToolTemplate = dock is IToolDockContent { ToolTemplate: not null };
        if (!hasToolTemplate)
        {
            var selectorContent = SelectToolTemplateContent(dock, item, index);
            if (selectorContent is null)
            {
                return null;
            }

            return new Tool
            {
                Id = Guid.NewGuid().ToString(),
                Content = selectorContent
            };
        }

        return new Tool
        {
            Id = Guid.NewGuid().ToString()
        };
    }

    /// <inheritdoc />
    public virtual void PrepareToolContainer(IToolItemsSourceDock dock, IDockable container, object item, int index)
    {
        container.Title = GetItemTitle(item);
        container.Context = item;
        container.CanClose = GetItemCanClose(item);
        ApplyGeneratedContainerMetadata(container, dock.ToolItemContainerTheme, dock.ToolItemTemplateSelector);

        if (container is not IToolContent content)
        {
            return;
        }

        if (dock is not IToolDockContent { ToolTemplate: not null } && content.Content != null)
        {
            return;
        }

        var selectorContent = SelectToolTemplateContent(dock, item, index);
        if (selectorContent != null)
        {
            content.Content = selectorContent;
            return;
        }

        if (dock is not IToolDockContent { ToolTemplate: var template } || template is null)
        {
            content.Content = null;
            return;
        }

        if (template.Content != null)
        {
            content.Content = template.Content;
            return;
        }

        content.Content = new Func<IServiceProvider, object>(_ => CreateToolFallbackContent(container, item));
    }

    /// <inheritdoc />
    public virtual void ClearToolContainer(IToolItemsSourceDock dock, IDockable container, object? item)
    {
        container.Context = null;
        ClearGeneratedContainerMetadata(container);

        if (container is IToolContent content)
        {
            content.Content = null;
        }
    }

    /// <summary>
    /// Resolves a source item title.
    /// </summary>
    /// <param name="item">The source item.</param>
    /// <returns>The resolved title text.</returns>
    protected virtual string GetItemTitle(object item)
    {
        var type = item.GetType();

        var titleProperty = type.GetProperty("Title");
        if (titleProperty?.GetValue(item) is string title)
        {
            return title;
        }

        var nameProperty = type.GetProperty("Name");
        if (nameProperty?.GetValue(item) is string name)
        {
            return name;
        }

        var displayNameProperty = type.GetProperty("DisplayName");
        if (displayNameProperty?.GetValue(item) is string displayName)
        {
            return displayName;
        }

        return item.ToString() ?? type.Name;
    }

    /// <summary>
    /// Resolves whether a source item can be closed.
    /// </summary>
    /// <param name="item">The source item.</param>
    /// <returns><c>true</c> when the generated container should be closable.</returns>
    protected virtual bool GetItemCanClose(object item)
    {
        var canCloseProperty = item.GetType().GetProperty("CanClose");
        if (canCloseProperty?.GetValue(item) is bool canClose)
        {
            return canClose;
        }

        return true;
    }

    private static void ApplyGeneratedContainerMetadata(
        IDockable container,
        object? containerTheme,
        object? templateSelector)
    {
        if (container is not IDockItemContainerMetadata metadata)
        {
            return;
        }

        metadata.ItemContainerTheme = containerTheme;
        metadata.ItemTemplateSelector = templateSelector;
    }

    private static void ClearGeneratedContainerMetadata(IDockable container)
    {
        if (container is not IDockItemContainerMetadata metadata)
        {
            return;
        }

        metadata.ItemContainerTheme = null;
        metadata.ItemTemplateSelector = null;
    }

    private static object? SelectDocumentTemplateContent(IItemsSourceDock dock, object item, int index)
    {
        if (dock.DocumentItemTemplateSelector is null)
        {
            return null;
        }

        var selected = dock.DocumentItemTemplateSelector.SelectTemplate(dock, item, index);
        if (selected is IDocumentTemplate documentTemplate)
        {
            return documentTemplate.Content;
        }

        return selected;
    }

    private static object? SelectToolTemplateContent(IToolItemsSourceDock dock, object item, int index)
    {
        if (dock.ToolItemTemplateSelector is null)
        {
            return null;
        }

        var selected = dock.ToolItemTemplateSelector.SelectTemplate(dock, item, index);
        if (selected is IToolTemplate toolTemplate)
        {
            return toolTemplate.Content;
        }

        return selected;
    }

    private static Control CreateDocumentFallbackContent(IDockable document, object item)
    {
        var contentPanel = new StackPanel
        {
            Margin = new Thickness(10)
        };

        var titleBlock = new TextBlock
        {
            Text = document.Title ?? "Document",
            FontWeight = FontWeight.Bold,
            FontSize = 16,
            Background = Brushes.LightBlue,
            Padding = new Thickness(5),
            Margin = new Thickness(0, 0, 0, 10)
        };
        contentPanel.Children.Add(titleBlock);

        var contentBlock = new TextBlock
        {
            Text = item.ToString() ?? "No content",
            Background = Brushes.LightGray,
            Padding = new Thickness(5),
            TextWrapping = TextWrapping.Wrap
        };
        contentPanel.Children.Add(contentBlock);

        contentPanel.DataContext = item;
        return contentPanel;
    }

    private static Control CreateToolFallbackContent(IDockable tool, object item)
    {
        var contentPanel = new StackPanel
        {
            Margin = new Thickness(10)
        };

        var titleBlock = new TextBlock
        {
            Text = tool.Title ?? "Tool",
            FontWeight = FontWeight.Bold,
            FontSize = 16,
            Background = Brushes.LightSteelBlue,
            Padding = new Thickness(5),
            Margin = new Thickness(0, 0, 0, 10)
        };
        contentPanel.Children.Add(titleBlock);

        var contentBlock = new TextBlock
        {
            Text = item.ToString() ?? "No content",
            Background = Brushes.LightGray,
            Padding = new Thickness(5),
            TextWrapping = TextWrapping.Wrap
        };
        contentPanel.Children.Add(contentBlock);

        contentPanel.DataContext = item;
        return contentPanel;
    }
}
