using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests.Controls;

public class DockItemContainerGeneratorTests
{
    private sealed class TestItem
    {
        public string Title { get; set; } = string.Empty;

        public bool CanClose { get; set; } = true;
    }

    private sealed class GeneratedDocument : Document
    {
        public bool IsPrepared { get; set; }
    }

    private sealed class GeneratedTool : Tool
    {
        public bool IsPrepared { get; set; }
    }

    private sealed class TrackingContainerGenerator : IDockItemContainerGenerator
    {
        public int PreparedDocuments { get; private set; }
        public int ClearedDocuments { get; private set; }
        public int PreparedTools { get; private set; }
        public int ClearedTools { get; private set; }

        public IDockable? CreateDocumentContainer(IItemsSourceDock dock, object item, int index)
        {
            return new GeneratedDocument
            {
                Id = $"GeneratedDocument-{index}"
            };
        }

        public void PrepareDocumentContainer(IItemsSourceDock dock, IDockable container, object item, int index)
        {
            PreparedDocuments++;

            var source = Assert.IsType<TestItem>(item);
            var document = Assert.IsType<GeneratedDocument>(container);
            document.Title = $"Generated {source.Title}";
            document.Context = source;
            document.CanClose = source.CanClose;
            document.IsPrepared = true;

            if (document is IDocumentContent content)
            {
                content.Content = new Func<IServiceProvider, object>(_ => new TextBlock
                {
                    Text = $"Doc:{document.Title}",
                    DataContext = source
                });
            }
        }

        public void ClearDocumentContainer(IItemsSourceDock dock, IDockable container, object? item)
        {
            ClearedDocuments++;
            container.Context = null;

            if (container is IDocumentContent content)
            {
                content.Content = null;
            }
        }

        public IDockable? CreateToolContainer(IToolItemsSourceDock dock, object item, int index)
        {
            return new GeneratedTool
            {
                Id = $"GeneratedTool-{index}"
            };
        }

        public void PrepareToolContainer(IToolItemsSourceDock dock, IDockable container, object item, int index)
        {
            PreparedTools++;

            var source = Assert.IsType<TestItem>(item);
            var tool = Assert.IsType<GeneratedTool>(container);
            tool.Title = $"Generated {source.Title}";
            tool.Context = source;
            tool.CanClose = source.CanClose;
            tool.IsPrepared = true;

            if (tool is IToolContent content)
            {
                content.Content = new Func<IServiceProvider, object>(_ => new TextBlock
                {
                    Text = $"Tool:{tool.Title}",
                    DataContext = source
                });
            }
        }

        public void ClearToolContainer(IToolItemsSourceDock dock, IDockable container, object? item)
        {
            ClearedTools++;
            container.Context = null;

            if (container is IToolContent content)
            {
                content.Content = null;
            }
        }
    }

    private sealed class InvalidContainerGenerator : IDockItemContainerGenerator
    {
        public int ClearedDocuments { get; private set; }
        public int ClearedTools { get; private set; }

        public IDockable? CreateDocumentContainer(IItemsSourceDock dock, object item, int index)
        {
            return new DockDock();
        }

        public void PrepareDocumentContainer(IItemsSourceDock dock, IDockable container, object item, int index)
        {
        }

        public void ClearDocumentContainer(IItemsSourceDock dock, IDockable container, object? item)
        {
            ClearedDocuments++;
        }

        public IDockable? CreateToolContainer(IToolItemsSourceDock dock, object item, int index)
        {
            return new Document();
        }

        public void PrepareToolContainer(IToolItemsSourceDock dock, IDockable container, object item, int index)
        {
        }

        public void ClearToolContainer(IToolItemsSourceDock dock, IDockable container, object? item)
        {
            ClearedTools++;
        }
    }

    private static IList<IDockable> RequireVisibleDockables(DocumentDock dock)
    {
        return dock.VisibleDockables ?? throw new InvalidOperationException("VisibleDockables should not be null.");
    }

    private static IList<IDockable> RequireVisibleDockables(ToolDock dock)
    {
        return dock.VisibleDockables ?? throw new InvalidOperationException("VisibleDockables should not be null.");
    }

    [AvaloniaFact]
    public void DocumentDock_CustomGenerator_CreatesCustomContainerAndContentBinding()
    {
        var generator = new TrackingContainerGenerator();
        var sourceItem = new TestItem { Title = "Doc A", CanClose = false };

        var dock = new DocumentDock
        {
            ItemContainerGenerator = generator,
            ItemsSource = new ObservableCollection<TestItem> { sourceItem }
        };

        var generated = Assert.IsType<GeneratedDocument>(RequireVisibleDockables(dock)[0]);
        Assert.True(generated.IsPrepared);
        Assert.Equal("Generated Doc A", generated.Title);
        Assert.False(generated.CanClose);
        Assert.Same(sourceItem, generated.Context);
        Assert.Equal(1, generator.PreparedDocuments);

        var contentFactory = Assert.IsType<Func<IServiceProvider, object>>(generated.Content);
        var content = Assert.IsType<TextBlock>(contentFactory(null!));
        Assert.Equal("Doc:Generated Doc A", content.Text);
        Assert.Same(sourceItem, content.DataContext);
    }

