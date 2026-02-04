using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Recycling;
using Avalonia.VisualTree;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class AvaloniaViewLeakTests
{
    [ReleaseFact]
    public void AvaloniaModel_WithViews_InputInteractions_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var factory = new Factory();
            var document = new Document { Factory = factory };
            var tool = new Tool { Factory = factory };

            var documentView = new Border
            {
                Child = new TextBlock { Text = "Document View" }
            };
            var toolView = new Border
            {
                Child = new TextBlock { Text = "Tool View" }
            };

            document.Content = documentView;
            tool.Content = toolView;

            var documentHost = new ContentControl { Content = document };
            documentHost.ContentTemplate = new ControlRecyclingDataTemplate { Parent = documentHost };
            var toolHost = new ContentControl { Content = tool };
            toolHost.ContentTemplate = new ControlRecyclingDataTemplate { Parent = toolHost };

            var panel = new StackPanel();
            panel.Children.Add(documentHost);
            panel.Children.Add(toolHost);

            var window = new Window { Content = panel };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);

            var hasDocumentView = panel.GetVisualDescendants().Any(visual => ReferenceEquals(visual, documentView));
            var hasToolView = panel.GetVisualDescendants().Any(visual => ReferenceEquals(visual, toolView));

            Assert.True(hasDocumentView);
            Assert.True(hasToolView);

            ExerciseInputInteractions(documentHost, includeDoubleTap: true, includeMiddlePress: true);

            var leakResult = new AvaloniaViewLeakResult(
                new WeakReference(document),
                new WeakReference(tool),
                new WeakReference(documentHost),
                new WeakReference(toolHost),
                new WeakReference(documentView),
                new WeakReference(toolView));

            CleanupWindow(window);
            return leakResult;
        });

        AssertCollected(
            result.DocumentRef,
            result.ToolRef,
            result.DocumentHostRef,
            result.ToolHostRef,
            result.DocumentViewRef,
            result.ToolViewRef);
    }

    private sealed record AvaloniaViewLeakResult(
        WeakReference DocumentRef,
        WeakReference ToolRef,
        WeakReference DocumentHostRef,
        WeakReference ToolHostRef,
        WeakReference DocumentViewRef,
        WeakReference ToolViewRef);
}
