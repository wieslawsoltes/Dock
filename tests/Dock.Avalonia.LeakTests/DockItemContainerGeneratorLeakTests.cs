using System;
using System.Collections.ObjectModel;
using Avalonia.Collections;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class DockItemContainerGeneratorLeakTests
{
    [ReleaseFact]
    public void DocumentDock_ItemContainerGeneratorSwap_DoesNotLeak_PreviousGenerator()
    {
        var result = RunInSession(() =>
        {
            var dock = new DocumentDock
            {
                VisibleDockables = new AvaloniaList<IDockable>(),
                DocumentTemplate = new DocumentTemplate()
            };

            var firstGenerator = new DockItemContainerGenerator();
            dock.ItemContainerGenerator = firstGenerator;
            dock.ItemsSource = new ObservableCollection<string> { "DocA" };

            var generatorRef = new WeakReference(firstGenerator);
            firstGenerator = null;

            dock.ItemContainerGenerator = new DockItemContainerGenerator();
            dock.ItemsSource = null;

            return new GeneratorSwapLeakResult(generatorRef, dock);
        });

        AssertCollected(result.GeneratorRef);
        GC.KeepAlive(result.DockKeepAlive);
    }

    [ReleaseFact]
    public void ToolDock_ItemContainerGeneratorSwap_DoesNotLeak_PreviousGenerator()
    {
        var result = RunInSession(() =>
        {
            var dock = new ToolDock
            {
                VisibleDockables = new AvaloniaList<IDockable>(),
                ToolTemplate = new ToolTemplate()
            };

            var firstGenerator = new DockItemContainerGenerator();
            dock.ItemContainerGenerator = firstGenerator;
            dock.ItemsSource = new ObservableCollection<string> { "ToolA" };

            var generatorRef = new WeakReference(firstGenerator);
            firstGenerator = null;

            dock.ItemContainerGenerator = new DockItemContainerGenerator();
            dock.ItemsSource = null;

            return new GeneratorSwapLeakResult(generatorRef, dock);
        });

        AssertCollected(result.GeneratorRef);
        GC.KeepAlive(result.DockKeepAlive);
    }

    private sealed record GeneratorSwapLeakResult(
        WeakReference GeneratorRef,
        object DockKeepAlive);
}
