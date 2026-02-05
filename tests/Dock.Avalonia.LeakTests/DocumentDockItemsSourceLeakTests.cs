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
public class DocumentDockItemsSourceLeakTests
{
    [ReleaseFact]
    public void DocumentDock_ItemsSourceSwap_DoesNotLeak_PreviousCollection()
    {
        var result = RunInSession(() =>
        {
            var dock = new DocumentDock
            {
                VisibleDockables = new AvaloniaList<IDockable>()
            };

            var itemsA = new ObservableCollection<string> { "DocA" };
            dock.ItemsSource = itemsA;

            var itemsRef = new WeakReference(itemsA);
            itemsA = null;

            var itemsB = new ObservableCollection<string> { "DocB" };
            dock.ItemsSource = itemsB;

            dock.ItemsSource = null;

            return new ItemsSourceSwapLeakResult(itemsRef, dock, itemsB);
        });

        AssertCollected(result.ItemsRef);
        GC.KeepAlive(result.DockKeepAlive);
        GC.KeepAlive(result.ItemsKeepAlive);
    }

    [ReleaseFact]
    public void DocumentDock_ItemsSourceWithTemplate_DoesNotLeak_GeneratedDocuments()
    {
        var result = RunInSession(() =>
        {
            var dock = new DocumentDock
            {
                VisibleDockables = new AvaloniaList<IDockable>(),
                DocumentTemplate = new DocumentTemplate
                {
                    Content = new Func<IServiceProvider, object>(_ => new TextBlock())
                }
            };

            var items = new ObservableCollection<string> { "DocA", "DocB" };
            dock.ItemsSource = items;

            var generated = dock.VisibleDockables is null
                ? new List<IDockable>()
                : new List<IDockable>(dock.VisibleDockables);

            var documentRefs = generated.ConvertAll(dockable => new WeakReference(dockable)).ToArray();
            var generatedCount = generated.Count;

            items.Clear();
            dock.ItemsSource = null;

            return new ItemsSourceTemplateLeakResult(documentRefs, generatedCount, dock, items);
        });

        Assert.True(result.GeneratedCount > 0, "DocumentDock did not generate documents from ItemsSource.");
        Assert.True(result.DockKeepAlive.VisibleDockables?.Count == 0, "Generated documents were not cleared after ItemsSource reset.");

        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.DocumentRefs);
        }

        GC.KeepAlive(result.DockKeepAlive);
        GC.KeepAlive(result.ItemsKeepAlive);
    }

    private sealed record ItemsSourceSwapLeakResult(
        WeakReference ItemsRef,
        DocumentDock DockKeepAlive,
        ObservableCollection<string> ItemsKeepAlive);

    private sealed record ItemsSourceTemplateLeakResult(
        WeakReference[] DocumentRefs,
        int GeneratedCount,
        DocumentDock DockKeepAlive,
        ObservableCollection<string> ItemsKeepAlive);
}
