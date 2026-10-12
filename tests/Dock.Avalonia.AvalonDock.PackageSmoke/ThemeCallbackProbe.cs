using System;
using Avalonia;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Themes;
using Dock.Model.Core;

namespace PackageConsumer;

internal static class ThemeCallbackProbe
{
    internal static void Verify()
    {
        CallbackManager manager = new() { Theme = CreateTheme("initial") };
        NavigatorWindow existing = manager.CreateNavigatorWindow();
        NavigatorWindow? later = null;
        LayoutDocumentFloatingWindow model = new() { RootDocument = new LayoutDocument() };
        LayoutDocumentFloatingWindowControl? floating = null;
        try
        {
            manager.Suppress = true;
            manager.Theme = CreateTheme("argument");
            AvaloniaPropertyChangedEventArgs argument = manager.LastChange!;
            manager.Theme = CreateTheme("current");
            later = manager.CreateNavigatorWindow();
            manager.Layout.FloatingWindows.Add(model);
            floating = new(model);
            Check(manager, "initial");
            Check(existing, "initial");
            Check(later, "current");
            Check(floating, "current");
            manager.Apply(argument);
            Check(manager, "argument");
            Check(existing, "current");
            Check(later, "current");
            Check(floating, "current");
            manager.Theme = null;
            AvaloniaPropertyChangedEventArgs clear = manager.LastChange!;
            manager.Theme = CreateTheme("current");
            manager.Apply(clear);
            Check(manager, null);
            Check(existing, "current");
            Check(floating, "current");
        }
        finally
        {
            existing.Close();
            later?.Close();
            if (floating is not null) ((IHostWindow)floating).Exit();
            manager.Layout.FloatingWindows.Clear();
            manager.Suppress = false;
            manager.Theme = null;
        }
    }

    private static Theme CreateTheme(string marker) => new ProbeTheme(new() { ["CallbackMarker"] = marker });

    private static void Check(StyledElement element, string? expected)
    {
        bool found = element.Resources.TryGetResource("CallbackMarker", null, out object? actual);
        if (found != (expected is not null) || !Equals(expected, actual))
            throw new Exception("Packaged theme callbacks did not distinguish callback arguments from the current window theme.");
    }

    private sealed class ProbeTheme(global::Avalonia.Controls.ResourceDictionary resources) : DictionaryTheme(resources) { }

    private sealed class CallbackManager : DockingManager
    {
        internal bool Suppress { get; set; }
        internal AvaloniaPropertyChangedEventArgs? LastChange { get; private set; }
        internal void Apply(AvaloniaPropertyChangedEventArgs args) => base.OnThemeChanged(args);
        protected override void OnThemeChanged(AvaloniaPropertyChangedEventArgs e)
        {
            LastChange = e;
            if (!Suppress) base.OnThemeChanged(e);
        }
    }
}