    [AvaloniaFact]
    public void ToolDock_CustomGenerator_CreatesCustomContainerAndContentBinding()
    {
        var generator = new TrackingContainerGenerator();
        var sourceItem = new TestItem { Title = "Tool A", CanClose = false };

        var dock = new ToolDock
        {
            ItemContainerGenerator = generator,
            ItemsSource = new ObservableCollection<TestItem> { sourceItem }
        };

        var generated = Assert.IsType<GeneratedTool>(RequireVisibleDockables(dock)[0]);
        Assert.True(generated.IsPrepared);
        Assert.Equal("Generated Tool A", generated.Title);
        Assert.False(generated.CanClose);
        Assert.Same(sourceItem, generated.Context);
        Assert.Equal(1, generator.PreparedTools);

        var contentFactory = Assert.IsType<Func<IServiceProvider, object>>(generated.Content);
        var content = Assert.IsType<TextBlock>(contentFactory(null!));
        Assert.Equal("Tool:Generated Tool A", content.Text);
        Assert.Same(sourceItem, content.DataContext);
    }

    [AvaloniaFact]
    public void DocumentDock_CustomGenerator_Close_RemovesSourceAndClearsContainer()
    {
        var factory = new Factory();
        var generator = new TrackingContainerGenerator();
        var items = new ObservableCollection<TestItem> { new() { Title = "Doc A" } };

        var dock = new DocumentDock
        {
            Factory = factory,
            ItemContainerGenerator = generator,
            ItemsSource = items
        };

        var generated = Assert.IsType<GeneratedDocument>(RequireVisibleDockables(dock)[0]);

        factory.CloseDockable(generated);

        Assert.Empty(items);
        Assert.Empty(RequireVisibleDockables(dock));
        Assert.Equal(1, generator.ClearedDocuments);
        Assert.Null(generated.Context);
    }

    [AvaloniaFact]
    public void ToolDock_CustomGenerator_Close_RemovesSourceAndClearsContainer()
    {
        var factory = new Factory();
        var generator = new TrackingContainerGenerator();
        var items = new ObservableCollection<TestItem> { new() { Title = "Tool A" } };

        var dock = new ToolDock
        {
            Factory = factory,
            ItemContainerGenerator = generator,
            ItemsSource = items
        };

        var generated = Assert.IsType<GeneratedTool>(RequireVisibleDockables(dock)[0]);

        factory.CloseDockable(generated);

        Assert.Empty(items);
        Assert.Empty(RequireVisibleDockables(dock));
        Assert.Equal(1, generator.ClearedTools);
        Assert.Null(generated.Context);
    }

    [AvaloniaFact]
    public void ChangingItemContainerGenerator_RegeneratesDocumentContainers()
    {
        var firstGenerator = new TrackingContainerGenerator();
        var secondGenerator = new TrackingContainerGenerator();
        var items = new ObservableCollection<TestItem> { new() { Title = "Doc A" } };

        var dock = new DocumentDock
        {
            ItemContainerGenerator = firstGenerator,
            ItemsSource = items
        };

        var firstDocument = Assert.IsType<GeneratedDocument>(RequireVisibleDockables(dock)[0]);
        dock.ItemContainerGenerator = secondGenerator;

        var secondDocument = Assert.IsType<GeneratedDocument>(RequireVisibleDockables(dock)[0]);
        Assert.NotSame(firstDocument, secondDocument);
        Assert.Equal(1, firstGenerator.ClearedDocuments);
        Assert.Equal(1, secondGenerator.PreparedDocuments);
    }

    [AvaloniaFact]
    public void ChangingItemContainerGenerator_RegeneratesToolContainers()
    {
        var firstGenerator = new TrackingContainerGenerator();
        var secondGenerator = new TrackingContainerGenerator();
        var items = new ObservableCollection<TestItem> { new() { Title = "Tool A" } };

        var dock = new ToolDock
        {
            ItemContainerGenerator = firstGenerator,
            ItemsSource = items
        };

        var firstTool = Assert.IsType<GeneratedTool>(RequireVisibleDockables(dock)[0]);
        dock.ItemContainerGenerator = secondGenerator;

        var secondTool = Assert.IsType<GeneratedTool>(RequireVisibleDockables(dock)[0]);
        Assert.NotSame(firstTool, secondTool);
        Assert.Equal(1, firstGenerator.ClearedTools);
        Assert.Equal(1, secondGenerator.PreparedTools);
    }

