using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Collections;
using Avalonia.Controls;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class ToolDockItemsSourceLeakTests
{
    [ReleaseFact]
    public void ToolDock_ItemsSourceSwap_DoesNotLeak_PreviousCollection()
    {
        var result = RunInSession(() =>
        {
            var dock = new ToolDock
            {
                VisibleDockables = new AvaloniaList<IDockable>(),
                ToolTemplate = new ToolTemplate()
            };

            var itemsA = new ObservableCollection<string> { "ToolA" };
            dock.ItemsSource = itemsA;

            var itemsRef = new WeakReference(itemsA);
            itemsA = null;

            var itemsB = new ObservableCollection<string> { "ToolB" };
            dock.ItemsSource = itemsB;

            dock.ItemsSource = null;

            return new ItemsSourceSwapLeakResult(itemsRef, dock, itemsB);
        });

        AssertCollected(result.ItemsRef);
        GC.KeepAlive(result.DockKeepAlive);
        GC.KeepAlive(result.ItemsKeepAlive);
    }

    [ReleaseFact]
    public void ToolDock_ItemsSourceWithTemplate_DoesNotLeak_GeneratedTools()
    {
        var result = RunInSession(() =>
        {
            var dock = new ToolDock
            {
                VisibleDockables = new AvaloniaList<IDockable>(),
                ToolTemplate = new ToolTemplate
                {
                    Content = new Func<IServiceProvider, object>(_ => new TextBlock())
                }
            };

            var items = new ObservableCollection<string> { "ToolA", "ToolB" };
            dock.ItemsSource = items;

            var generated = dock.VisibleDockables is null
                ? new List<IDockable>()
                : new List<IDockable>(dock.VisibleDockables);

            var toolRefs = generated.ConvertAll(dockable => new WeakReference(dockable)).ToArray();
            var generatedCount = generated.Count;

            items.Clear();
            dock.ItemsSource = null;

            return new ItemsSourceTemplateLeakResult(toolRefs, generatedCount, dock, items);
        });

        Assert.True(result.GeneratedCount > 0, "ToolDock did not generate tools from ItemsSource.");
        Assert.True(result.DockKeepAlive.VisibleDockables?.Count == 0, "Generated tools were not cleared after ItemsSource reset.");

        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.ToolRefs);
        }

        GC.KeepAlive(result.DockKeepAlive);
        GC.KeepAlive(result.ItemsKeepAlive);
    }

    private sealed record ItemsSourceSwapLeakResult(
        WeakReference ItemsRef,
        ToolDock DockKeepAlive,
        ObservableCollection<string> ItemsKeepAlive);

    private sealed record ItemsSourceTemplateLeakResult(
        WeakReference[] ToolRefs,
        int GeneratedCount,
        ToolDock DockKeepAlive,
        ObservableCollection<string> ItemsKeepAlive);
}
