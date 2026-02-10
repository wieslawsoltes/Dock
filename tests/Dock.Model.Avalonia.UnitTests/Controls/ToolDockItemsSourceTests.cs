using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests.Controls;

public class ToolDockItemsSourceTests
{
    public class TestToolModel : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        private bool _canClose = true;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public bool CanClose
        {
            get => _canClose;
            set => SetProperty(ref _canClose, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    private sealed class CustomToolTemplate : IToolTemplate
    {
        public object? Content { get; set; }
    }

    private static IList<IDockable> RequireVisibleDockables(ToolDock dock)
    {
        return dock.VisibleDockables ?? throw new InvalidOperationException("VisibleDockables should not be null.");
    }

    [AvaloniaFact]
    public void ItemsSource_WhenSet_CreatesToolsFromItems()
    {
        var dock = new ToolDock
        {
            ToolTemplate = new ToolTemplate()
        };

        var items = new List<TestToolModel>
        {
            new() { Title = "Tool1" },
            new() { Title = "Tool2" }
        };

        dock.ItemsSource = items;

        var visibleDockables = RequireVisibleDockables(dock);
        Assert.Equal(2, visibleDockables.Count);

        var tool1 = Assert.IsType<Tool>(visibleDockables[0]);
        var tool2 = Assert.IsType<Tool>(visibleDockables[1]);

        Assert.Equal("Tool1", tool1.Title);
        Assert.Equal("Tool2", tool2.Title);
        Assert.Same(items[0], tool1.Context);
        Assert.Same(items[1], tool2.Context);
    }

    [AvaloniaFact]
    public void ItemsSource_WhenObservableCollectionItemAdded_CreatesNewTool()
    {
        var dock = new ToolDock
        {
            ToolTemplate = new ToolTemplate()
        };

        var items = new ObservableCollection<TestToolModel>
        {
            new() { Title = "Tool1" }
        };

        dock.ItemsSource = items;
        Assert.Single(RequireVisibleDockables(dock));

        items.Add(new TestToolModel { Title = "Tool2" });

        var visibleDockables = RequireVisibleDockables(dock);
        Assert.Equal(2, visibleDockables.Count);
        var tool = Assert.IsType<Tool>(visibleDockables[1]);
        Assert.Equal("Tool2", tool.Title);
    }

    [AvaloniaFact]
    public void ItemsSource_WhenObservableCollectionItemRemoved_RemovesTool()
    {
        var dock = new ToolDock
        {
            ToolTemplate = new ToolTemplate()
        };

        var item1 = new TestToolModel { Title = "Tool1" };
        var item2 = new TestToolModel { Title = "Tool2" };
        var items = new ObservableCollection<TestToolModel> { item1, item2 };

        dock.ItemsSource = items;
        Assert.Equal(2, RequireVisibleDockables(dock).Count);

        items.Remove(item1);

        var visibleDockables = RequireVisibleDockables(dock);
        Assert.Single(visibleDockables);
        var remaining = Assert.IsType<Tool>(visibleDockables[0]);
        Assert.Equal("Tool2", remaining.Title);
        Assert.Same(item2, remaining.Context);
    }

    [AvaloniaFact]
    public void ItemsSource_WhenObservableCollectionItemReplaced_ReplacesTool()
    {
        var dock = new ToolDock
        {
            ToolTemplate = new ToolTemplate()
        };

        var oldItem = new TestToolModel { Title = "Old" };
        var newItem = new TestToolModel { Title = "New" };
        var items = new ObservableCollection<TestToolModel> { oldItem };

        dock.ItemsSource = items;
        Assert.Single(RequireVisibleDockables(dock));

        items[0] = newItem;

        var visibleDockables = RequireVisibleDockables(dock);
        Assert.Single(visibleDockables);
        var tool = Assert.IsType<Tool>(visibleDockables[0]);
        Assert.Equal("New", tool.Title);
        Assert.Same(newItem, tool.Context);
    }

    [AvaloniaFact]
    public void ItemsSource_WhenObservableCollectionReset_RemovesAllTools()
    {
        var dock = new ToolDock
        {
            ToolTemplate = new ToolTemplate()
        };

        var items = new ObservableCollection<TestToolModel>
        {
            new() { Title = "Tool1" },
            new() { Title = "Tool2" }
        };

        dock.ItemsSource = items;
        Assert.Equal(2, RequireVisibleDockables(dock).Count);

        items.Clear();

        Assert.Empty(RequireVisibleDockables(dock));
    }

    [AvaloniaFact]
    public void ItemsSource_WhenSetToNull_ClearsAllTools()
    {
        var dock = new ToolDock
        {
            ToolTemplate = new ToolTemplate()
        };

        dock.ItemsSource = new List<TestToolModel>
        {
            new() { Title = "Tool1" },
            new() { Title = "Tool2" }
        };

        Assert.Equal(2, RequireVisibleDockables(dock).Count);

        dock.ItemsSource = null;

        Assert.Empty(RequireVisibleDockables(dock));
    }

    [AvaloniaFact]
    public void ItemsSource_WithoutToolTemplate_DoesNotCreateTools()
    {
        var dock = new ToolDock();

        dock.ItemsSource = new List<TestToolModel>
        {
            new() { Title = "Tool1" }
        };

        Assert.True(dock.VisibleDockables == null || dock.VisibleDockables.Count == 0);
    }

    [AvaloniaFact]
    public void ItemsSource_MapsPropertiesCorrectly()
    {
        var dock = new ToolDock
        {
            ToolTemplate = new ToolTemplate()
        };

        var items = new List<TestToolModel>
        {
            new() { Title = "Mapped Tool", CanClose = false }
        };

        dock.ItemsSource = items;

        var visibleDockables = RequireVisibleDockables(dock);
        Assert.Single(visibleDockables);
        var tool = Assert.IsType<Tool>(visibleDockables[0]);
        Assert.Equal("Mapped Tool", tool.Title);
        Assert.False(tool.CanClose);
        Assert.Same(items[0], tool.Context);
    }

    [AvaloniaFact]
    public void ItemsSource_WithNameProperty_UsesNameAsTitle()
    {
        var itemWithName = new { Name = "Named Tool" };
        var dock = new ToolDock
        {
            ToolTemplate = new ToolTemplate()
        };

        dock.ItemsSource = new[] { itemWithName };

        var visibleDockables = RequireVisibleDockables(dock);
        var tool = Assert.IsType<Tool>(visibleDockables[0]);
        Assert.Equal("Named Tool", tool.Title);
    }

    [AvaloniaFact]
    public void ItemsSource_WithoutTitleProperties_UsesToStringAsTitle()
    {
        var item = new FallbackToolModel("Fallback Tool");
        var dock = new ToolDock
        {
            ToolTemplate = new ToolTemplate()
        };

        dock.ItemsSource = new[] { item };

        var visibleDockables = RequireVisibleDockables(dock);
        var tool = Assert.IsType<Tool>(visibleDockables[0]);
        Assert.Equal("Fallback Tool", tool.Title);
    }

    [AvaloniaFact]
    public void ClosingItemsSourceTool_RemovesItemFromSourceCollection()
    {
        var factory = new Factory();
        var dock = new ToolDock
        {
            Factory = factory,
            ToolTemplate = new ToolTemplate()
        };

        var item1 = new TestToolModel { Title = "Tool1", CanClose = true };
        var item2 = new TestToolModel { Title = "Tool2", CanClose = true };
        var items = new ObservableCollection<TestToolModel> { item1, item2 };

        dock.ItemsSource = items;

        var visibleDockables = RequireVisibleDockables(dock);
        Assert.Equal(2, visibleDockables.Count);

        var tool1 = Assert.IsType<Tool>(visibleDockables[0]);

        factory.CloseDockable(tool1);

        visibleDockables = RequireVisibleDockables(dock);
        Assert.Single(visibleDockables);
        Assert.Single(items);

        var remainingTool = Assert.IsType<Tool>(visibleDockables[0]);
        Assert.Same(item2, remainingTool.Context);
        Assert.DoesNotContain(item1, items);
    }

    [AvaloniaFact]
    public void ClosingNonItemsSourceTool_DoesNotAffectSourceCollection()
    {
        var factory = new Factory();
        var dock = new ToolDock
        {
            Factory = factory,
            ToolTemplate = new ToolTemplate()
        };

        var items = new ObservableCollection<TestToolModel>
        {
            new() { Title = "ItemsSource Tool", CanClose = true }
        };

        dock.ItemsSource = items;

        var manualTool = new Tool
        {
            Title = "Manual Tool",
            CanClose = true,
            Content = "Manual Content"
        };

        factory.AddDockable(dock, manualTool);

        var visibleDockables = RequireVisibleDockables(dock);
        Assert.Equal(2, visibleDockables.Count);
        Assert.Single(items);

        factory.CloseDockable(manualTool);

        visibleDockables = RequireVisibleDockables(dock);
        Assert.Single(visibleDockables);
        Assert.Single(items);

        var remainingTool = Assert.IsType<Tool>(visibleDockables[0]);
        Assert.Equal("ItemsSource Tool", remainingTool.Title);
    }

    [AvaloniaFact]
    public void ClosingItemsSourceTool_WhenHideToolsOnCloseTrue_DoesNotMoveToolToHiddenCollection()
    {
        var factory = new Factory
        {
            HideToolsOnClose = true
        };

        var dock = new ToolDock
        {
            Id = "Tools",
            Factory = factory,
            ToolTemplate = new ToolTemplate()
        };

        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>(dock);
        root.ActiveDockable = dock;
        factory.InitLayout(root);

        var item1 = new TestToolModel { Title = "Tool1", CanClose = true };
        var item2 = new TestToolModel { Title = "Tool2", CanClose = true };
        var items = new ObservableCollection<TestToolModel> { item1, item2 };
        dock.ItemsSource = items;

        var tool1 = Assert.IsType<Tool>(RequireVisibleDockables(dock)[0]);
        factory.CloseDockable(tool1);

        Assert.Single(items);
        Assert.Single(RequireVisibleDockables(dock));
        Assert.True(root.HiddenDockables == null || root.HiddenDockables.Count == 0);
    }

    [AvaloniaFact]
    public void RemovingActiveItemsSourceTool_ReassignsActiveDockable()
    {
        var dock = new ToolDock
        {
            ToolTemplate = new ToolTemplate()
        };

        var first = new TestToolModel { Title = "Tool1" };
        var second = new TestToolModel { Title = "Tool2" };
        var items = new ObservableCollection<TestToolModel> { first, second };

        dock.ItemsSource = items;

        var visibleDockables = RequireVisibleDockables(dock);
        var active = Assert.IsType<Tool>(visibleDockables[0]);
        Assert.Same(active, dock.ActiveDockable);

        items.Remove(first);

        visibleDockables = RequireVisibleDockables(dock);
        Assert.Single(visibleDockables);
        Assert.Same(visibleDockables[0], dock.ActiveDockable);

        items.Clear();
        Assert.Empty(visibleDockables);
        Assert.Null(dock.ActiveDockable);
    }

    [AvaloniaFact]
    public void ItemsSource_WithCustomToolTemplate_UsesTemplateContent()
    {
        var templateContent = "custom-template-content";
        var dock = new ToolDock
        {
            ToolTemplate = new CustomToolTemplate
            {
                Content = templateContent
            }
        };

        dock.ItemsSource = new[] { new TestToolModel { Title = "Tool1" } };

        var tool = Assert.IsType<Tool>(RequireVisibleDockables(dock)[0]);
        Assert.Same(templateContent, tool.Content);
    }

    [AvaloniaFact]
    public void ClosingItemsSourceTool_WithFixedSizeList_DoesNotThrowAndUntracksGeneratedTool()
    {
        var factory = new Factory();
        var dock = new ToolDock
        {
            Factory = factory,
            ToolTemplate = new ToolTemplate()
        };

        var source = new[] { new TestToolModel { Title = "Tool1", CanClose = true } };
        dock.ItemsSource = source;

        var tool = Assert.IsType<Tool>(RequireVisibleDockables(dock)[0]);

        var exception = Record.Exception(() => factory.CloseDockable(tool));

        Assert.Null(exception);
        Assert.True(dock.VisibleDockables == null || dock.VisibleDockables.Count == 0);
        Assert.False(dock.IsToolFromItemsSource(tool));
    }

    [AvaloniaFact]
    public void ClosingDuplicateSourceTools_RemovesBothItemsOverTwoCloses()
    {
        var factory = new Factory();
        var dock = new ToolDock
        {
            Factory = factory,
            ToolTemplate = new ToolTemplate()
        };

        var shared = new TestToolModel { Title = "Shared Tool", CanClose = true };
        var source = new ObservableCollection<TestToolModel> { shared, shared };
        dock.ItemsSource = source;

        var first = Assert.IsType<Tool>(RequireVisibleDockables(dock)[0]);
        factory.CloseDockable(first);

        Assert.Single(source);
        Assert.Single(RequireVisibleDockables(dock));

        var second = Assert.IsType<Tool>(RequireVisibleDockables(dock)[0]);
        factory.CloseDockable(second);

        Assert.Empty(source);
        Assert.Empty(RequireVisibleDockables(dock));
    }

    [AvaloniaFact]
    public void ItemsSource_WhenReplacedAndCleared_PreservesManualTools()
    {
        var dock = new ToolDock
        {
            ToolTemplate = new ToolTemplate()
        };

        var manualTool = new Tool
        {
            Title = "Manual",
            CanClose = true
        };
        RequireVisibleDockables(dock).Add(manualTool);
        dock.ActiveDockable = manualTool;

        dock.ItemsSource = new ObservableCollection<TestToolModel>
        {
            new() { Title = "GeneratedA" }
        };

        var visibleDockables = RequireVisibleDockables(dock);
        Assert.Equal(2, visibleDockables.Count);
        Assert.Contains(manualTool, visibleDockables);
        Assert.Single(visibleDockables, d => d is Tool t && t.Title == "GeneratedA");

        dock.ItemsSource = new ObservableCollection<TestToolModel>
        {
            new() { Title = "GeneratedB" },
            new() { Title = "GeneratedC" }
        };

        visibleDockables = RequireVisibleDockables(dock);
        Assert.Equal(3, visibleDockables.Count);
        Assert.Contains(manualTool, visibleDockables);
        Assert.Single(visibleDockables, d => d is Tool t && t.Title == "GeneratedB");
        Assert.Single(visibleDockables, d => d is Tool t && t.Title == "GeneratedC");

        dock.ItemsSource = null;

        visibleDockables = RequireVisibleDockables(dock);
        Assert.Single(visibleDockables);
        Assert.Same(manualTool, visibleDockables[0]);
        Assert.Same(manualTool, dock.ActiveDockable);
    }

    [AvaloniaFact]
    public void ItemsSource_WithValueTypeItems_RemovingItem_RemovesGeneratedTool()
    {
        var dock = new ToolDock
        {
            ToolTemplate = new ToolTemplate()
        };

        var source = new ObservableCollection<int> { 1, 2 };
        dock.ItemsSource = source;

        Assert.Equal(2, RequireVisibleDockables(dock).Count);

        source.Remove(1);

        var visibleDockables = RequireVisibleDockables(dock);
        Assert.Single(visibleDockables);
        var remaining = Assert.IsType<Tool>(visibleDockables[0]);
        Assert.Equal(2, Assert.IsType<int>(remaining.Context));
    }

    [AvaloniaFact]
    public void ClosingValueTypeItemsSourceTool_RemovesValueFromSource()
    {
        var factory = new Factory();
        var dock = new ToolDock
        {
            Factory = factory,
            ToolTemplate = new ToolTemplate()
        };

        var source = new ObservableCollection<int> { 1, 2 };
        dock.ItemsSource = source;

        var closingTool = Assert.IsType<Tool>(RequireVisibleDockables(dock)[0]);

        factory.CloseDockable(closingTool);

        Assert.Single(source);
        Assert.DoesNotContain(1, source);
        Assert.Single(RequireVisibleDockables(dock));
    }

    [AvaloniaFact]
    public void ClosingMovedItemsSourceTool_RemovesItemFromOriginalSource()
    {
        var factory = new Factory();
        var sourceDock = new ToolDock
        {
            Id = "SourceTools",
            Factory = factory,
            ToolTemplate = new ToolTemplate()
        };

        var targetDock = new ToolDock
        {
            Id = "TargetTools",
            Factory = factory,
            ToolTemplate = new ToolTemplate()
        };

        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>(sourceDock, targetDock);
        root.ActiveDockable = sourceDock;
        factory.InitLayout(root);

        var item = new TestToolModel { Title = "MovedTool", CanClose = true };
        var source = new ObservableCollection<TestToolModel> { item };
        sourceDock.ItemsSource = source;

        var generatedTool = Assert.IsType<Tool>(RequireVisibleDockables(sourceDock)[0]);

        factory.MoveDockable(sourceDock, targetDock, generatedTool, null);
        Assert.NotNull(root.VisibleDockables);
        Assert.DoesNotContain(sourceDock, root.VisibleDockables!);
        Assert.Empty(RequireVisibleDockables(sourceDock));
        Assert.Single(RequireVisibleDockables(targetDock));

        factory.CloseDockable(generatedTool);

        Assert.Empty(source);
        Assert.Empty(RequireVisibleDockables(targetDock));
    }

    [AvaloniaFact]
    public void RemovingMovedItemsSourceItem_RemovesToolFromCurrentOwner()
    {
        var factory = new Factory();
        var sourceDock = new ToolDock
        {
            Id = "SourceTools",
            Factory = factory,
            ToolTemplate = new ToolTemplate()
        };

        var targetDock = new ToolDock
        {
            Id = "TargetTools",
            Factory = factory,
            ToolTemplate = new ToolTemplate()
        };

        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>(sourceDock, targetDock);
        root.ActiveDockable = sourceDock;
        factory.InitLayout(root);

        var item = new TestToolModel { Title = "MovedTool", CanClose = true };
        var source = new ObservableCollection<TestToolModel> { item };
        sourceDock.ItemsSource = source;

        var generatedTool = Assert.IsType<Tool>(RequireVisibleDockables(sourceDock)[0]);
        factory.MoveDockable(sourceDock, targetDock, generatedTool, null);
        Assert.NotNull(root.VisibleDockables);
        Assert.DoesNotContain(sourceDock, root.VisibleDockables!);
        Assert.Single(RequireVisibleDockables(targetDock));

        source.Remove(item);

        Assert.Empty(RequireVisibleDockables(sourceDock));
        Assert.Empty(RequireVisibleDockables(targetDock));
    }

    [AvaloniaFact]
    public void SettingToolTemplateAfterItemsSource_GeneratesTools()
    {
        var dock = new ToolDock();
        var source = new ObservableCollection<TestToolModel>
        {
            new() { Title = "DeferredTemplateTool" }
        };

        dock.ItemsSource = source;
        Assert.Empty(RequireVisibleDockables(dock));

        dock.ToolTemplate = new ToolTemplate();

        var visibleDockables = RequireVisibleDockables(dock);
        Assert.Single(visibleDockables);
        var tool = Assert.IsType<Tool>(visibleDockables[0]);
        Assert.Equal("DeferredTemplateTool", tool.Title);
    }

    private sealed class FallbackToolModel
    {
        private readonly string _name;

        public FallbackToolModel(string name)
        {
            _name = name;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
