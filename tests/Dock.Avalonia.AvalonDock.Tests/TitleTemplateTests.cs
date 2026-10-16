using System;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Avalonia.Controls;
using ReactiveUI.Reactive;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class TitleTemplateTests
{
    [AvaloniaFact]
    public void Derived_Managers_Observe_Title_Changes_And_Selector_Coercion()
    {
        TitleTemplateFixture resources = new();
        TitleNotificationsManager manager = new();
        Assert.Null(manager.AnchorableTitleTemplate);
        Assert.Null(manager.AnchorableTitleTemplateSelector);
        manager.AnchorableTitleTemplate = (IDataTemplate)resources.Resources["ToolTitle"]!;
        manager.AnchorableTitleTemplateSelector = new DataTemplateSelector();
        Assert.Equal(2, manager.TitleChanges);
        Assert.Equal(1, manager.SelectorChanges);
        Assert.Null(manager.AnchorableTitleTemplate);
        manager.AnchorableTitleTemplate = (IDataTemplate)resources.Resources["AlternateTitle"]!;
        Assert.Null(manager.AnchorableTitleTemplate);
        Assert.Equal(2, manager.TitleChanges);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Compiled_Title_Follows_Native_Selection_And_Metadata(bool floating)
    {
        using Workspace workspace = new(floating);
        TextBlock title = Title(workspace.Scope, "tool-title");
        Assert.Same(workspace.Tool, title.DataContext);
        Assert.Equal("Title: First", title.Text);
        Assert.Same(workspace.Tool.DockModel.Owner, title.GetVisualAncestors().OfType<ToolChromeControl>().Single().DataContext);
        workspace.Tool.Title = "Renamed";
        Assert.Equal("Title: Renamed", title.Text);
        TextBlock tab = workspace.Scope.GetVisualDescendants().OfType<ToolTabStripItem>()
            .Single(control => ReferenceEquals(control.DataContext, workspace.OtherTool.DockModel))
            .GetVisualDescendants().OfType<TextBlock>().Single(text => text.Classes.Contains("tool-tab"));
        Click(tab);
        Assert.True(workspace.OtherTool.IsSelected);
        Assert.True(workspace.OtherTool.IsActive);
        TextBlock selectedTitle = Title(workspace.Scope, "tool-title");
        Assert.NotSame(title, selectedTitle);
        Assert.Same(workspace.OtherTool, selectedTitle.DataContext);
        Assert.Equal("Title: Second", selectedTitle.Text);
        workspace.OtherTool.DockModel.Title = "Native change";
        Assert.Equal("Title: Native change", selectedTitle.Text);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Runtime_Title_Template_Replacement_Leaves_Tab_Headers_Intact(bool floating)
    {
        using Workspace workspace = new(floating);
        TextBlock previous = Title(workspace.Scope, "tool-title");
        TextBlock tab = workspace.Scope.GetVisualDescendants().OfType<TextBlock>().Single(text =>
            text.Classes.Contains("tool-tab") && ReferenceEquals(text.DataContext, workspace.Tool));
        workspace.Manager.AnchorableTitleTemplate = (IDataTemplate)workspace.Window.Resources["AlternateTitle"]!;
        workspace.Update();
        TextBlock replacement = Title(workspace.Scope, "alternate-title");
        Assert.NotSame(previous, replacement);
        Assert.Same(workspace.Tool, replacement.DataContext);
        Assert.Equal("ID: first", replacement.Text);
        Assert.Contains(tab, workspace.Scope.GetVisualDescendants());
        workspace.Tool.ContentId = "changed";
        Assert.Equal("ID: changed", replacement.Text);
        workspace.Manager.AnchorableTitleTemplate = null;
        workspace.Update();
        TextBlock fallback = DefaultTitle(workspace.Scope, workspace.Tool);
        Assert.Equal(workspace.Tool.Title, fallback.Text);
        Assert.Contains(tab, workspace.Scope.GetVisualDescendants());
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Title_Selector_Takes_Precedence_Receives_Selected_Model_And_Can_Fall_Back(bool floating)
    {
        using Workspace workspace = new(floating);
        HeaderKindSelector selector = (HeaderKindSelector)workspace.Window.Resources["Selector"]!;
        workspace.Manager.AnchorableTitleTemplateSelector = selector;
        workspace.Manager.AnchorableTitleTemplate = (IDataTemplate)workspace.Window.Resources["AlternateTitle"]!;
        workspace.Update();
        Assert.Null(workspace.Manager.AnchorableTitleTemplate);
        TextBlock title = Title(workspace.Scope, "tool-title");
        Assert.Contains(selector.Calls, call => ReferenceEquals(call.Item, workspace.Tool) &&
            call.Container is ContentPresenter presenter && ReferenceEquals(presenter.Child, title));
        workspace.OtherTool.IsActive = true;
        workspace.Update();
        Assert.Same(workspace.OtherTool.DockModel, ((Dock.Model.Core.IDock)workspace.Tool.DockModel.Owner!).ActiveDockable);
        TextBlock selected = Title(workspace.Scope, "tool-title");
        Assert.Same(workspace.OtherTool, selected.DataContext);
        Assert.Contains(selector.Calls, call => ReferenceEquals(call.Item, workspace.OtherTool) &&
            call.Container is ContentPresenter presenter && ReferenceEquals(presenter.Child, selected));
        workspace.Manager.AnchorableTitleTemplateSelector = new DataTemplateSelector();
        workspace.Update();
        Assert.Equal(workspace.OtherTool.Title, DefaultTitle(workspace.Scope, workspace.OtherTool).Text);
        workspace.Manager.AnchorableTitleTemplateSelector = null;
        workspace.Manager.AnchorableTitleTemplate = (IDataTemplate)workspace.Window.Resources["AlternateTitle"]!;
        workspace.Update();
        Assert.Equal("ID: second", Title(workspace.Scope, "alternate-title").Text);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Custom_Title_Keeps_Native_Context_Menu_And_Close_Button_Input(bool floating)
    {
        using Workspace workspace = new(floating);
        LayoutItem item = workspace.Manager.GetLayoutItemFromModel(workspace.Tool)!;
        Click(Title(workspace.Scope, "tool-title"), MouseButton.Right);
        ContextMenu menu = workspace.Manager.AnchorableContextMenu!;
        Assert.True(menu.IsOpen);
        Assert.Same(item, menu.DataContext);
        menu.Close();
        workspace.Update();
        int requests = 0;
        using var command = ReactiveCommand.Create(() => requests++, outputScheduler: ImmediateScheduler.Instance);
        item.CloseCommand = command;
        Button close = workspace.Scope.GetVisualDescendants().OfType<Button>()
            .Single(button => button.Name == "PART_CloseButton" && ReferenceEquals(button.DataContext, item));
        Click(close);
        Assert.Equal(1, requests);
        Assert.Same(workspace.Tool, Title(workspace.Scope, "tool-title").DataContext);
    }

    [AvaloniaFact]
    public void Auto_Hide_Preview_Uses_The_Current_Title_Template()
    {
        using Workspace workspace = new();
        workspace.Tool.ToggleAutoHide();
        workspace.Update();
        ToolPinItemControl pin = workspace.Manager.GetVisualDescendants().OfType<ToolPinItemControl>().Single();
        Click(pin);
        Assert.Same(workspace.Tool.DockModel, workspace.Manager.DockLayout.PinnedDock?.ActiveDockable);
        TextBlock title = Title(workspace.Manager, "tool-title", workspace.Tool);
        Assert.Equal("Title: First", title.Text);
        workspace.Manager.AnchorableTitleTemplate = (IDataTemplate)workspace.Window.Resources["AlternateTitle"]!;
        workspace.Update();
        Assert.Equal("ID: first", Title(workspace.Manager, "alternate-title", workspace.Tool).Text);
        workspace.Tool.ToggleAutoHide();
        workspace.Tool.IsActive = true;
        workspace.Update();
        Assert.False(workspace.Tool.IsAutoHidden);
        Assert.Equal("ID: first", Title(workspace.Manager, "alternate-title").Text);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void XML_Replacement_Releases_The_Previous_Title_And_Uses_Restored_Models(bool floating)
    {
        using Workspace workspace = new(floating);
        ContentPresenter previous = Presenter(Title(workspace.Scope, "tool-title"));
        XmlLayoutSerializer serializer = new(workspace.Manager);
        using StringWriter writer = new();
        serializer.Serialize(writer);
        serializer.Deserialize(new StringReader(writer.ToString()));
        workspace.Update();
        Assert.Null(previous.Content);
        Assert.Null(previous.Child);
        Assert.Null(previous.Parent);
        LayoutAnchorable restored = workspace.Manager.Layout.Descendents().OfType<LayoutAnchorable>()
            .Single(content => content.ContentId == "first");
        Control scope = floating ? (HostWindow)restored.FindParent<LayoutFloatingWindow>()!.DockWindow.Host! : workspace.Manager;
        Assert.NotSame(workspace.Tool, restored);
        Assert.Same(restored, Title(scope, "tool-title").DataContext);
        restored.Title = "Restored";
        Assert.Equal("Title: Restored", Title(scope, "tool-title").Text);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Retaining_A_Removed_Title_Does_Not_Retain_Its_Model_Or_Manager(bool floating)
    {
        var result = CreateRemovedTitle(floating);
        for (int i = 0; i < 3; i++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        Assert.Null(result.Title.Content);
        Assert.Null(result.Title.Child);
        Assert.Null(result.Title.Parent);
        Assert.Null(result.Title.GetVisualParent());
        Assert.Null(result.Title.DataContext);
        Assert.Null(result.Title.TemplatedParent);
        Assert.False(result.Manager.TryGetTarget(out _));
        Assert.False(result.Tool.TryGetTarget(out _));
        GC.KeepAlive(result.Title);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (ContentPresenter Title, WeakReference<DockingManager> Manager, WeakReference<LayoutAnchorable> Tool)
        CreateRemovedTitle(bool floating)
    {
        using Workspace workspace = new(floating);
        ContentPresenter title = Presenter(Title(workspace.Scope, "tool-title"));
        workspace.Source.Tools.Remove((SourceContent)workspace.Tool.Content!);
        workspace.Update();
        return (title, new(workspace.Manager), new(workspace.Tool));
    }

    private static ContentPresenter Presenter(Control title) =>
        title.GetVisualAncestors().OfType<ContentPresenter>().First(presenter => ReferenceEquals(presenter.Child, title));

    private static TextBlock Title(Control scope, string name, LayoutContent? model = null) =>
        scope.GetVisualDescendants().OfType<TextBlock>().Single(text => text.IsEffectivelyVisible && text.Classes.Contains(name) &&
            (model is null || ReferenceEquals(text.DataContext, model)));

    private static TextBlock DefaultTitle(Control scope, LayoutAnchorable model) =>
        scope.GetVisualDescendants().OfType<ContentPresenter>().Single(presenter => presenter.Name == "PART_Title")
            .GetVisualDescendants().OfType<TextBlock>().Single(text => ReferenceEquals(text.DataContext, model));

    private static void Click(Control control, MouseButton button = MouseButton.Left)
    {
        TopLevel top = TopLevel.GetTopLevel(control)!;
        top.UpdateLayout();
        Point point = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), top)!.Value;
        top.MouseMove(point);
        IInputElement? hit = top.InputHitTest(point);
        Assert.True(ReferenceEquals(hit, control) || hit is Visual visual && visual.GetVisualAncestors().Contains(control),
            $"Expected {control.GetType().Name} {control.Bounds} at {point}, hit {hit?.GetType().Name} {(hit as Control)?.Name}.");
        top.MouseDown(point, button);
        top.MouseUp(point, button);
        Dispatcher.UIThread.RunJobs();
    }

    private sealed class Workspace : IDisposable
    {
        public SourceViewModel Source { get; } = new();
        public TitleTemplateFixture Window { get; }
        public DockingManager Manager { get; }
        public LayoutAnchorable Tool { get; }
        public LayoutAnchorable OtherTool { get; }
        public LayoutAnchorablePane Pane { get; }
        public Control Scope { get; }

        public Workspace(bool floating = false)
        {
            Source.Tools.Clear();
            Source.Tools.Add(new("First"));
            Source.Tools.Add(new("Second"));
            Window = new() { DataContext = Source };
            Manager = Window.FindControl<DockingManager>("Manager")!;
            Tool = Manager.Layout.Descendents().OfType<LayoutAnchorable>().First();
            OtherTool = Manager.Layout.Descendents().OfType<LayoutAnchorable>().Last();
            Pane = (LayoutAnchorablePane)Tool.Parent!;
            Pane.DockWidth = new GridLength(400);
            Tool.Title = "First";
            OtherTool.Title = "Second";
            Tool.CanClose = true;
            OtherTool.CanClose = true;
            Tool.ContentId = "first";
            OtherTool.ContentId = "second";
            Tool.IsActive = true;
            Window.Show();
            Update();
            if (floating) Manager.DockFactory.FloatAllDockables(Tool.DockModel);
            Update();
            Scope = floating ? (HostWindow)Tool.FindParent<LayoutFloatingWindow>()!.DockWindow.Host! : Manager;
            if (Scope is HostWindow host) host.Width = 500;
            Update();
        }

        public void Update()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
            Scope?.UpdateLayout();
        }

        public void Dispose()
        {
            Manager.Layout.FloatingWindows.Clear();
            Window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private sealed class TitleNotificationsManager : DockingManager
    {
        public int TitleChanges { get; private set; }
        public int SelectorChanges { get; private set; }
        protected override void OnAnchorableTitleTemplateChanged(AvaloniaPropertyChangedEventArgs e) => TitleChanges++;
        protected override void OnAnchorableTitleTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e)
        {
            SelectorChanges++;
            base.OnAnchorableTitleTemplateSelectorChanged(e);
        }
    }
}
