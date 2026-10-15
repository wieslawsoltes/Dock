using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Themes;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class ThemeCallbackTests
{
    public static IEnumerable<object[]> WindowCases()
    {
        foreach (string kind in new[] { "document", "tool", "navigator", "overlay" })
        foreach (bool flag in new[] { false, true })
            yield return new object[] { kind, flag };
    }

    [AvaloniaTheory]
    [MemberData(nameof(WindowCases))]
    public void Redirected_Callbacks_Use_Their_Argument_For_The_Manager_And_Current_Property_For_Windows(string kind, bool afterBase)
    {
        using Workspace workspace = new();
        Window existing = workspace.CreateWindow(kind);
        Theme requested = CreateTheme("requested");
        Theme current = CreateTheme("current");
        workspace.Manager.Callback = (args, after) =>
        {
            if (after == afterBase && ReferenceEquals(args.NewValue, requested)) workspace.Manager.Theme = current;
        };
        workspace.Manager.Theme = requested;
        Assert.Same(current, workspace.Manager.Theme);
        AssertMarker(workspace.Manager, afterBase ? "current" : "requested");
        AssertMarker(existing, "current");
        AssertMarker(workspace.CreateWindow(kind), "current");
    }

    [AvaloniaTheory]
    [MemberData(nameof(WindowCases))]
    public void New_Windows_Read_The_Current_Theme_After_A_Suppressed_Or_Throwing_Manager_Callback(string kind, bool fail)
    {
        using Workspace workspace = new();
        Window existing = workspace.CreateWindow(kind);
        Theme next = CreateTheme("current");
        workspace.Manager.Suppress = true;
        workspace.Manager.Fail = fail;
        if (fail) Assert.Throws<InvalidOperationException>(() => workspace.Manager.Theme = next);
        else workspace.Manager.Theme = next;
        Assert.Same(next, workspace.Manager.Theme);
        AssertMarker(workspace.Manager, "initial");
        AssertMarker(existing, "initial");
        AssertMarker(workspace.CreateWindow(kind), "current");
    }

    [AvaloniaTheory]
    [MemberData(nameof(WindowCases))]
    public void An_Explicit_Base_Callback_Uses_Its_NewValue_Without_Changing_The_Theme_Property(string kind, bool clear)
    {
        using Workspace workspace = new();
        Window existing = workspace.CreateWindow(kind);
        workspace.Manager.Suppress = true;
        workspace.Manager.Theme = clear ? null : CreateTheme("argument");
        AvaloniaPropertyChangedEventArgs args = workspace.Manager.LastChange!;
        Theme current = CreateTheme("current");
        workspace.Manager.Theme = current;
        workspace.Manager.Apply(args);
        Assert.Same(current, workspace.Manager.Theme);
        AssertMarker(workspace.Manager, clear ? null : "argument");
        AssertMarker(existing, "current");
        AssertMarker(workspace.CreateWindow(kind), "current");
    }

    private static Theme CreateTheme(string marker) => new TestDictionaryTheme(new() { ["CallbackMarker"] = marker });

    private static void AssertMarker(StyledElement target, string? marker)
    {
        bool found = target.Resources.TryGetResource("CallbackMarker", null, out object? actual);
        Assert.Equal(marker is not null, found);
        Assert.Equal(marker, actual);
    }

    private sealed class CallbackManager : DockingManager
    {
        internal Action<AvaloniaPropertyChangedEventArgs, bool>? Callback { get; set; }
        internal bool Suppress { get; set; }
        internal bool Fail { get; set; }
        internal AvaloniaPropertyChangedEventArgs? LastChange { get; private set; }
        internal void Apply(AvaloniaPropertyChangedEventArgs args) => base.OnThemeChanged(args);
        protected override void OnThemeChanged(AvaloniaPropertyChangedEventArgs e)
        {
            LastChange = e;
            Callback?.Invoke(e, false);
            if (Fail) throw new InvalidOperationException("Theme callback failure.");
            if (!Suppress) base.OnThemeChanged(e);
            Callback?.Invoke(e, true);
        }
    }

    private sealed class TestOverlay(DockingManager manager) : OverlayWindow(manager, new Border(), new Border()) { }

    private sealed class Workspace : IDisposable
    {
        internal CallbackManager Manager { get; } = new();
        private readonly List<Window> _windows = new();
        private readonly Window _owner;

        internal Workspace()
        {
            Manager.Theme = CreateTheme("initial");
            _owner = new() { Width = 800, Height = 600, Content = Manager };
            _owner.Show();
            Dispatcher.UIThread.RunJobs();
        }

        internal Window CreateWindow(string kind)
        {
            Window window;
            if (kind == "navigator") window = Manager.CreateNavigatorWindow();
            else if (kind == "overlay") window = new TestOverlay(Manager);
            else
            {
                LayoutContent content;
                if (kind == "document")
                {
                    LayoutDocument document = new() { Title = "Document", Content = "Document" };
                    Manager.Layout.RootPanel.Children.Add(new LayoutDocumentPane(document));
                    content = document;
                }
                else
                {
                    LayoutAnchorable tool = new() { Title = "Tool", Content = "Tool", CanClose = true };
                    Manager.Layout.RootPanel.Children.Add(new LayoutAnchorablePane(tool));
                    content = tool;
                }
                content.Float();
                Dispatcher.UIThread.RunJobs();
                window = (Window)content.FindParent<LayoutFloatingWindow>()!.DockWindow.Host!;
            }
            _windows.Add(window);
            return window;
        }

        public void Dispose()
        {
            Manager.Callback = null;
            Manager.Suppress = Manager.Fail = false;
            Manager.Layout.FloatingWindows.Clear();
            foreach (Window window in _windows) window.Close();
            Manager.Theme = null;
            _owner.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
