using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class ToolTabStripTemplateBindingTests
{
    [AvaloniaFact]
    public void Standalone_ToolTabStrip_Uses_All_Item_Templates()
    {
        var templates = CreateTemplates();
        var tool = new Tool { Id = "tool-1", Title = "Tool 1", IsModified = true };
        var tool2 = new Tool { Id = "tool-2", Title = "Tool 2", IsModified = false };
        var tabStrip = new ToolTabStrip
        {
            Width = 320,
            Height = 48,
            SelectedIndex = 0,
            IconTemplate = templates.IconTemplate,
            HeaderTemplate = templates.HeaderTemplate,
            ModifiedTemplate = templates.ModifiedTemplate,
            ItemsSource = new AvaloniaList<IDockable> { tool, tool2 }
        };

        var window = ShowInWindow(tabStrip);
        try
        {
            var tabItem = GetTabItem(tabStrip, 0);
            Assert.Same(templates.IconTemplate, GetPresenter(tabItem, "PART_IconPresenter").ContentTemplate);
            Assert.Same(templates.HeaderTemplate, GetPresenter(tabItem, "PART_HeaderPresenter").ContentTemplate);
            Assert.Same(templates.ModifiedTemplate, GetPresenter(tabItem, "PART_ModifiedPresenter").ContentTemplate);

            var renderedTexts = tabItem.GetVisualDescendants()
                .OfType<TextBlock>()
                .Select(textBlock => textBlock.Text ?? string.Empty)
                .ToArray();

            Assert.Contains("tool-icon:Tool 1", renderedTexts);
            Assert.Contains("tool-header:Tool 1", renderedTexts);
            Assert.Contains("tool-modified:Tool 1", renderedTexts);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ToolControl_Forwards_Templates_To_ToolTabStrip()
    {
        var templates = CreateTemplates();
        var factory = new Factory();
        var dock = new ToolDock
        {
            Factory = factory,
            VisibleDockables = factory.CreateList<IDockable>()
        };

        var tool = new Tool { Id = "tool-1", Title = "Tool 1", IsModified = true };
        var tool2 = new Tool { Id = "tool-2", Title = "Tool 2", IsModified = false };
        dock.VisibleDockables!.Add(tool);
        dock.VisibleDockables!.Add(tool2);
        dock.ActiveDockable = tool;

        var control = new ToolControl
        {
            DataContext = dock,
            IconTemplate = templates.IconTemplate,
            HeaderTemplate = templates.HeaderTemplate,
            ModifiedTemplate = templates.ModifiedTemplate
        };

        var window = ShowInWindow(control);
        try
        {
            var tabStrip = control.GetVisualDescendants().OfType<ToolTabStrip>().FirstOrDefault();
            Assert.NotNull(tabStrip);

            Assert.Same(templates.IconTemplate, tabStrip!.IconTemplate);
            Assert.Same(templates.HeaderTemplate, tabStrip.HeaderTemplate);
            Assert.Same(templates.ModifiedTemplate, tabStrip.ModifiedTemplate);

            var tabItem = GetTabItem(tabStrip, 0);
            Assert.Same(templates.IconTemplate, GetPresenter(tabItem, "PART_IconPresenter").ContentTemplate);
            Assert.Same(templates.HeaderTemplate, GetPresenter(tabItem, "PART_HeaderPresenter").ContentTemplate);
            Assert.Same(templates.ModifiedTemplate, GetPresenter(tabItem, "PART_ModifiedPresenter").ContentTemplate);

            var renderedTexts = tabItem.GetVisualDescendants()
                .OfType<TextBlock>()
                .Select(textBlock => textBlock.Text ?? string.Empty)
                .ToArray();

            Assert.Contains("tool-icon:Tool 1", renderedTexts);
            Assert.Contains("tool-header:Tool 1", renderedTexts);
            Assert.Contains("tool-modified:Tool 1", renderedTexts);
        }
        finally
        {
            window.Close();
        }
    }

    private static (IDataTemplate IconTemplate, IDataTemplate HeaderTemplate, IDataTemplate ModifiedTemplate) CreateTemplates()
    {
        var iconTemplate = new FuncDataTemplate<IDockable>((dockable, _) => new TextBlock { Text = $"tool-icon:{dockable.Title}" }, true);
        var headerTemplate = new FuncDataTemplate<IDockable>((dockable, _) => new TextBlock { Text = $"tool-header:{dockable.Title}" }, true);
        var modifiedTemplate = new FuncDataTemplate<IDockable>((dockable, _) => new TextBlock { Text = $"tool-modified:{dockable.Title}" }, true);
        return (iconTemplate, headerTemplate, modifiedTemplate);
    }

    private static Window ShowInWindow(Control control)
    {
        var window = new Window
        {
            Width = 600,
            Height = 400,
            Content = control
        };

        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();
        control.UpdateLayout();
        return window;
    }

    private static ToolTabStripItem GetTabItem(ToolTabStrip tabStrip, int index)
    {
        var tabItem = tabStrip.ContainerFromIndex(index) as ToolTabStripItem;
        Assert.NotNull(tabItem);
        tabItem!.ApplyTemplate();
        tabItem.UpdateLayout();
        return tabItem;
    }

    private static ContentPresenter GetPresenter(ToolTabStripItem tabItem, string presenterName)
    {
        var presenter = tabItem.GetVisualDescendants()
            .OfType<ContentPresenter>()
            .FirstOrDefault(candidate => candidate.Name == presenterName);
        Assert.NotNull(presenter);
        return presenter!;
    }
}
