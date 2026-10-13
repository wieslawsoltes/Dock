using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;
using ReactiveUI.Reactive;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class NativePinnedWindowTests
{
    [AvaloniaTheory]
    [InlineData(AnchorSide.Left)]
    [InlineData(AnchorSide.Right)]
    [InlineData(AnchorSide.Top)]
    [InlineData(AnchorSide.Bottom)]
    public void Native_Preview_Owns_The_Editor_And_Tracks_Themes_And_Display_Mode(AnchorSide side)
    {
        using Workspace workspace = new(side);
        PinnedDockWindow window = workspace.Preview;
        Assert.True(window.IsVisible);
        Assert.Same(window, TopLevel.GetTopLevel(workspace.Manager.AutoHideWindow!));
        Assert.Same(window, TopLevel.GetTopLevel(workspace.Editor));
        Assert.Equal("first", window.FindResource("Marker"));
        ControlTheme contentTheme = new(typeof(LayoutAnchorableControl))
        {
            BasedOn = (ControlTheme)window.FindResource(typeof(LayoutAnchorableControl))!,
            Setters = { new Setter(Control.TagProperty, "native content theme") }
        };
        workspace.Manager.AutoHideWindow!.AnchorableStyle = contentTheme;
        workspace.Update();
        Assert.Equal("native content theme", Assert.Single(window.GetVisualDescendants().OfType<LayoutAnchorableControl>()).Tag);
        ResourceDictionary next = new() { ["Marker"] = "next" };
        workspace.Manager.Theme = new TestDictionaryTheme(next);
        Assert.Equal("next", window.FindResource("Marker"));
        next["Marker"] = "live";
        Assert.Equal("live", window.FindResource("Marker"));

        workspace.Tool.DockModel.PinnedDockDisplayModeOverride = PinnedDockDisplayMode.Inline;
        workspace.Update();
        Assert.False(window.IsVisible);
        Assert.Empty(window.Resources.MergedDictionaries);
        Assert.Same(workspace.Owner, TopLevel.GetTopLevel(workspace.Editor));
        workspace.Tool.DockModel.PinnedDockDisplayModeOverride = PinnedDockDisplayMode.Overlay;
        workspace.Update();
        PinnedDockWindow reopened = workspace.Preview;
        Assert.NotSame(window, reopened);
        Assert.Same(reopened, TopLevel.GetTopLevel(workspace.Editor));
        Assert.Equal("live", reopened.FindResource("Marker"));

        workspace.Manager.DockFactory.HidePreviewingDockables(workspace.Manager.DockLayout);
        workspace.Update();
        Assert.False(reopened.IsVisible);
        Assert.Empty(reopened.Resources.MergedDictionaries);
        Assert.Null(workspace.Manager.AutoHideWindow!.Model);
        workspace.Show();
        Assert.Same(workspace.Preview, TopLevel.GetTopLevel(workspace.Editor));
    }

    [AvaloniaFact]
    public void Native_Preview_Uses_Manager_Menu_And_Custom_Close_Command()
    {
        using Workspace workspace = new();
        PinnedDockWindow window = workspace.Preview;
        ContextMenu menu = new() { Items = { new MenuItem { Header = "Tool action" } } };
        workspace.Manager.AnchorableContextMenu = menu;
        Button menuButton = window.GetVisualDescendants().OfType<Button>().Single(button => button.Name == "PART_MenuButton");
        Click(window, menuButton);
        Assert.True(menu.IsOpen);
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.Tool), menu.DataContext);
        menu.Close();
        int closes = 0;
        using var close = ReactiveCommand.Create(() => closes++, outputScheduler: ImmediateScheduler.Instance);
        workspace.Manager.GetLayoutItemFromModel(workspace.Tool)!.CloseCommand = close;
        workspace.Update();
        Button closeButton = window.GetVisualDescendants().OfType<Button>().Single(button => button.Name == "PART_CloseButton");
        Click(window, closeButton);
        Assert.Equal(1, closes);
        Assert.True(window.IsVisible);
    }

    [AvaloniaFact]
    public void Ctrl_Tab_From_The_Native_Preview_Opens_The_Manager_Navigator()
    {
        using Workspace workspace = new();
        Assert.True(workspace.Editor.Focus());
        workspace.Preview.KeyPressQwerty(PhysicalKey.Tab, RawInputModifiers.Control);
        workspace.Update();
        NavigatorWindow navigator = Assert.Single(workspace.Owner.OwnedWindows.OfType<NavigatorWindow>());
        Assert.True(navigator.IsVisible);
        LayoutContent selected = (navigator.SelectedDocument?.LayoutElement ?? navigator.SelectedAnchorable?.LayoutElement)!;
        navigator.KeyReleaseQwerty(PhysicalKey.ControlLeft, RawInputModifiers.None);
        workspace.Update();
        Assert.False(navigator.IsVisible);
        Assert.Same(selected, workspace.Manager.Layout.ActiveContent);
    }

    [AvaloniaFact]
    public void Native_Preview_Themes_Are_Isolated_Between_Managers()
    {
        using Workspace first = new();
        first.Tool.DockModel.KeepPinnedDockableVisible = true;
        using Workspace second = new();
        second.Manager.Theme = new TestDictionaryTheme(new() { ["Marker"] = "second" });
        Assert.Equal("first", first.Preview.FindResource("Marker"));
        Assert.Equal("second", second.Preview.FindResource("Marker"));
        Assert.Same(first.Preview, TopLevel.GetTopLevel(first.Editor));
        Assert.Same(second.Preview, TopLevel.GetTopLevel(second.Editor));
    }

    [AvaloniaFact]
    public void Detachment_Closes_The_Native_Preview_And_Reattachment_Uses_A_New_Window()
    {
        using Workspace workspace = new();
        PinnedDockWindow first = workspace.Preview;
        global::AvalonDock.Themes.AvalonDockTheme ownedTheme = Assert.Single(first.Styles.OfType<global::AvalonDock.Themes.AvalonDockTheme>());
        global::Avalonia.Styling.Style applicationStyle = new();
        first.Styles.Add(applicationStyle);
        workspace.Owner.Content = null;
        workspace.Update();
        Assert.False(first.IsVisible);
        Assert.Empty(first.Resources.MergedDictionaries);
        Assert.DoesNotContain(ownedTheme, first.Styles);
        Assert.Contains(applicationStyle, first.Styles);
        workspace.Owner.Content = workspace.Manager;
        workspace.Update();
        workspace.Show();
        Assert.NotSame(first, workspace.Preview);
        Assert.Same(workspace.Preview, TopLevel.GetTopLevel(workspace.Editor));
    }

    [AvaloniaFact]
    public async Task Window_Observation_And_Retained_Theme_Do_Not_Retain_A_Closed_Manager()
    {
        var (root, dictionary, manager, window) = CreateClosedReferences();
        for (int attempt = 0; attempt < 8; attempt++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            // Closed native windows can finish render cleanup asynchronously.
            // Let those completions run between collections under suite load.
            await Task.Delay(25);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        Assert.False(manager.TryGetTarget(out _));
        Assert.False(window.TryGetTarget(out _));
        GC.KeepAlive(root);
        GC.KeepAlive(dictionary);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (LayoutRoot, ResourceDictionary, WeakReference<DockingManager>, WeakReference<PinnedDockWindow>) CreateClosedReferences()
    {
        using Workspace workspace = new();
        var references = (workspace.Manager.Layout, workspace.Resources,
            new WeakReference<DockingManager>(workspace.Manager), new WeakReference<PinnedDockWindow>(workspace.Preview));
        workspace.Manager.Layout = new LayoutRoot();
        workspace.Update();
        return references;
    }

    private static void Click(Window window, Control control)
    {
        Point point = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), window)!.Value;
        window.MouseMove(point);
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private sealed class Workspace : IDisposable
    {
        private readonly bool _previous = DockSettings.UsePinnedDockWindow;
        internal TextBox Editor { get; } = new() { Text = "Native preview editor" };
        internal LayoutAnchorable Tool { get; }
        internal DockingManager Manager { get; }
        internal ResourceDictionary Resources { get; } = new() { ["Marker"] = "first" };
        internal Window Owner { get; }
        internal PinnedDockWindow Preview => Assert.Single(Owner.OwnedWindows.OfType<PinnedDockWindow>());

        internal Workspace(AnchorSide side = AnchorSide.Left)
        {
            DockSettings.UsePinnedDockWindow = true;
            Tool = new() { Title = "Preview", Content = Editor, AutoHideWidth = 220, AutoHideHeight = 190, CanClose = true };
            LayoutAnchorGroup group = new();
            group.Children.Add(Tool);
            LayoutDocumentPane documents = new(new LayoutDocument { Content = new TextBox { Text = "First document" } });
            documents.Children.Add(new LayoutDocument { Content = new TextBox { Text = "Second document" } });
            Manager = new()
            {
                AutoHideWindowClosingTimer = int.MaxValue,
                Theme = new TestDictionaryTheme(Resources),
                Layout = new() { RootPanel = new(documents) }
            };
            LayoutAnchorSide anchors = side switch
            {
                AnchorSide.Left => Manager.Layout.LeftSide!, AnchorSide.Right => Manager.Layout.RightSide!,
                AnchorSide.Top => Manager.Layout.TopSide!, _ => Manager.Layout.BottomSide!
            };
            anchors.Children.Add(group);
            Owner = new Window { Width = 950, Height = 650, Content = Manager };
            try { Owner.Show(); Update(); Show(); }
            catch { Owner.Close(); DockSettings.UsePinnedDockWindow = _previous; throw; }
        }

        internal void Show()
        {
            // The compatibility theme chooses an embedded child by default. These
            // extension tests explicitly opt into Dock's separate owned-window mode.
            foreach (PinnedDockControl host in Manager.GetVisualDescendants().OfType<PinnedDockControl>())
                host.UsePinnedDockWindow = true;
            Tool.IsSelected = true;
            Update();
        }
        internal void Update()
        {
            Owner.UpdateLayout();
            foreach (Window window in Owner.OwnedWindows.ToArray()) window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }
        public void Dispose()
        {
            try
            {
                Manager.DockFactory.HidePreviewingDockables(Manager.DockLayout);
                Owner.Close();
                Dispatcher.UIThread.RunJobs();
            }
            finally { DockSettings.UsePinnedDockWindow = _previous; }
        }
    }
}
