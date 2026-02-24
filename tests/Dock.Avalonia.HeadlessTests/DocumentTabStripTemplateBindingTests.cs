using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DocumentTabStripTemplateBindingTests
{
    [AvaloniaFact]
    public void Standalone_DocumentTabStrip_Uses_All_Item_Templates()
    {
        var templates = CreateTemplates();
        var document = new Document { Id = "doc-1", Title = "Doc 1", IsModified = true };
        var tabStrip = new DocumentTabStrip
        {
            Width = 320,
            Height = 48,
            IconTemplate = templates.IconTemplate,
            HeaderTemplate = templates.HeaderTemplate,
            ModifiedTemplate = templates.ModifiedTemplate,
            CloseTemplate = templates.CloseTemplate,
            ItemsSource = new AvaloniaList<IDockable> { document }
        };

        var window = ShowInWindow(tabStrip);
        try
        {
            var tabItem = GetTabItem(tabStrip, 0);

            Assert.Same(templates.IconTemplate, GetPresenter(tabItem, "PART_IconPresenter").ContentTemplate);
            Assert.Same(templates.HeaderTemplate, GetPresenter(tabItem, "PART_HeaderPresenter").ContentTemplate);
            Assert.Same(templates.ModifiedTemplate, GetPresenter(tabItem, "PART_ModifiedPresenter").ContentTemplate);
            Assert.Same(templates.CloseTemplate, GetPresenter(tabItem, "PART_ClosePresenter").ContentTemplate);

            var renderedTexts = tabItem.GetVisualDescendants()
                .OfType<TextBlock>()
                .Select(textBlock => textBlock.Text ?? string.Empty)
                .ToArray();

            Assert.Contains("icon:Doc 1", renderedTexts);
            Assert.Contains("header:Doc 1", renderedTexts);
            Assert.Contains("modified:Doc 1", renderedTexts);
            Assert.Contains("close:Doc 1", renderedTexts);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DocumentControl_Forwards_Templates_To_DocumentTabStrip()
    {
        var templates = CreateTemplates();
        var factory = new Factory();
        var dock = new DocumentDock
        {
            Factory = factory,
            LayoutMode = DocumentLayoutMode.Tabbed,
            VisibleDockables = factory.CreateList<IDockable>()
        };

        var document = new Document { Id = "doc-1", Title = "Doc 1", IsModified = true };
        dock.VisibleDockables!.Add(document);
        dock.ActiveDockable = document;

        var control = new DocumentControl
        {
            DataContext = dock,
            IconTemplate = templates.IconTemplate,
            HeaderTemplate = templates.HeaderTemplate,
            ModifiedTemplate = templates.ModifiedTemplate,
            CloseTemplate = templates.CloseTemplate
        };

        var window = ShowInWindow(control);
        try
        {
            var tabStrip = control.GetVisualDescendants().OfType<DocumentTabStrip>().FirstOrDefault();
            Assert.NotNull(tabStrip);

            Assert.Same(templates.IconTemplate, tabStrip!.IconTemplate);
            Assert.Same(templates.HeaderTemplate, tabStrip.HeaderTemplate);
            Assert.Same(templates.ModifiedTemplate, tabStrip.ModifiedTemplate);
            Assert.Same(templates.CloseTemplate, tabStrip.CloseTemplate);

            var tabItem = GetTabItem(tabStrip, 0);
            Assert.Same(templates.CloseTemplate, GetPresenter(tabItem, "PART_ClosePresenter").ContentTemplate);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Standalone_DocumentTabStrip_Uses_Default_Close_Theme_And_Template()
    {
        var document = new Document { Id = "doc-1", Title = "Doc 1" };
        var tabStrip = new DocumentTabStrip
        {
            Width = 320,
            Height = 48,
            ItemsSource = new AvaloniaList<IDockable> { document }
        };

        var window = ShowInWindow(tabStrip);
        try
        {
            Assert.True(tabStrip.TryFindResource("DocumentCloseButtonTheme", out var closeThemeResource));
            var closeButtonTheme = Assert.IsType<ControlTheme>(closeThemeResource);
            Assert.Same(closeButtonTheme, tabStrip.CloseButtonTheme);

            Assert.True(tabStrip.TryFindResource("DockDefaultDocumentCloseTemplate", out var closeTemplateResource));
            var closeTemplate = Assert.IsAssignableFrom<IDataTemplate>(closeTemplateResource);
            Assert.Same(closeTemplate, tabStrip.CloseTemplate);

            var tabItem = GetTabItem(tabStrip, 0);
            Assert.Same(tabStrip.CloseTemplate, GetPresenter(tabItem, "PART_ClosePresenter").ContentTemplate);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DocumentControl_Hides_TabStrip_When_DockDocumentControlTabStripVisible_Is_False()
    {
        var factory = new Factory();
        var dock = new DocumentDock
        {
            Factory = factory,
            LayoutMode = DocumentLayoutMode.Tabbed,
            VisibleDockables = factory.CreateList<IDockable>()
        };

        var document = new Document { Id = "doc-1", Title = "Doc 1", IsModified = false };
        dock.VisibleDockables!.Add(document);
        dock.ActiveDockable = document;

        var control = new DocumentControl
        {
            DataContext = dock
        };

        control.Resources["DockDocumentControlTabStripVisible"] = false;

        var window = ShowInWindow(control);
        try
        {
            var tabStrip = control.GetVisualDescendants().OfType<DocumentTabStrip>().FirstOrDefault();
            Assert.NotNull(tabStrip);
            Assert.False(tabStrip!.IsVisible);

            var separatorHost = control.GetVisualDescendants()
                .OfType<Panel>()
                .FirstOrDefault(candidate => candidate.Name == "PART_DocumentSeperatorHost");
            Assert.NotNull(separatorHost);
            Assert.False(separatorHost!.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    private static (IDataTemplate IconTemplate, IDataTemplate HeaderTemplate, IDataTemplate ModifiedTemplate, IDataTemplate CloseTemplate) CreateTemplates()
    {
        var iconTemplate = new FuncDataTemplate<IDockable>((dockable, _) => new TextBlock { Text = $"icon:{dockable.Title}" }, true);
        var headerTemplate = new FuncDataTemplate<IDockable>((dockable, _) => new TextBlock { Text = $"header:{dockable.Title}" }, true);
        var modifiedTemplate = new FuncDataTemplate<IDockable>((dockable, _) => new TextBlock { Text = $"modified:{dockable.Title}" }, true);
        var closeTemplate = new FuncDataTemplate<IDockable>((dockable, _) => new TextBlock { Text = $"close:{dockable.Title}" }, true);
        return (iconTemplate, headerTemplate, modifiedTemplate, closeTemplate);
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

    private static DocumentTabStripItem GetTabItem(DocumentTabStrip tabStrip, int index)
    {
        var tabItem = tabStrip.ContainerFromIndex(index) as DocumentTabStripItem;
        Assert.NotNull(tabItem);
        tabItem!.ApplyTemplate();
        tabItem.UpdateLayout();
        return tabItem;
    }

    private static ContentPresenter GetPresenter(DocumentTabStripItem tabItem, string presenterName)
    {
        var presenter = tabItem.GetVisualDescendants()
            .OfType<ContentPresenter>()
            .FirstOrDefault(candidate => candidate.Name == presenterName);
        Assert.NotNull(presenter);
        return presenter!;
    }
}