    [AvaloniaFact]
    public void DocumentDock_CustomGenerator_TracksAddRemoveReplaceReset()
    {
        var generator = new TrackingContainerGenerator();
        var items = new ObservableCollection<TestItem>
        {
            new() { Title = "Doc A" }
        };

        var dock = new DocumentDock
        {
            ItemContainerGenerator = generator,
            ItemsSource = items
        };

        Assert.Single(RequireVisibleDockables(dock));
        Assert.Equal(1, generator.PreparedDocuments);
        Assert.Equal(0, generator.ClearedDocuments);

        items.Add(new TestItem { Title = "Doc B" });
        Assert.Equal(2, RequireVisibleDockables(dock).Count);
        Assert.Equal(2, generator.PreparedDocuments);

        items[0] = new TestItem { Title = "Doc C" };
        Assert.Equal(2, RequireVisibleDockables(dock).Count);
        Assert.Equal(3, generator.PreparedDocuments);
        Assert.Equal(1, generator.ClearedDocuments);

        items.RemoveAt(1);
        Assert.Single(RequireVisibleDockables(dock));
        Assert.Equal(2, generator.ClearedDocuments);

        items.Clear();
        Assert.Empty(RequireVisibleDockables(dock));
        Assert.Equal(3, generator.ClearedDocuments);
    }

    [AvaloniaFact]
    public void ToolDock_CustomGenerator_TracksAddRemoveReplaceReset()
    {
        var generator = new TrackingContainerGenerator();
        var items = new ObservableCollection<TestItem>
        {
            new() { Title = "Tool A" }
        };

        var dock = new ToolDock
        {
            ItemContainerGenerator = generator,
            ItemsSource = items
        };

        Assert.Single(RequireVisibleDockables(dock));
        Assert.Equal(1, generator.PreparedTools);
        Assert.Equal(0, generator.ClearedTools);

        items.Add(new TestItem { Title = "Tool B" });
        Assert.Equal(2, RequireVisibleDockables(dock).Count);
        Assert.Equal(2, generator.PreparedTools);

        items[0] = new TestItem { Title = "Tool C" };
        Assert.Equal(2, RequireVisibleDockables(dock).Count);
        Assert.Equal(3, generator.PreparedTools);
        Assert.Equal(1, generator.ClearedTools);

        items.RemoveAt(1);
        Assert.Single(RequireVisibleDockables(dock));
        Assert.Equal(2, generator.ClearedTools);

        items.Clear();
        Assert.Empty(RequireVisibleDockables(dock));
        Assert.Equal(3, generator.ClearedTools);
    }

    [AvaloniaFact]
    public void DocumentDock_CustomGenerator_RespectsManualDockablesOnItemsSourceSwap()
    {
        var generator = new TrackingContainerGenerator();
        var dock = new DocumentDock
        {
            ItemContainerGenerator = generator
        };

        var manual = new Document { Title = "Manual" };
        RequireVisibleDockables(dock).Add(manual);
        dock.ActiveDockable = manual;

        dock.ItemsSource = new ObservableCollection<TestItem>
        {
            new() { Title = "Doc A" }
        };

        Assert.Equal(2, RequireVisibleDockables(dock).Count);
        Assert.Contains(manual, RequireVisibleDockables(dock));

        dock.ItemsSource = null;
        Assert.Single(RequireVisibleDockables(dock));
        Assert.Same(manual, RequireVisibleDockables(dock)[0]);
        Assert.Same(manual, dock.ActiveDockable);
    }

    [AvaloniaFact]
    public void ToolDock_CustomGenerator_RespectsManualDockablesOnItemsSourceSwap()
    {
        var generator = new TrackingContainerGenerator();
        var dock = new ToolDock
        {
            ItemContainerGenerator = generator
        };

        var manual = new Tool { Title = "Manual" };
        RequireVisibleDockables(dock).Add(manual);
        dock.ActiveDockable = manual;

        dock.ItemsSource = new ObservableCollection<TestItem>
        {
            new() { Title = "Tool A" }
        };

        Assert.Equal(2, RequireVisibleDockables(dock).Count);
        Assert.Contains(manual, RequireVisibleDockables(dock));

        dock.ItemsSource = null;
        Assert.Single(RequireVisibleDockables(dock));
        Assert.Same(manual, RequireVisibleDockables(dock)[0]);
        Assert.Same(manual, dock.ActiveDockable);
    }
    [AvaloniaFact]
    public void DocumentDock_InvalidContainerType_IsSkippedAndCleared()
    {
        var generator = new InvalidContainerGenerator();
        var dock = new DocumentDock
        {
            ItemContainerGenerator = generator,
            ItemsSource = new ObservableCollection<TestItem>
            {
                new() { Title = "Invalid" }
            }
        };

        Assert.Empty(RequireVisibleDockables(dock));
        Assert.Equal(1, generator.ClearedDocuments);
    }

    [AvaloniaFact]
    public void ToolDock_InvalidContainerType_IsSkippedAndCleared()
    {
        var generator = new InvalidContainerGenerator();
        var dock = new ToolDock
        {
            ItemContainerGenerator = generator,
            ItemsSource = new ObservableCollection<TestItem>
            {
                new() { Title = "Invalid" }
            }
        };

        Assert.Empty(RequireVisibleDockables(dock));
        Assert.Equal(1, generator.ClearedTools);
    }
}
