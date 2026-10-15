using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Themes;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class ThemeResourceRegistryTests
{
    [AvaloniaFact]
    public void Compiled_Uri_Resources_Are_Loaded_In_The_Manager_Floating_Hosts_And_Navigator()
    {
        using Workspace workspace = new();
        Uri uri = new("avares://Dock.Avalonia.AvalonDock.Tests/RegisteredThemeResources.axaml");
        int calls = 0;
        workspace.Manager.ThemeResources.Register(uri, () => { calls++; return new RegisteredThemeResources(); });
        workspace.Manager.Theme = new UriTheme(uri);
        workspace.Document.Float();
        workspace.Tool.Float();
        Dispatcher.UIThread.RunJobs();
        NavigatorWindow navigator = workspace.Manager.CreateNavigatorWindow();
        try
        {
            AssertBrush(workspace.Manager);
            LayoutFloatingWindowControl[] hosts = workspace.Manager.FloatingWindows.ToArray();
            Assert.Equal(2, hosts.Length);
            Assert.All(hosts, AssertBrush);
            AssertBrush(navigator);
            Assert.Equal(4, calls);
            workspace.Manager.Theme = null;
            Assert.False(workspace.Manager.Resources.TryGetResource("RegisteredBrush", null, out _));
            Assert.All(hosts, host => Assert.False(host.Resources.TryGetResource("RegisteredBrush", null, out _)));
            Assert.False(navigator.Resources.TryGetResource("RegisteredBrush", null, out _));
        }
        finally { navigator.Close(); }
    }

    [AvaloniaFact]
    public void Shared_Factory_Results_Keep_Live_Updates_And_Release_Resource_Ownership()
    {
        using Workspace workspace = new();
        ResourceDictionary resources = new() { ["RegisteredBrush"] = Brushes.Red };
        Uri uri = new("themes/shared.axaml", UriKind.Relative);
        workspace.Manager.ThemeResources.Register(uri, () => resources);
        workspace.Manager.Theme = new UriTheme(uri);
        workspace.Document.Float();
        Dispatcher.UIThread.RunJobs();
        LayoutFloatingWindowControl host = Assert.Single(workspace.Manager.FloatingWindows);
        resources["RegisteredBrush"] = Brushes.Green;
        Assert.Same(Brushes.Green, workspace.Manager.FindResource("RegisteredBrush"));
        Assert.Same(Brushes.Green, host.FindResource("RegisteredBrush"));
        workspace.Manager.Theme = null;
        Assert.Null(resources.Owner);
    }

    [AvaloniaFact]
    public void Uri_Registrations_Belong_To_Their_Manager()
    {
        DockingManager first = new();
        DockingManager second = new();
        Uri uri = new("avares://Application/Theme.axaml");
        first.ThemeResources.Register(uri, static () => new ResourceDictionary { ["Marker"] = "first" });
        second.ThemeResources.Register(uri, static () => new ResourceDictionary { ["Marker"] = "second" });
        first.Theme = new UriTheme(uri);
        second.Theme = new UriTheme(uri);
        Assert.Equal("first", first.FindResource("Marker"));
        Assert.Equal("second", second.FindResource("Marker"));
        first.Theme = null;
        second.Theme = null;
    }

    [AvaloniaTheory]
    [InlineData("unknown")]
    [InlineData("null")]
    [InlineData("throws")]
    [InlineData("cycle")]
    public void Failed_Uri_Resolution_Can_Be_Recovered_With_A_Subsequent_Theme(string failure)
    {
        DockingManager manager = new();
        Uri uri = new("avares://Application/Theme.axaml");
        if (failure != "unknown")
            manager.ThemeResources.Register(uri, () => failure switch
            {
                "null" => null!,
                "cycle" => (ResourceDictionary)manager.Resources,
                _ => throw new InvalidOperationException("factory failure")
            });
        if (failure == "unknown") Assert.Throws<NotSupportedException>(() => manager.Theme = new UriTheme(uri));
        else Assert.Throws<InvalidOperationException>(() => manager.Theme = new UriTheme(uri));
        manager.Theme = new GenericTheme();
        Assert.True(manager.Resources.TryGetResource(typeof(DockingManager), null, out _));
        manager.Theme = null;
    }

    [AvaloniaFact]
    public void Invalid_And_Duplicate_Registrations_Are_Rejected()
    {
        ThemeResourceRegistry registry = new();
        Uri uri = new("avares://Application/Theme.axaml");
        Assert.Throws<ArgumentNullException>(() => registry.Register(null!, static () => new()));
        Assert.Throws<ArgumentNullException>(() => registry.Register(uri, null!));
        Assert.Throws<ArgumentException>(() => registry.Register(new GenericTheme().GetResourceUri(), static () => new()));
        registry.Register(uri, static () => new());
        Assert.Throws<ArgumentException>(() => registry.Register(uri, static () => new()));
    }

    [AvaloniaFact]
    public void A_Factory_Can_Replace_The_Theme_During_Resolution()
    {
        DockingManager manager = new();
        Uri first = new("avares://Application/First.axaml");
        Uri second = new("avares://Application/Second.axaml");
        manager.ThemeResources.Register(first, () =>
        {
            manager.Theme = new UriTheme(second);
            return new ResourceDictionary { ["Marker"] = "first" };
        });
        manager.ThemeResources.Register(second, static () => new ResourceDictionary { ["Marker"] = "second" });
        manager.Theme = new UriTheme(first);
        Assert.Equal(second, manager.Theme.GetResourceUri());
        Assert.Equal("second", manager.FindResource("Marker"));
        manager.Theme = null;
    }

    private static void AssertBrush(Control control) => Assert.Equal(Color.Parse("#2468AC"), Assert.IsAssignableFrom<ISolidColorBrush>(control.FindResource("RegisteredBrush")).Color);
    private sealed class UriTheme(Uri uri) : Theme { public override Uri GetResourceUri() => uri; }

    private sealed class Workspace : IDisposable
    {
        public DockingManager Manager { get; }
        public Window Window { get; }
        public LayoutDocument Document { get; } = new() { ContentId = "document", Content = "Document" };
        public LayoutAnchorable Tool { get; } = new() { ContentId = "tool", Content = "Tool" };
        public Workspace()
        {
            Manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel { Children = { new LayoutDocumentPane(Document), new LayoutAnchorablePane(Tool) } } } };
            Window = new() { Width = 640, Height = 480, Content = Manager };
            Window.Show();
            Dispatcher.UIThread.RunJobs();
        }
        public void Dispose()
        {
            Window.Content = null;
            Dispatcher.UIThread.RunJobs();
            Window.Close();
        }
    }
}
