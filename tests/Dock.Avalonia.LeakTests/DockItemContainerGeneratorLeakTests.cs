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
    private sealed class NoOpDocumentSelector : IDocumentItemTemplateSelector
    {
        public object? SelectTemplate(IItemsSourceDock dock, object item, int index)
        {
            return null;
        }
    }

    private sealed class NoOpToolSelector : IToolItemTemplateSelector
    {
        public object? SelectTemplate(IToolItemsSourceDock dock, object item, int index)
        {
            return null;
        }
    }

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

    [ReleaseFact]
    public void DocumentDock_DocumentTemplateSwap_DoesNotLeak_PreviousTemplate()
    {
        var result = RunInSession(() =>
        {
            var dock = new DocumentDock
            {
                VisibleDockables = new AvaloniaList<IDockable>(),
                DocumentTemplate = new DocumentTemplate()
            };

            dock.ItemsSource = new ObservableCollection<string> { "DocA" };
            var firstTemplate = dock.DocumentTemplate;
            var templateRef = new WeakReference(firstTemplate);
            firstTemplate = null;

            dock.DocumentTemplate = new DocumentTemplate();
            dock.ItemsSource = null;

            return new GeneratorSwapLeakResult(templateRef, dock);
        });

        AssertCollected(result.GeneratorRef);
        GC.KeepAlive(result.DockKeepAlive);
    }

    [ReleaseFact]
    public void DocumentDock_TemplateSelectorSwap_DoesNotLeak_PreviousSelector()
    {
        var result = RunInSession(() =>
        {
            var dock = new DocumentDock
            {
                VisibleDockables = new AvaloniaList<IDockable>(),
                DocumentTemplate = new DocumentTemplate()
            };

            dock.ItemsSource = new ObservableCollection<string> { "DocA" };
            var firstSelector = new NoOpDocumentSelector();
            dock.DocumentItemTemplateSelector = firstSelector;
            var selectorRef = new WeakReference(firstSelector);
            firstSelector = null;

            dock.DocumentItemTemplateSelector = new NoOpDocumentSelector();
            dock.ItemsSource = null;

            return new GeneratorSwapLeakResult(selectorRef, dock);
        });

        AssertCollected(result.GeneratorRef);
        GC.KeepAlive(result.DockKeepAlive);
    }

    [ReleaseFact]
    public void ToolDock_TemplateSelectorSwap_DoesNotLeak_PreviousSelector()
    {
        var result = RunInSession(() =>
        {
            var dock = new ToolDock
            {
                VisibleDockables = new AvaloniaList<IDockable>(),
                ToolTemplate = new ToolTemplate()
            };

            dock.ItemsSource = new ObservableCollection<string> { "ToolA" };
            var firstSelector = new NoOpToolSelector();
            dock.ToolItemTemplateSelector = firstSelector;
            var selectorRef = new WeakReference(firstSelector);
            firstSelector = null;

            dock.ToolItemTemplateSelector = new NoOpToolSelector();
            dock.ItemsSource = null;

            return new GeneratorSwapLeakResult(selectorRef, dock);
        });

        AssertCollected(result.GeneratorRef);
        GC.KeepAlive(result.DockKeepAlive);
    }

    [ReleaseFact]
    public void DocumentDock_ContainerThemeSwap_DoesNotLeak_PreviousThemeObject()
    {
        var result = RunInSession(() =>
        {
            var dock = new DocumentDock
            {
                VisibleDockables = new AvaloniaList<IDockable>(),
                DocumentTemplate = new DocumentTemplate()
            };

            dock.ItemsSource = new ObservableCollection<string> { "DocA" };
            var firstTheme = new object();
            dock.DocumentItemContainerTheme = firstTheme;
            var themeRef = new WeakReference(firstTheme);
            firstTheme = null;

            dock.DocumentItemContainerTheme = new object();
            dock.ItemsSource = null;

            return new GeneratorSwapLeakResult(themeRef, dock);
        });

        AssertCollected(result.GeneratorRef);
        GC.KeepAlive(result.DockKeepAlive);
    }

    [ReleaseFact]
    public void ToolDock_ContainerThemeSwap_DoesNotLeak_PreviousThemeObject()
    {
        var result = RunInSession(() =>
        {
            var dock = new ToolDock
            {
                VisibleDockables = new AvaloniaList<IDockable>(),
                ToolTemplate = new ToolTemplate()
            };

            dock.ItemsSource = new ObservableCollection<string> { "ToolA" };
            var firstTheme = new object();
            dock.ToolItemContainerTheme = firstTheme;
            var themeRef = new WeakReference(firstTheme);
            firstTheme = null;

            dock.ToolItemContainerTheme = new object();
            dock.ItemsSource = null;

            return new GeneratorSwapLeakResult(themeRef, dock);
        });

        AssertCollected(result.GeneratorRef);
        GC.KeepAlive(result.DockKeepAlive);
    }

    private sealed record GeneratorSwapLeakResult(
        WeakReference GeneratorRef,
        object DockKeepAlive);
}
