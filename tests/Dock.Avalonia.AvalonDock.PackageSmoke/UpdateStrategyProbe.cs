using System;
using System.Collections.Generic;
using AvalonDock;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class UpdateStrategyProbe
{
    internal static void Verify()
    {
        DockingManager manager = new();
        List<string> callbacks = new();
        Strategy replacement = new() { After = content => callbacks.Add("replacement:" + content.Content) };
        manager.LayoutUpdateStrategy = new Strategy
        {
            Before = _ => { manager.LayoutUpdateStrategy = replacement; return false; },
            After = _ => throw new Exception("A replaced strategy received the packaged after callback.")
        };
        manager.DocumentsSource = new[] { "first", "second" };
        if (callbacks.Count != 2 || callbacks[0] != "replacement:first" || callbacks[1] != "replacement:second")
            throw new Exception("The packaged source import did not use the current strategy throughout the batch.");

        LayoutAnchorable tool = new();
        LayoutAnchorablePane original = new(tool);
        LayoutAnchorablePane target = new(new LayoutAnchorable());
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
