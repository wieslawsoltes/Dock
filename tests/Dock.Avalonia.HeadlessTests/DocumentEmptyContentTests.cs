using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DocumentEmptyContentTests
{
    [AvaloniaFact]
    public void DocumentControl_Shows_EmptyContent_When_NoDocuments()
    {
        var factory = new Factory();
        var dock = new DocumentDock
        {
            Factory = factory,
            LayoutMode = DocumentLayoutMode.Tabbed,
            VisibleDockables = factory.CreateList<IDockable>(),
            EmptyContent = "No documents are open."
        };

        var control = new DocumentControl { DataContext = dock };
        var window = new Window
        {
            Width = 800,
            Height = 600,
            Content = control
        };

        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();
        control.UpdateLayout();

        try
        {
            var emptyHost = control.GetVisualDescendants()
                .OfType<ContentControl>()
                .FirstOrDefault(candidate => candidate.Name == "PART_EmptyContentHost");
            Assert.NotNull(emptyHost);
            Assert.True(emptyHost!.IsVisible);
            Assert.Equal("No documents are open.", emptyHost.Content);
            Assert.False(control.HasVisibleDockables);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DocumentControl_Hides_EmptyContent_When_Documents_Exist()
    {
        var factory = new Factory();
        var dock = new DocumentDock
        {
            Factory = factory,
            LayoutMode = DocumentLayoutMode.Tabbed,
            VisibleDockables = factory.CreateList<IDockable>(),
            EmptyContent = "No documents are open."
        };

        var document = new Document
        {
            Id = "Document1",
            Title = "Document 1"
        };
        dock.VisibleDockables!.Add(document);
        dock.ActiveDockable = document;

        var control = new DocumentControl { DataContext = dock };
        var window = new Window
        {
            Width = 800,
            Height = 600,
            Content = control
        };

        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();
        control.UpdateLayout();

        try
        {
            var emptyHost = control.GetVisualDescendants()
                .OfType<ContentControl>()
                .FirstOrDefault(candidate => candidate.Name == "PART_EmptyContentHost");
            Assert.NotNull(emptyHost);
            Assert.False(emptyHost!.IsVisible);
            Assert.True(control.HasVisibleDockables);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DocumentControl_Updates_EmptyContent_Visibility_On_Document_Add_Remove()
    {
        var factory = new Factory();
        var dock = new DocumentDock
        {
            Factory = factory,
            LayoutMode = DocumentLayoutMode.Tabbed,
            VisibleDockables = factory.CreateList<IDockable>(),
            EmptyContent = "No documents are open."
        };

        var control = new DocumentControl { DataContext = dock };
        var window = new Window
        {
            Width = 800,
            Height = 600,
            Content = control
        };

        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();
        control.UpdateLayout();

        try
        {
            var emptyHost = control.GetVisualDescendants()
                .OfType<ContentControl>()
                .FirstOrDefault(candidate => candidate.Name == "PART_EmptyContentHost");
            Assert.NotNull(emptyHost);

            Assert.True(emptyHost!.IsVisible);
            Assert.False(control.HasVisibleDockables);

            var document = new Document
            {
                Id = "Document1",
                Title = "Document 1"
            };
            dock.VisibleDockables!.Add(document);
            dock.ActiveDockable = document;
            window.UpdateLayout();
            control.UpdateLayout();

            Assert.False(emptyHost.IsVisible);
            Assert.True(control.HasVisibleDockables);

            dock.VisibleDockables.Remove(document);
            dock.ActiveDockable = null;
            window.UpdateLayout();
            control.UpdateLayout();

            Assert.True(emptyHost.IsVisible);
            Assert.False(control.HasVisibleDockables);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DocumentControl_Applies_EmptyContentTemplate()
    {
        var factory = new Factory();
        var template = new FuncDataTemplate<string>(
            (content, _) => new TextBlock { Text = $"Template: {content}" },
            true);

        var dock = new DocumentDock
        {
            Factory = factory,
            LayoutMode = DocumentLayoutMode.Tabbed,
            VisibleDockables = factory.CreateList<IDockable>(),
            EmptyContent = "No documents are open."
        };

        var control = new DocumentControl
        {
            DataContext = dock,
            EmptyContentTemplate = template
        };

        var window = new Window
        {
            Width = 800,
            Height = 600,
            Content = control
        };

        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();
        control.UpdateLayout();

        try
        {
            var emptyHost = control.GetVisualDescendants()
                .OfType<ContentControl>()
                .FirstOrDefault(candidate => candidate.Name == "PART_EmptyContentHost");
            Assert.NotNull(emptyHost);
            Assert.Same(template, emptyHost!.ContentTemplate);

            var templateText = emptyHost.GetVisualDescendants()
                .OfType<TextBlock>()
                .FirstOrDefault(candidate => candidate.Text == "Template: No documents are open.");
            Assert.NotNull(templateText);
        }
        finally
        {
            window.Close();
        }
    }
}
