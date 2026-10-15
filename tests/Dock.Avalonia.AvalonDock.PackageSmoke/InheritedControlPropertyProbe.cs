using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class InheritedControlPropertyProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        int checks = 0;
        foreach (string kind in new[] { "document", "tool", "group" })
        {
            Check($"taskbar default {kind}", () =>
            {
                LayoutFloatingWindowControl host = CreateHost(kind);
                try
                {
                    Require(!host.ShowInTaskbar);
                    host.ShowInTaskbar = true;
                    host.ClearValue(Window.ShowInTaskbarProperty);
                    Require(!host.ShowInTaskbar);
                }
                finally { host.Close(); }
            });
            Check($"taskbar style release {kind}", () =>
            {
                LayoutFloatingWindowControl host = CreateHost(kind);
                try
                {
                    host.Theme = new ControlTheme(typeof(LayoutFloatingWindowControl));
                    Style style = new() { Setters = { new Setter(Window.ShowInTaskbarProperty, true) } };
                    host.Styles.Add(style);
                    host.ApplyStyling();
                    Require(host.ShowInTaskbar);
                    host.Styles.Remove(style);
                    Require(!host.ShowInTaskbar);
                }
                finally { host.Close(); }
            });
        }
        Check("unstyled splitter", () =>
        {
            LayoutGridResizerControl splitter = new();
            Require(ReferenceEquals(splitter.Background, Brushes.Transparent));
            splitter.Background = Brushes.Red;
            splitter.ClearValue(TemplatedControl.BackgroundProperty);
            Require(ReferenceEquals(splitter.Background, Brushes.Transparent));
        });
        Check("splitter theme release", () =>
        {
            LayoutGridResizerControl splitter = new() { Theme = new ControlTheme(typeof(LayoutGridResizerControl)) };
            Style style = new() { Setters = { new Setter(TemplatedControl.BackgroundProperty, Brushes.Red) } };
            splitter.Styles.Add(style);
            splitter.ApplyStyling();
            Require(ReferenceEquals(splitter.Background, Brushes.Red));
            splitter.Styles.Remove(style);
            Require(ReferenceEquals(splitter.Background, Brushes.Transparent));
        });
        Check("auto-hide inherited defaults", () =>
        {
            DockingManager manager = new();
            manager.OnApplyTemplate();
            LayoutAutoHideWindowControl preview = manager.AutoHideWindow!;
            preview.Focusable = false;
            preview.ClearValue(LayoutAutoHideWindowControl.FocusableProperty);
            preview.ClearValue(LayoutAutoHideWindowControl.IsVisibleProperty);
            Require(preview.Focusable && preview.IsTabStop && !preview.IsVisible);
        });
        Check("auto-hide focus style", () =>
        {
            DockingManager manager = new();
            manager.OnApplyTemplate();
            LayoutAutoHideWindowControl preview = manager.AutoHideWindow!;
            Style style = new() { Setters = { new Setter(LayoutAutoHideWindowControl.FocusableProperty, false) } };
            preview.Styles.Add(style);
            preview.ApplyStyling();
            Require(!preview.Focusable);
            preview.Styles.Remove(style);
            Require(preview.Focusable);
        });
        foreach (bool activation in new[] { false, true })
            Check($"overlay activation/taskbar {activation}", () =>
            {
                OverlayHost overlay = new();
                try
                {
                    StyledProperty<bool> property = activation ? Window.ShowActivatedProperty : Window.ShowInTaskbarProperty;
                    Require(!overlay.GetValue(property));
                    overlay.SetValue(property, true);
                    overlay.ClearValue(property);
                    Require(!overlay.GetValue(property));
                }
                finally { overlay.Close(); }
            });
        Check("overlay decorations", () =>
        {
            OverlayHost overlay = new();
            try
            {
                Require(overlay.WindowDecorations == WindowDecorations.None);
                overlay.Theme = new ControlTheme(typeof(OverlayWindow))
                { Setters = { new Setter(Window.WindowDecorationsProperty, WindowDecorations.Full) } };
                overlay.ApplyStyling();
                Require(overlay.WindowDecorations == WindowDecorations.Full);
                overlay.Theme = new ControlTheme(typeof(OverlayWindow));
                overlay.ApplyStyling();
                Require(overlay.WindowDecorations == WindowDecorations.None);
            }
            finally { overlay.Close(); }
        });
        Check("overlay transparency", () =>
        {
            OverlayHost overlay = new();
            try
            {
                Require(overlay.TransparencyLevelHint.Count == 1 && overlay.TransparencyLevelHint[0] == WindowTransparencyLevel.Transparent);
                overlay.TransparencyLevelHint = new[] { WindowTransparencyLevel.None };
                overlay.ClearValue(TopLevel.TransparencyLevelHintProperty);
                Require(overlay.TransparencyLevelHint.Count == 1 && overlay.TransparencyLevelHint[0] == WindowTransparencyLevel.Transparent);
            }
            finally { overlay.Close(); }
        });
        if (failures.Count != 0)
            throw new InvalidOperationException($"Packaged inherited properties: {failures.Count}/{checks} failed: " + string.Join("; ", failures));
        Console.WriteLine($"Inherited control properties: {checks} packaged checks passed.");

        void Check(string name, Action action)
        {
            checks++;
            try { action(); }
            catch (Exception error) { failures.Add(name + ": " + error.Message); }
        }
    }

    private static void Require(bool condition)
    {
        if (!condition) throw new InvalidOperationException("Inherited control property contract changed.");
    }

    private static LayoutFloatingWindowControl CreateHost(string kind) => kind switch
    {
        "document" => new DocumentHost(),
        "tool" => new ToolHost(),
        _ => new GroupHost()
    };

    private sealed class DocumentHost() : LayoutDocumentFloatingWindowControl(new LayoutDocumentFloatingWindow());
    private sealed class ToolHost() : LayoutAnchorableFloatingWindowControl(new LayoutAnchorableFloatingWindow());
    private sealed class GroupHost() : LayoutGroupFloatingWindowControl(new LayoutGroupFloatingWindow());
    private sealed class OverlayHost() : OverlayWindow(new DockingManager(), new Panel(), new Panel());
}
