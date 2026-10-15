using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Themes;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class ThemeWindowLifetimeTests
{
    public static IEnumerable<object[]> ClosePoints()
    {
        foreach (string kind in new[] { "document", "tool", "navigator", "overlay" })
        foreach (string phase in new[] { "removal", "creation", "addition" })
        foreach (bool cancel in new[] { false, true })
            yield return new object[] { kind, phase, cancel };
    }

    [AvaloniaTheory]
    [MemberData(nameof(ClosePoints))]
    public void Closing_During_Theme_Replacement_Releases_Resources_Unless_Closing_Is_Canceled(
        string kind, string phase, bool cancel)
    {
        ResourceDictionary previous = new() { ["Marker"] = "previous" };
        ResourceDictionary next = new() { ["Marker"] = "next" };
        using Workspace workspace = new(new TestDictionaryTheme(previous));
        Window target = workspace.CreateWindow(kind);
        bool cancelClose = cancel;
        bool attempted = false;
        bool closed = false;
        target.Closing += (_, args) => args.Cancel = cancelClose;
        target.Closed += (_, _) => closed = true;
        target.ResourcesChanged += (_, _) =>
        {
            if (phase == "removal" && !target.Resources.TryGetResource("Marker", null, out _)) CloseOnce();
            else if (phase == "addition" && Equals(target.FindResource("Marker"), "next")) CloseOnce();
        };
        Theme replacement = phase == "creation"
            ? new CallbackTheme(() =>
            {
                // The manager loads first. Wait until this window has removed
                // its old provider and is constructing the replacement.
                if (!target.Resources.TryGetResource("Marker", null, out _)) CloseOnce();
            })
            : new TestDictionaryTheme(next);
        try
        {
            Assert.Equal("previous", target.FindResource("Marker"));
            workspace.Manager.Theme = replacement;
            Dispatcher.UIThread.RunJobs();
            Assert.True(attempted);
            Assert.Equal(!cancel, closed);
            if (cancel)
            {
                Assert.NotEmpty(target.Resources.MergedDictionaries);
                if (phase == "creation")
                    Assert.True(target.Resources.TryGetResource(typeof(NavigatorWindow), null, out _));
                else Assert.Equal("next", target.FindResource("Marker"));
            }
            else Assert.Empty(target.Resources.MergedDictionaries);

            ResourceDictionary latest = new() { ["Marker"] = "latest" };
            workspace.Manager.Theme = new TestDictionaryTheme(latest);
            Assert.Equal("latest", workspace.Manager.FindResource("Marker"));
            if (cancel) Assert.Equal("latest", target.FindResource("Marker"));
            else Assert.Empty(target.Resources.MergedDictionaries);

            cancelClose = false;
            target.Close();
            Assert.True(closed);
            Assert.Empty(target.Resources.MergedDictionaries);
            int notifications = 0;
            target.ResourcesChanged += (_, _) => notifications++;
            previous["Marker"] = "retired previous";
            next["Marker"] = "retired next";
            latest["Marker"] = "live manager";
            Assert.Equal(0, notifications);
            Assert.Equal("live manager", workspace.Manager.FindResource("Marker"));
        }
        finally { cancelClose = false; target.Close(); }

        void CloseOnce()
        {
            if (attempted) return;
            attempted = true;
            target.Close();
        }
    }

    private sealed class CallbackTheme(Action callback) : Theme
    {
        public override Uri GetResourceUri()
        {
            callback();
            return new GenericTheme().GetResourceUri();
        }
    }

    private sealed class TestOverlay(DockingManager manager)
        : OverlayWindow(manager, new Border(), new Border()) { }

    private sealed class Workspace : IDisposable
    {
        internal DockingManager Manager { get; } = new();
        private readonly LayoutDocument _document = new() { Title = "Document", Content = "Document" };
        private readonly LayoutAnchorable _tool = new() { Title = "Tool", Content = "Tool", CanClose = true };
        private readonly Window _owner;

        internal Workspace(Theme theme)
        {
            Manager.Theme = theme;
            Manager.Layout = new() { RootPanel = new()
                { Children = { new LayoutDocumentPane(_document), new LayoutAnchorablePane(_tool) } } };
            _owner = new() { Width = 800, Height = 600, Content = Manager };
            _owner.Show();
            Dispatcher.UIThread.RunJobs();
        }

        internal Window CreateWindow(string kind)
        {
            if (kind == "navigator") return Manager.CreateNavigatorWindow();
            if (kind == "overlay") return new TestOverlay(Manager);
            if (kind == "document") _document.Float();
            else _tool.Float();
            Dispatcher.UIThread.RunJobs();
            return Assert.Single(Manager.FloatingWindows);
        }

        public void Dispose()
        {
            Manager.Layout.FloatingWindows.Clear();
            Manager.Theme = null;
            _owner.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
