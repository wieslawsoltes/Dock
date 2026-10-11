using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Themes;
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class ThemeTests
{
    [AvaloniaFact]
    public void Compiled_Theme_Bindings_And_Both_Xml_Namespaces_Use_The_AvalonDock_Property()
    {
        ThemeBindingViewModel model = new() { Theme = new GenericTheme() };
        ThemeBindingFixture view = new() { DataContext = model };
        try
        {
            view.Show();
            Dispatcher.UIThread.RunJobs();
            ThemeNotificationsManager bound = view.FindControl<ThemeNotificationsManager>("BoundManager")!;
            Assert.Same(model.Theme, bound.Theme);
            Assert.Equal(1, bound.Changes);
            Assert.IsType<GenericTheme>(view.FindControl<DockingManager>("DefaultNamespaceManager")!.Theme);
            Assert.IsType<GenericTheme>(view.FindControl<DockingManager>("AliasManager")!.Theme);
            Assert.Equal(3, view.GetVisualDescendants().OfType<DockControl>().Count());
            Assert.True(Assert.IsType<ResourceDictionary>(view.Resources["GenericByUri"]).TryGetResource(typeof(NavigatorWindow), null, out _));
            model.Theme = new TestDictionaryTheme(new() { ["Marker"] = "bound" });
            Assert.Same(model.Theme, bound.Theme);
            Assert.Equal("bound", bound.FindResource("Marker"));
            Assert.Equal(2, bound.Changes);
            model.Theme = null;
            Assert.Null(bound.Theme);
            Assert.Equal(3, bound.Changes);
        }
        finally { view.Close(); }
    }

    [AvaloniaFact]
    public void Dictionary_And_Generic_Themes_Expose_Their_Resource_Contracts()
    {
        ResourceDictionary dictionary = new();
        Assert.Null(new TestDictionaryTheme().ThemeResourceDictionary);
        Assert.Null(new TestDictionaryTheme(null).ThemeResourceDictionary);
        TestDictionaryTheme theme = new(dictionary);
        Assert.Same(dictionary, theme.ThemeResourceDictionary);
        Assert.Null(theme.GetResourceUri());
        Uri uri = new GenericTheme().GetResourceUri();
        Assert.True(uri.IsAbsoluteUri);
        Assert.Equal("avares", uri.Scheme);
        using Workspace workspace = new(new GenericTheme());
        Assert.Single(workspace.Manager.GetVisualDescendants().OfType<DockControl>());
        Assert.True(workspace.Manager.Resources.TryGetResource(typeof(DockingManager), null, out object? managerTheme));
        Assert.IsType<ControlTheme>(managerTheme);
        Assert.True(workspace.Manager.Resources.TryGetResource(typeof(NavigatorWindow), null, out object? navigatorTheme));
        Assert.IsType<ControlTheme>(navigatorTheme);
    }

    [AvaloniaFact]
    public void Shared_Dictionary_Changes_Reach_Independent_Managers_And_Theme_Variants()
    {
        ResourceDictionary dictionary = new();
        dictionary.ThemeDictionaries[ThemeVariant.Default] = new ResourceDictionary { ["ThemeBrush"] = Brushes.Red };
        dictionary.ThemeDictionaries[ThemeVariant.Dark] = new ResourceDictionary { ["ThemeBrush"] = Brushes.Black };
        using Workspace first = new(new TestDictionaryTheme(dictionary));
        using Workspace second = new(new TestDictionaryTheme(dictionary));
        first.Owner.RequestedThemeVariant = ThemeVariant.Light;
        second.Owner.RequestedThemeVariant = ThemeVariant.Dark;
        BindBrush(first.Manager);
        BindBrush(second.Manager);
        Assert.Same(Brushes.Red, first.Manager.Background);
        Assert.Same(Brushes.Black, second.Manager.Background);
        ((ResourceDictionary)dictionary.ThemeDictionaries[ThemeVariant.Default])["ThemeBrush"] = Brushes.Green;
        ((ResourceDictionary)dictionary.ThemeDictionaries[ThemeVariant.Dark])["ThemeBrush"] = Brushes.Blue;
        Assert.Same(Brushes.Green, first.Manager.Background);
        Assert.Same(Brushes.Blue, second.Manager.Background);
        first.Manager.Theme = null;
        Assert.NotNull(dictionary.Owner);
        second.Manager.Theme = null;
        Assert.Null(dictionary.Owner);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void View_Scoped_Styles_Can_Create_Floating_Windows_Without_Application_Dock_Resources(bool generic)
    {
        Styles styles = Application.Current!.Styles;
        AvalonDockTheme applicationTheme = Assert.Single(styles.OfType<AvalonDockTheme>());
        int index = styles.IndexOf(applicationTheme);
        styles.RemoveAt(index);
        try
        {
            using Workspace workspace = new(generic ? new GenericTheme() : null, localStyles: true);
            Assert.IsType<ControlTheme>(workspace.Manager.FindResource(typeof(DockingManager)));
            Assert.Single(workspace.Manager.GetVisualDescendants().OfType<DockControl>());
            workspace.Document.Float();
            workspace.Tool.Float();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(2, workspace.Manager.FloatingWindows.Count());
            foreach (LayoutFloatingWindowControl host in workspace.Manager.FloatingWindows)
            {
                Assert.True(host.IsVisible);
                Assert.Single(host.GetVisualDescendants().OfType<DockControl>());
            }
            NavigatorWindow navigator = workspace.Manager.CreateNavigatorWindow();
            try
            {
                _ = navigator.ShowDialog(workspace.Owner);
                Dispatcher.UIThread.RunJobs();
                Assert.True(navigator.IsVisible);
                Assert.Equal(2, navigator.GetVisualDescendants().OfType<ListBox>().Count());
            }
            finally { navigator.Close(); }
        }
        finally { styles.Insert(index, applicationTheme); }
    }

    [AvaloniaFact]
    public void Replacement_Updates_Docked_Floating_And_Navigator_Resources_And_Preserves_Local_Resources()
    {
        ResourceDictionary first = new() { ["ThemeBrush"] = Brushes.Red };
        ResourceDictionary next = new() { ["ThemeBrush"] = Brushes.Blue };
        using Workspace workspace = new(new TestDictionaryTheme(first));
        ResourceDictionary applicationResources = new() { ["ApplicationValue"] = "kept" };
        workspace.Manager.Resources.MergedDictionaries.Add(applicationResources);
        workspace.Document.Float();
        workspace.Tool.Float();
        Dispatcher.UIThread.RunJobs();
        LayoutFloatingWindowControl[] floating = workspace.Manager.FloatingWindows.ToArray();
        Assert.Equal(2, floating.Length);
        NavigatorWindow navigator = workspace.Manager.CreateNavigatorWindow();
        _ = navigator.ShowDialog(workspace.Owner);
        try
        {
            BindBrush(workspace.Manager);
            foreach (LayoutFloatingWindowControl host in floating) BindBrush(host);
            BindBrush(navigator);
            Assert.All(floating, host => Assert.Same(Brushes.Red, host.Background));
            Assert.Same(Brushes.Red, navigator.Background);
            workspace.Manager.Theme = new TestDictionaryTheme(next);
            Assert.Same(Brushes.Blue, workspace.Manager.Background);
            Assert.All(floating, host => Assert.Same(Brushes.Blue, host.Background));
            Assert.Same(Brushes.Blue, navigator.Background);
            Assert.Null(first.Owner);
            next["ThemeBrush"] = Brushes.Green;
            Assert.All(floating, host => Assert.Same(Brushes.Green, host.Background));
            Assert.Same(Brushes.Green, navigator.Background);
            workspace.Manager.Theme = null;
            Assert.Null(next.Owner);
            Assert.Equal("kept", workspace.Manager.FindResource("ApplicationValue"));
            Assert.Same(applicationResources, Assert.Single(workspace.Manager.Resources.MergedDictionaries));
        }
        finally { navigator.Close(); }
    }

    [AvaloniaFact]
    public void A_Hidden_Navigator_And_A_Later_Floating_Window_Receive_Current_Resources()
    {
        using Workspace workspace = new();
        NavigatorWindow navigator = workspace.Manager.CreateNavigatorWindow();
        _ = navigator.ShowDialog(workspace.Owner);
        navigator.Hide();
        try
        {
            ResourceDictionary dictionary = new() { ["ThemeBrush"] = Brushes.Blue };
            workspace.Manager.Theme = new TestDictionaryTheme(dictionary);
            Assert.Same(Brushes.Blue, navigator.FindResource("ThemeBrush"));
            workspace.Document.Float();
            Dispatcher.UIThread.RunJobs();
            Assert.Same(Brushes.Blue, Assert.Single(workspace.Manager.FloatingWindows).FindResource("ThemeBrush"));
        }
        finally { navigator.Close(); }
    }

    [AvaloniaFact]
    public void An_Externally_Owned_Dictionary_Can_Be_Shared_And_Transferred_Without_Taking_Its_Owner()
    {
        ResourceDictionary dictionary = new() { ["ThemeBrush"] = Brushes.Red };
        Border firstOwner = new();
        Border nextOwner = new();
        firstOwner.Resources.MergedDictionaries.Add(dictionary);
        using Workspace workspace = new(new TestDictionaryTheme(dictionary));
        BindBrush(workspace.Manager);
        Assert.Same(firstOwner, dictionary.Owner);
        firstOwner.Resources.MergedDictionaries.Remove(dictionary);
        nextOwner.Resources.MergedDictionaries.Add(dictionary);
        Dispatcher.UIThread.RunJobs();
        dictionary["ThemeBrush"] = Brushes.Blue;
        Assert.Same(Brushes.Blue, workspace.Manager.Background);
        Assert.Same(nextOwner, dictionary.Owner);
        nextOwner.Resources.MergedDictionaries.Remove(dictionary);
        Dispatcher.UIThread.RunJobs();
        dictionary["ThemeBrush"] = Brushes.Green;
        Assert.Same(Brushes.Green, workspace.Manager.Background);
        workspace.Manager.Theme = null;
        Assert.Null(dictionary.Owner);
    }

    [AvaloniaFact]
    public void Theme_Changes_Keep_AvalonDock_Resources_Separate_From_Avalonias_Control_Theme()
    {
        using Workspace workspace = new();
        ControlTheme controlTheme = new(typeof(DockingManager))
        {
            BasedOn = (ControlTheme)workspace.Manager.FindResource(typeof(DockingManager))!
        };
        ((StyledElement)workspace.Manager).Theme = controlTheme;
        workspace.Manager.Theme = new GenericTheme();
        Assert.Same(controlTheme, ((StyledElement)workspace.Manager).Theme);
        Assert.IsType<GenericTheme>(workspace.Manager.GetValue(DockingManager.ThemeProperty));
        Assert.Same(controlTheme, workspace.Manager.GetValue(StyledElement.ThemeProperty));
        workspace.Manager.Theme = null;
        Assert.Same(controlTheme, ((StyledElement)workspace.Manager).Theme);
    }

    [AvaloniaFact]
    public void A_Resource_Callback_Can_Replace_The_Theme_While_It_Is_Being_Applied()
    {
        using Workspace workspace = new();
        TestDictionaryTheme first = new(new() { ["Marker"] = "first" });
        TestDictionaryTheme next = new(new() { ["Marker"] = "next" });
        NavigatorWindow navigator = workspace.Manager.CreateNavigatorWindow();
        try
        {
            workspace.Manager.ResourcesChanged += (_, _) =>
            {
                if (Equals(workspace.Manager.FindResource("Marker"), "first")) workspace.Manager.Theme = next;
            };
            workspace.Manager.Theme = first;
            Assert.Same(next, workspace.Manager.Theme);
            Assert.Equal("next", workspace.Manager.FindResource("Marker"));
            Assert.Equal("next", navigator.FindResource("Marker"));
        }
        finally { navigator.Close(); }
    }

    [AvaloniaFact]
    public void Resource_Updates_Can_Reenter_And_A_Dictionary_Can_Already_Belong_To_A_Consumer()
    {
        ResourceDictionary dictionary = new() { ["ThemeBrush"] = Brushes.Red };
        using Workspace first = new();
        using Workspace second = new();
        first.Manager.Resources.MergedDictionaries.Add(dictionary);
        TestDictionaryTheme theme = new(dictionary);
        first.Manager.Theme = theme;
        second.Manager.Theme = theme;
        BindBrush(first.Manager);
        BindBrush(second.Manager);
        first.Manager.ResourcesChanged += (_, _) =>
        {
            if (ReferenceEquals(dictionary["ThemeBrush"], Brushes.Blue)) dictionary["ThemeBrush"] = Brushes.Green;
        };
        dictionary["ThemeBrush"] = Brushes.Blue;
        Assert.Same(Brushes.Green, first.Manager.Background);
        Assert.Same(Brushes.Green, second.Manager.Background);
        Assert.Same(first.Manager, dictionary.Owner);
    }

    [AvaloniaFact]
    public void A_Navigator_Keeps_Resources_When_Close_Is_Canceled_And_Releases_Them_When_It_Closes()
    {
        ResourceDictionary dictionary = new() { ["ThemeBrush"] = Brushes.Red };
        using Workspace workspace = new(new TestDictionaryTheme(dictionary));
        NavigatorWindow navigator = workspace.Manager.CreateNavigatorWindow();
        _ = navigator.ShowDialog(workspace.Owner);
        EventHandler<WindowClosingEventArgs> cancel = (_, args) => args.Cancel = true;
        navigator.Closing += cancel;
        navigator.Close();
        dictionary["ThemeBrush"] = Brushes.Blue;
        Assert.True(navigator.IsVisible);
        Assert.Same(Brushes.Blue, navigator.FindResource("ThemeBrush"));
        navigator.Closing -= cancel;
        navigator.Close();
        Assert.False(navigator.Resources.TryGetResource("ThemeBrush", null, out _));
        dictionary["ThemeBrush"] = Brushes.Green;
        Assert.False(navigator.Resources.TryGetResource("ThemeBrush", null, out _));
        Assert.Same(Brushes.Green, workspace.Manager.FindResource("ThemeBrush"));
    }

    [AvaloniaFact]
    public void A_Theme_Cannot_Merge_A_Managers_Own_Resource_Scope_Into_Itself()
    {
        using Workspace workspace = new(new TestDictionaryTheme(new() { ["Marker"] = "previous" }));
        Assert.Throws<InvalidOperationException>(() => workspace.Manager.Theme = new TestDictionaryTheme((ResourceDictionary)workspace.Manager.Resources));
        Assert.False(workspace.Manager.Resources.TryGetResource("Marker", null, out _));
        workspace.Manager.Theme = new TestDictionaryTheme(new() { ["Marker"] = "recovered" });
        Assert.Equal("recovered", workspace.Manager.FindResource("Marker"));
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Keeping_A_Theme_Does_Not_Keep_A_Manager_After_Use(bool generic, bool shown)
    {
        Theme theme = generic ? new GenericTheme() : new TestDictionaryTheme(new() { ["Marker"] = "kept" });
        WeakReference reference = CreateReleasedManager(theme, shown);
        for (int index = 0; index < 5 && reference.IsAlive; index++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        Assert.False(reference.IsAlive);
        if (theme is DictionaryTheme dictionary) dictionary.ThemeResourceDictionary!["Marker"] = "changed";
        GC.KeepAlive(theme);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateReleasedManager(Theme theme, bool shown)
    {
        DockingManager manager = new() { Theme = theme };
        if (shown)
        {
            Window owner = new() { Content = manager, Width = 600, Height = 400 };
            owner.Show();
            Dispatcher.UIThread.RunJobs();
            owner.Close();
        }
        return new(manager);
    }

    [AvaloniaFact]
    public void Theme_Subclasses_Resolve_The_Generic_Uri_And_Invalid_Themes_Remove_Previous_Resources()
    {
        using Workspace workspace = new(new UriTheme(new GenericTheme().GetResourceUri()));
        Assert.True(workspace.Manager.Resources.TryGetResource(typeof(DockingManager), null, out _));
        Assert.Throws<ArgumentException>(() => workspace.Manager.Theme = new UriTheme(null));
        Assert.Empty(workspace.Manager.Resources.MergedDictionaries);
        workspace.Manager.Theme = new GenericTheme();
        Assert.Throws<ArgumentNullException>(() => workspace.Manager.Theme = new TestDictionaryTheme());
        Assert.Empty(workspace.Manager.Resources.MergedDictionaries);
        workspace.Manager.Theme = null;
        Assert.Single(workspace.Manager.GetVisualDescendants().OfType<DockControl>());
    }

    private sealed class UriTheme(Uri? uri) : Theme
    {
        public override Uri? GetResourceUri() => uri;
    }

    private static void BindBrush(TemplatedControl control) =>
        control.Bind(TemplatedControl.BackgroundProperty, new DynamicResourceExtension("ThemeBrush"));

    private sealed class Workspace : IDisposable
    {
        internal DockingManager Manager { get; } = new();
        internal Window Owner { get; }
        internal LayoutDocument Document { get; } = new() { Title = "Document", Content = new TextBox() };
        internal LayoutAnchorable Tool { get; } = new() { Title = "Tool", Content = new TextBox() };
        internal Workspace(Theme? theme = null, bool localStyles = false)
        {
            Manager.Theme = theme;
            Manager.Layout = new() { RootPanel = new() { Children = { new LayoutDocumentPane(Document), new LayoutAnchorablePane(Tool) } } };
            Owner = new() { Width = 640, Height = 480 };
            if (localStyles) Owner.Styles.Add(new AvalonDockTheme());
            Owner.Content = Manager;
            Owner.Show();
            Dispatcher.UIThread.RunJobs();
        }
        public void Dispose()
        {
            Manager.Layout.FloatingWindows.Clear();
            Manager.Theme = null;
            Owner.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}

public class TestDictionaryTheme : DictionaryTheme
{
    public TestDictionaryTheme() { }
    public TestDictionaryTheme(ResourceDictionary? dictionary) : base(dictionary) { }
}
