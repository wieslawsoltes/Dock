using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Themes;

namespace PackageConsumer;

internal static class UpdateStrategyProbe
{
    internal static void Verify()
    {
        DockingManager manager = new();
        List<string> callbacks = new();
        Strategy replacement = new()
        {
            After = content =>
            {
                if (manager.GetLayoutItemFromModel(content) is not null)
                    throw new Exception("A packaged source container was registered before its callback finished.");
                callbacks.Add("replacement:" + content.Content);
            }
        };
        manager.LayoutUpdateStrategy = new Strategy
        {
            Before = _ => { manager.LayoutUpdateStrategy = replacement; return false; },
            After = _ => throw new Exception("A replaced strategy received the packaged after callback.")
        };
        manager.DocumentsSource = new[] { "first", "second" };
        if (callbacks.Count != 2 || callbacks[0] != "replacement:first" || callbacks[1] != "replacement:second")
            throw new Exception("The packaged source import did not use the current strategy throughout the batch.");
        if (manager.Layout.Descendents().OfType<LayoutDocument>().Any(content => manager.GetLayoutItemFromModel(content) is null))
            throw new Exception("The packaged source containers were not registered after their callbacks.");

        LayoutAnchorable tool = new() { ContentId = "tool" };
        LayoutAnchorablePane original = new(tool);
        LayoutAnchorablePane target = new(new LayoutAnchorable { ContentId = "target-tool" });
        manager.Layout.RootPanel.Children.Add(original);
        manager.Layout.RootPanel.Children.Add(target);
        tool.Hide();
        bool restored = false;
        manager.LayoutUpdateStrategy = new Strategy
        {
            Before = content =>
            {
                content.PreviousContainer = target;
                content.PreviousContainerIndex = 0;
                return false;
            },
            After = content =>
            {
                restored = ReferenceEquals(content.Parent, target) && ReferenceEquals(content.DockModel.Owner, target.DockModel)
                    && ReferenceEquals(target.Children[0], tool) && !tool.IsHidden;
            }
        };
        tool.Show();
        if (!restored || original.Children.Count != 0 || !tool.IsActive)
            throw new Exception("The packaged Show operation did not honor the strategy's return pane and index.");

        VerifyRenderingDuringInsertion();
    }

    private static void VerifyRenderingDuringInsertion()
    {
        DockingManager manager = new();
        TextBox editor = new() { Text = "Container registered after callback" };
        Window window = new() { Width = 640, Height = 480 };
        window.Styles.Add(new AvalonDockTheme());
        window.Content = manager;
        try
        {
            window.Show();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            manager.LayoutUpdateStrategy = new Strategy
            {
                After = content =>
                {
                    content.IsActive = true;
                    window.UpdateLayout();
                    Dispatcher.UIThread.RunJobs();
                    if (manager.GetLayoutItemFromModel(content) is not null)
                        throw new Exception("Rendering prematurely created a packaged source container.");
                }
            };
            manager.DocumentsSource = new[] { editor };
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            if (!manager.GetVisualDescendants().Contains(editor))
                throw new Exception("The packaged content did not update after container registration.");
            LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            Button close = manager.GetVisualDescendants().OfType<Button>().First(button =>
                button.Classes.Contains("avalondock-close") && button.IsEffectivelyVisible);
            if (!ReferenceEquals(close.Command, manager.GetLayoutItemFromModel(document)!.CloseCommand) ||
                close.Command?.CanExecute(close.CommandParameter) != true)
                throw new Exception("The packaged close button missed deferred container registration.");
            close.Command.Execute(close.CommandParameter);
            Dispatcher.UIThread.RunJobs();
            if (manager.Layout.Descendents().OfType<LayoutDocument>().Any())
                throw new Exception("The packaged close command did not remove its source document.");
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    private sealed class Strategy : ILayoutUpdateStrategy
    {
        internal Func<LayoutContent, bool>? Before { get; init; }
        internal Action<LayoutContent>? After { get; init; }
        public bool BeforeInsertDocument(LayoutRoot root, LayoutDocument document, ILayoutContainer? destination) => Before?.Invoke(document) ?? false;
        public bool BeforeInsertAnchorable(LayoutRoot root, LayoutAnchorable tool, ILayoutContainer? destination) => Before?.Invoke(tool) ?? false;
        public void AfterInsertDocument(LayoutRoot root, LayoutDocument document) => After?.Invoke(document);
        public void AfterInsertAnchorable(LayoutRoot root, LayoutAnchorable tool) => After?.Invoke(tool);
    }
}
