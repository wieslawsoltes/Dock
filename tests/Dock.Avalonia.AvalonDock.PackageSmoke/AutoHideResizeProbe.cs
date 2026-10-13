using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Themes;
using Dock.Avalonia.Controls;

namespace PackageConsumer;

internal static class AutoHideResizeProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        int checks = 0;
        foreach (AnchorSide side in new[] { AnchorSide.Left, AnchorSide.Right, AnchorSide.Top, AnchorSide.Bottom })
        foreach (bool rtl in new[] { false, true })
        foreach (bool grow in new[] { false, true })
        {
            checks++;
            try { VerifyEdge(side, rtl, grow); }
            catch (Exception error) { failures.Add($"{side}, RTL={rtl}, grow={grow}: {error.Message}"); }
        }
        if (failures.Count != 0)
            throw new InvalidOperationException($"Packaged auto-hide resize: {failures.Count}/{checks} failed: " + string.Join("; ", failures));
        Console.WriteLine($"Auto-hide resize: {checks} packaged checks passed.");
    }

    private static void VerifyEdge(AnchorSide side, bool rtl, bool grow)
    {
        LayoutAnchorable tool = new()
        {
            Title = "Pinned tool", Content = "Tool content", AutoHideWidth = 220, AutoHideHeight = 190,
            AutoHideMinWidth = 175, AutoHideMinHeight = 155
        };
        LayoutRoot layout = new()
        {
            RootPanel = new LayoutPanel(new LayoutDocumentPane(new LayoutDocument { Title = "Editor", Content = "Document" }))
        };
        LayoutAnchorSide anchorSide = side switch
        {
            AnchorSide.Left => layout.LeftSide!, AnchorSide.Right => layout.RightSide!,
            AnchorSide.Top => layout.TopSide!, _ => layout.BottomSide!
        };
        anchorSide.Children.Add(new LayoutAnchorGroup { Children = { tool } });
        DockingManager manager = new()
        {
            Layout = layout, AutoHideWindowClosingTimer = int.MaxValue,
            FlowDirection = rtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight
        };
        Window window = new() { Width = 900, Height = 650 };
        window.Styles.Add(new AvalonDockTheme());
        window.Content = manager;
        Point releasePoint = default;
        bool pressed = false;
        try
        {
            List<string> errors = new();
            window.Show(); Update(); tool.IsSelected = true; Update();
            LayoutGridResizerControl splitter = manager.GetVisualDescendants().OfType<LayoutGridResizerControl>()
                .Single(control => control.Name == "PART_PinnedDockSplitter" && control.IsEffectivelyVisible);
            PinnedDockControl host = splitter.FindAncestorOfType<PinnedDockControl>()!;
            PinnedDockHostPanel area = host.FindAncestorOfType<PinnedDockHostPanel>()!;
            ContentControl content = host.GetVisualDescendants().OfType<ContentControl>()
                .Single(control => control.Name == "PART_PinnedDock");
            Border ghost = host.GetVisualDescendants().OfType<Border>()
                .Single(control => control.Name == "PART_PinnedDockResizePreview");
            bool horizontal = side is AnchorSide.Left or AnchorSide.Right;
            bool reverse = side is AnchorSide.Right or AnchorSide.Bottom;
            Rect areaBounds = PhysicalBounds(area);
            double extent = horizontal ? areaBounds.Width : areaBounds.Height;
            double thickness = horizontal ? splitter.Bounds.Width : splitter.Bounds.Height;
            double original = horizontal ? tool.AutoHideWidth : tool.AutoHideHeight;
            double minimum = horizontal ? tool.AutoHideMinWidth : tool.AutoHideMinHeight;
            double upperLimit = horizontal ? extent : extent - tool.AutoHideMinHeight;
            double expectedGhost = grow != reverse ? upperLimit : 25;
            double expectedSize = !grow ? minimum : side switch
            {
                AnchorSide.Left => extent, AnchorSide.Right => extent - 25 - thickness,
                AnchorSide.Top => extent - tool.AutoHideMinHeight, _ => extent - 25 - thickness
            };
            Point start = splitter.TranslatePoint(new Point(splitter.Bounds.Width / 2, splitter.Bounds.Height / 2), window)!.Value;
            double delta = (grow ? 2000 : -2000) * (reverse ? -1 : 1);
            Point end = start + (horizontal ? new Vector(delta, 0) : new Vector(0, delta));
            releasePoint = end;
            window.MouseMove(start);
            window.MouseDown(start, MouseButton.Left);
            pressed = true;
            window.MouseMove(end, RawInputModifiers.LeftMouseButton);
            Update();
            if (!ghost.IsVisible) errors.Add("The resize ghost was not shown.");
            Equal(original, horizontal ? tool.AutoHideWidth : tool.AutoHideHeight, "size before release", errors);
            Rect ghostBounds = PhysicalBounds(ghost);
            Equal(expectedGhost, horizontal ? ghostBounds.X - areaBounds.X : ghostBounds.Y - areaBounds.Y, "ghost coordinate", errors);
            window.MouseUp(end, MouseButton.Left);
            pressed = false;
            Update();
            if (ghost.IsVisible) errors.Add("The resize ghost survived release.");
            Equal(expectedSize, horizontal ? tool.AutoHideWidth : tool.AutoHideHeight, "committed size", errors);
            Equal(expectedSize, horizontal ? content.Bounds.Width : content.Bounds.Height, "displayed size", errors);
            Rect contentBounds = PhysicalBounds(content);
            double fixedEdge = side switch
            {
                AnchorSide.Left => contentBounds.X - areaBounds.X, AnchorSide.Right => contentBounds.Right - areaBounds.X,
                AnchorSide.Top => contentBounds.Y - areaBounds.Y, _ => contentBounds.Bottom - areaBounds.Y
            };
            Equal(reverse ? extent : 0, fixedEdge, "stationary content edge", errors);
            if (errors.Count != 0) throw new InvalidOperationException(string.Join(", ", errors));
        }
        finally
        {
            if (pressed) window.MouseUp(releasePoint, MouseButton.Left);
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }

        void Update() { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); }

        Rect PhysicalBounds(Control control)
        {
            Point first = control.TranslatePoint(default, window)!.Value;
            Point second = control.TranslatePoint(new Point(control.Bounds.Width, control.Bounds.Height), window)!.Value;
            return new Rect(Math.Min(first.X, second.X), Math.Min(first.Y, second.Y),
                Math.Abs(second.X - first.X), Math.Abs(second.Y - first.Y));
        }
    }

    private static void Equal(double expected, double actual, string label, List<string> errors)
    {
        if (!double.IsFinite(actual) || Math.Abs(expected - actual) > 0.1)
            errors.Add($"{label}: expected {expected}, actual {actual}");
    }
}
