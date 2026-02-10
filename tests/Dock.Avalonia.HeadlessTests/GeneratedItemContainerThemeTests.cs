using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class GeneratedItemContainerThemeTests
{
    private sealed class TestItem
    {
        public string Title { get; set; } = string.Empty;
    }

    private sealed class ThemeBeforeBaseGenerator : DockItemContainerGenerator
    {
        private readonly object _theme;

        public ThemeBeforeBaseGenerator(object theme)
        {
            _theme = theme;
        }

        public override void PrepareDocumentContainer(IItemsSourceDock dock, IDockable container, object item, int index)
        {
            if (container is IDockItemContainerMetadata metadata)
            {
                metadata.ItemContainerTheme = _theme;
            }

            base.PrepareDocumentContainer(dock, container, item, index);
        }

        public override void PrepareToolContainer(IToolItemsSourceDock dock, IDockable container, object item, int index)
        {
            if (container is IDockItemContainerMetadata metadata)
            {
                metadata.ItemContainerTheme = _theme;
            }

            base.PrepareToolContainer(dock, container, item, index);
        }
    }

    [AvaloniaFact]
    public void DocumentContentControl_AppliesTheme_FromDockThemeKeyMetadata()
    {
        var dock = new DocumentDock
        {
            DocumentTemplate = new DocumentTemplate(),
            DocumentItemContainerTheme = "DocumentGeneratedTheme",
            ItemsSource = new ObservableCollection<TestItem>
            {
                new() { Title = "Doc A" }
            }
        };

        var generated = Assert.IsType<Document>(Assert.Single(dock.VisibleDockables!));
        var expectedTheme = new ControlTheme(typeof(DocumentContentControl));

        var control = new DocumentContentControl
        {
            DataContext = generated
        };
        control.Resources["DocumentGeneratedTheme"] = expectedTheme;

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
            Assert.Same(expectedTheme, control.Theme);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ToolContentControl_AppliesTheme_FromDockThemeKeyMetadata()
    {
        var dock = new ToolDock
        {
            ToolTemplate = new ToolTemplate(),
            ToolItemContainerTheme = "ToolGeneratedTheme",
            ItemsSource = new ObservableCollection<TestItem>
            {
                new() { Title = "Tool A" }
            }
        };

        var generated = Assert.IsType<Tool>(Assert.Single(dock.VisibleDockables!));
        var expectedTheme = new ControlTheme(typeof(ToolContentControl));

        var control = new ToolContentControl
        {
            DataContext = generated
        };
        control.Resources["ToolGeneratedTheme"] = expectedTheme;

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
            Assert.Same(expectedTheme, control.Theme);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DocumentContentControl_DockThemeMetadata_OverridesGeneratorMetadata()
    {
        var dockTheme = new ControlTheme(typeof(DocumentContentControl));
        var generatorTheme = new ControlTheme(typeof(DocumentContentControl));

        var dock = new DocumentDock
        {
            DocumentTemplate = new DocumentTemplate(),
            ItemContainerGenerator = new ThemeBeforeBaseGenerator(generatorTheme),
            DocumentItemContainerTheme = dockTheme,
            ItemsSource = new ObservableCollection<TestItem>
            {
                new() { Title = "Doc A" }
            }
        };

        var generated = Assert.IsType<Document>(Assert.Single(dock.VisibleDockables!));
        var control = new DocumentContentControl { DataContext = generated };
        var window = new Window { Width = 800, Height = 600, Content = control };

        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();
        control.UpdateLayout();

        try
        {
            Assert.Same(dockTheme, control.Theme);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ToolContentControl_DockThemeMetadata_OverridesGeneratorMetadata()
    {
        var dockTheme = new ControlTheme(typeof(ToolContentControl));
        var generatorTheme = new ControlTheme(typeof(ToolContentControl));

        var dock = new ToolDock
        {
            ToolTemplate = new ToolTemplate(),
            ItemContainerGenerator = new ThemeBeforeBaseGenerator(generatorTheme),
            ToolItemContainerTheme = dockTheme,
            ItemsSource = new ObservableCollection<TestItem>
            {
                new() { Title = "Tool A" }
            }
        };

        var generated = Assert.IsType<Tool>(Assert.Single(dock.VisibleDockables!));
        var control = new ToolContentControl { DataContext = generated };
        var window = new Window { Width = 800, Height = 600, Content = control };

        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();
        control.UpdateLayout();

        try
        {
            Assert.Same(dockTheme, control.Theme);
        }
        finally
        {
            window.Close();
        }
    }
}
