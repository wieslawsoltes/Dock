using System;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using ReactiveUI.Reactive;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class DocumentTitleTests
{
    [AvaloniaFact]
    public void Public_Title_Properties_Notify_Derived_Managers_And_Coerce_Explicit_Templates()
    {
        DocumentTitleFixture resources = new();
        TrackingManager manager = new();
        Assert.Null(manager.DocumentTitleTemplate);
        Assert.Null(manager.DocumentTitleTemplateSelector);
        manager.DocumentTitleTemplate = (IDataTemplate)resources.Resources["DocumentTitle"]!;
        manager.DocumentTitleTemplateSelector = new DataTemplateSelector();
        Assert.Null(manager.DocumentTitleTemplate);
        Assert.Equal(2, manager.TemplateChanges);
        Assert.Equal(1, manager.SelectorChanges);
        manager.DocumentTitleTemplate = (IDataTemplate)resources.Resources["AlternateTitle"]!;
        Assert.Null(manager.DocumentTitleTemplate);
        Assert.Equal(2, manager.TemplateChanges);
    }

    [AvaloniaFact]
    public void Compiled_Caption_Uses_Public_Root_Document_And_Observes_Native_Metadata()
    {
        using Workspace workspace = new();
        TextBlock title = Text(workspace.Host, "document-title");
        Assert.Equal("Window: First", title.Text);
        Assert.Same(workspace.Document, title.DataContext);
        Assert.Equal(OperatingSystem.IsWindows() ? WindowDecorations.Full : WindowDecorations.BorderOnly,
            workspace.Host.WindowDecorations);
        Assert.True(workspace.Host.ExtendClientAreaToDecorationsHint);
        Assert.False(workspace.Host.IsToolWindow);
        Assert.DoesNotContain(workspace.Manager.GetVisualDescendants().OfType<TextBlock>(), text => text.Classes.Contains("document-title"));
        workspace.Document.DockModel.Title = "Native rename";
        workspace.Update();
        Assert.Equal("Window: Native rename", title.Text);
        Assert.Equal("Tab: Native rename", Text(workspace.Host, "document-tab").Text);
        Assert.Equal("Native rename", workspace.Host.Title);
        Assert.True(title.Bounds.Width > 0 && title.Bounds.Height > 0);
    }

    [AvaloniaFact]
    public void Runtime_Title_Changes_Leave_Native_Tab_Headers_Intact()
    {
        using Workspace workspace = new();
        TextBlock tab = Text(workspace.Host, "document-tab");
        workspace.Manager.DocumentTitleTemplate = workspace.Template("AlternateTitle");
        workspace.Update();
        Assert.Equal("ID: first", Text(workspace.Host, "alternate-document-title").Text);
        workspace.Document.ContentId = "changed";
        Assert.Equal("ID: changed", Text(workspace.Host, "alternate-document-title").Text);
        Assert.Contains(tab, workspace.Host.GetVisualDescendants());
        workspace.Manager.DocumentTitleTemplate = null;
        workspace.Update();
        Assert.Equal("First", Text(workspace.Host, "avalondock-default-document-title").Text);
        Assert.Contains(tab, workspace.Host.GetVisualDescendants());
    }

    [AvaloniaFact]
    public void Selector_Receives_The_Document_And_Actual_Presenter_And_Can_Fall_Back()
    {
        using Workspace workspace = new();
        HeaderKindSelector selector = new() { DocumentTemplate = workspace.Template("AlternateTitle") };
        workspace.Manager.DocumentTitleTemplateSelector = selector;
        workspace.Manager.DocumentTitleTemplate = workspace.Template("DocumentTitle");
        workspace.Update();
        Assert.Null(workspace.Manager.DocumentTitleTemplate);
        TextBlock selected = Text(workspace.Host, "alternate-document-title");
        Assert.Contains(selector.Calls, call => ReferenceEquals(call.Item, workspace.Document) &&
            call.Container is ContentPresenter presenter && ReferenceEquals(presenter.Child, selected));
        workspace.Manager.DocumentTitleTemplateSelector = new DataTemplateSelector();
        workspace.Update();
        Assert.Equal("First", Text(workspace.Host, "avalondock-default-document-title").Text);
        workspace.Manager.DocumentTitleTemplateSelector = null;
        workspace.Manager.DocumentTitleTemplate = workspace.Template("DocumentTitle");
        workspace.Update();
        Assert.Equal("Window: First", Text(workspace.Host, "document-title").Text);
    }

    [AvaloniaFact]
    public void Caption_Maximize_And_Restore_Use_The_Native_Window_And_Persist_State()
    {
        using Workspace workspace = new();
        Button button = Button(workspace.Host, "avalondock-document-maximize");
        Click(button);
        Assert.Equal(WindowState.Maximized, workspace.Host.WindowState);
        Assert.Equal(DockWindowState.Maximized, workspace.Floating.DockWindow.WindowState);
        Assert.True(workspace.Document.IsMaximized);
        Click(button);
        Assert.Equal(WindowState.Normal, workspace.Host.WindowState);
        Assert.False(workspace.Document.IsMaximized);
        workspace.Host.CanResize = false;
        workspace.Update();
        Assert.False(button.IsEffectivelyEnabled);
        workspace.Host.CanResize = true;
        workspace.Update();
        Assert.True(button.IsEffectivelyEnabled);
    }

    [AvaloniaFact]
    public void Caption_Close_Uses_Current_Command_And_Honors_Enablement_And_Cancellation()
    {
        using Workspace workspace = new();
        LayoutDocumentFloatingWindow floating = Assert.IsType<LayoutDocumentFloatingWindow>(workspace.Document.Parent);
        LayoutItem item = workspace.Manager.GetLayoutItemFromModel(workspace.Document)!;
        ICommand original = item.CloseCommand!;
        int calls = 0;
        using BehaviorSubject<bool> enabled = new(false);
        using var command = ReactiveCommand.Create(() => calls++, enabled, outputScheduler: ImmediateScheduler.Instance);
        item.CloseCommand = command;
        Button button = Button(workspace.Host, "avalondock-document-window-close");
        Assert.False(button.IsEffectivelyEnabled);
        enabled.OnNext(true);
        workspace.Update();
        Click(button);
        Assert.Equal(1, calls);
        Assert.True(workspace.Host.IsVisible);
        IDock pane = (IDock)workspace.Document.DockModel.Owner!;
        pane.DockCapabilityPolicy = new DockCapabilityPolicy { CanClose = false };
        workspace.Update();
        Assert.False(button.IsVisible);
        pane.DockCapabilityPolicy = null;
        workspace.Document.CanClose = false;
        Assert.False(button.IsVisible);
        workspace.Document.CanClose = true;
        item.CloseCommand = original;
        bool cancel = true;
        workspace.Document.Closing += (_, e) => e.Cancel = cancel;
        Click(button);
        Assert.True(workspace.Host.IsVisible);
        cancel = false;
        Click(button);
        Assert.False(workspace.Host.IsVisible);
        Assert.Same(floating, workspace.Document.Parent);
        Assert.Null(workspace.Document.Root);
    }

    [AvaloniaFact]
    public void Caption_Opens_The_Managers_Document_Context_Menu()
    {
        using Workspace workspace = new();
        LayoutItem item = workspace.Manager.GetLayoutItemFromModel(workspace.Document)!;
        Click(Text(workspace.Host, "document-title"), MouseButton.Right);
        ContextMenu menu = workspace.Manager.DocumentContextMenu!;
        Assert.True(menu.IsOpen);
        Assert.Same(item, menu.DataContext);
        menu.Close();
        Assert.Null(menu.DataContext);
        Assert.Null(menu.PlacementTarget);
    }

    [AvaloniaFact]
    public void Root_Document_Replacement_Updates_The_Title_And_Retains_The_Old_Item_Until_Detachment()
    {
        using Workspace workspace = new();
        ContentPresenter previous = Presenter(Text(workspace.Host, "document-title"));
        LayoutDocument replacement = new() { Title = "Replacement", ContentId = "replacement", Content = new SourceContent("New content") };
        workspace.Floating.RootDocument = replacement;
        workspace.Update();
        // Upstream retains the former root document's public parent/root and
        // only collects its item after LayoutElement.Root leaves the manager.
        Assert.Same(workspace.Floating, workspace.Document.Parent);
        Assert.Same(workspace.Document, previous.Content);
        Assert.Same(replacement, Text(workspace.Host, "document-title").DataContext);
        Assert.Equal("Window: Replacement", Text(workspace.Host, "document-title").Text);
        workspace.Document.Parent = null;
        workspace.Update();
        Assert.Null(previous.Content);
        Assert.Null(previous.Child);
    }

    [AvaloniaFact]
    public void XML_Replacement_Closes_The_Old_Host_And_Uses_The_Restored_Document()
    {
        using Workspace workspace = new();
        ContentPresenter previous = Presenter(Text(workspace.Host, "document-title"));
        XmlLayoutSerializer serializer = new(workspace.Manager);
        using StringWriter writer = new();
        serializer.Serialize(writer);
        serializer.Deserialize(new StringReader(writer.ToString()));
        workspace.Update();
        Assert.False(workspace.Host.IsVisible);
        Assert.Null(previous.Content);
        Assert.Null(previous.Child);
        LayoutDocumentFloatingWindow restored = workspace.Manager.Layout.FloatingWindows.OfType<LayoutDocumentFloatingWindow>().Single();
        HostWindow host = (HostWindow)restored.DockWindow.Host!;
        Assert.NotSame(workspace.Document, restored.RootDocument);
        Assert.Same(restored.RootDocument, Text(host, "document-title").DataContext);
    }

    [AvaloniaFact]
    public void Reattachment_Uses_New_Caption_Commands_And_Current_Templates()
    {
        using Workspace workspace = new();
        // Keep the source scope stable while the manager has no logical parent.
        workspace.Manager.DataContext = workspace.Source;
        ICommand oldCommand = Button(workspace.Host, "avalondock-document-maximize").Command!;
        workspace.Window.Content = null;
        workspace.Update();
        Assert.False(workspace.Host.IsVisible);
        workspace.Manager.DocumentTitleTemplate = workspace.Template("AlternateTitle");
        workspace.Window.Content = workspace.Manager;
        workspace.Update();
        HostWindow host = (HostWindow)workspace.Floating.DockWindow.Host!;
        Assert.NotSame(workspace.Host, host);
        Assert.Equal("ID: first", Text(host, "alternate-document-title").Text);
        Button maximize = Button(host, "avalondock-document-maximize");
        Assert.NotSame(oldCommand, maximize.Command);
        Click(maximize);
        Assert.Equal(WindowState.Maximized, host.WindowState);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Caption_Drag_Uses_Native_Window_Lifecycle_And_Cancellation(bool cancel)
    {
        using Workspace workspace = new();
        int began = 0;
        int ended = 0;
        workspace.Manager.DockFactory.WindowMoveDragBegin += (_, e) =>
        {
            Assert.Same(workspace.Floating.DockWindow, e.Window);
            began++;
            e.Cancel = cancel;
        };
        workspace.Manager.DockFactory.WindowMoveDragEnd += (_, _) => ended++;
        Control title = Text(workspace.Host, "document-title");
        Point start = title.TranslatePoint(new Point(5, title.Bounds.Height / 2), workspace.Host)!.Value;
        Point end = start + new Vector(20, 0);
        workspace.Host.MouseMove(start);
        workspace.Host.MouseDown(start, MouseButton.Left);
        workspace.Host.MouseMove(end, RawInputModifiers.LeftMouseButton);
        Assert.Equal(1, began);
        Assert.Equal(!cancel, workspace.Host.Classes.Contains(":dragging"));
        Assert.Equal(!cancel, Assert.IsType<global::AvalonDock.Controls.LayoutDocumentFloatingWindowControl>(workspace.Host).IsDragging);
        workspace.Host.MouseUp(end, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(cancel ? 0 : 1, ended);
        Assert.DoesNotContain(":dragging", workspace.Host.Classes);
        Assert.False(((global::AvalonDock.Controls.LayoutFloatingWindowControl)workspace.Host).IsDragging);
    }

    [AvaloniaFact]
    public void Caption_Buttons_Do_Not_Start_Window_Dragging()
    {
        using Workspace workspace = new();
        int began = 0;
        workspace.Manager.DockFactory.WindowMoveDragBegin += (_, e) => { began++; e.Cancel = true; };
        Button button = Button(workspace.Host, "avalondock-document-maximize");
        Point start = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), workspace.Host)!.Value;
        Point end = start - new Vector(25, 0);
        workspace.Host.MouseMove(start);
        workspace.Host.MouseDown(start, MouseButton.Left);
        workspace.Host.MouseMove(end, RawInputModifiers.LeftMouseButton);
        workspace.Host.MouseUp(end, MouseButton.Left);
        Assert.Equal(0, began);
    }

    [AvaloniaFact]
    public void Hover_After_A_Caption_Click_Does_Not_Start_A_Window_Drag()
    {
        using Workspace workspace = new();
        int began = 0;
        workspace.Manager.DockFactory.WindowMoveDragBegin += (_, e) => { began++; e.Cancel = true; };
        Control title = Text(workspace.Host, "document-title");
        Click(title);
        Point point = title.TranslatePoint(new Point(title.Bounds.Width / 2 + 20, title.Bounds.Height / 2), workspace.Host)!.Value;
        workspace.Host.MouseMove(point);
        Assert.Equal(0, began);
    }

    [AvaloniaFact]
    public void Caption_Double_Click_Maximizes_Without_Starting_A_Drag()
    {
        using Workspace workspace = new();
        Control title = Text(workspace.Host, "document-title");
        Click(title);
        Click(title);
        Assert.Equal(WindowState.Maximized, workspace.Host.WindowState);
        Assert.DoesNotContain(":dragging", workspace.Host.Classes);
    }

    [AvaloniaFact]
    public void Retained_Title_And_Caption_Command_Release_Removed_Models_And_Closed_Hosts()
    {
        var result = RemoveTitle();
        for (int i = 0; i < 3; i++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        Assert.Null(result.Title.Content);
        Assert.Null(result.Title.Child);
        Assert.Null(result.Title.DataContext);
        Assert.Null(result.Title.Parent);
        Assert.Null(result.Title.TemplatedParent);
        Assert.Null(result.Title.GetVisualParent());
        Assert.False(result.Document.TryGetTarget(out _));
        Assert.False(result.Manager.TryGetTarget(out _));
        Assert.False(result.Host.TryGetTarget(out _));
        GC.KeepAlive(result.Title);
        GC.KeepAlive(result.Command);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (ContentPresenter Title, ICommand Command, WeakReference<LayoutDocument> Document,
        WeakReference<DockingManager> Manager, WeakReference<HostWindow> Host) RemoveTitle()
    {
        using Workspace workspace = new();
        ContentPresenter title = Presenter(Text(workspace.Host, "document-title"));
        ICommand command = Button(workspace.Host, "avalondock-document-maximize").Command!;
        workspace.Source.Documents.Remove((SourceContent)workspace.Document.Content!);
        workspace.Update();
        workspace.Window.CaptureRenderedFrame()?.Dispose();
        return (title, command, new(workspace.Document), new(workspace.Manager), new(workspace.Host));
    }

    private sealed class TrackingManager : DockingManager
    {
        public int TemplateChanges { get; private set; }
        public int SelectorChanges { get; private set; }
        protected override void OnDocumentTitleTemplateChanged(AvaloniaPropertyChangedEventArgs e) => TemplateChanges++;
        protected override void OnDocumentTitleTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.OnDocumentTitleTemplateSelectorChanged(e);
            SelectorChanges++;
        }
    }

    private static TextBlock Text(Control scope, string name) => scope.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Classes.Contains(name));
    private static Button Button(Control scope, string name) => scope.GetVisualDescendants().OfType<Button>().Single(button => button.Classes.Contains(name));
    private static ContentPresenter Presenter(Control child) => child.GetVisualAncestors().OfType<ContentPresenter>().First(presenter => ReferenceEquals(presenter.Child, child));
    private static void Click(Control control, MouseButton button = MouseButton.Left)
    {
        TopLevel host = TopLevel.GetTopLevel(control)!;
        host.UpdateLayout();
        Point point = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), host)!.Value;
        host.MouseMove(point);
        IInputElement? hit = host.InputHitTest(point);
        Assert.True(ReferenceEquals(hit, control) || hit is Visual visual && visual.GetVisualAncestors().Contains(control));
        host.MouseDown(point, button);
        host.MouseUp(point, button);
        Dispatcher.UIThread.RunJobs();
    }

    private sealed class Workspace : IDisposable
    {
        public SourceViewModel Source { get; } = new();
        public DocumentTitleFixture Window { get; }
        public DockingManager Manager { get; }
        public LayoutDocument Document { get; }
        public LayoutDocumentFloatingWindow Floating { get; }
        public HostWindow Host { get; }
        public Workspace()
        {
            Source.Documents.Clear();
            Source.Documents.Add(new("First"));
            Source.Documents.Add(new("Second"));
            Window = new() { DataContext = Source };
            Manager = Window.FindControl<DockingManager>("Manager")!;
            Document = Manager.Layout.Descendents().OfType<LayoutDocument>().First();
            Document.Title = "First";
            Document.ContentId = "first";
            Window.Show();
            Update();
            Document.Float();
            Floating = (LayoutDocumentFloatingWindow)Document.Parent!;
            Host = (HostWindow)Floating.DockWindow.Host!;
            Update();
        }
        public IDataTemplate Template(string key) => (IDataTemplate)Window.Resources[key]!;
        public void Update()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
            foreach (LayoutFloatingWindow floating in Manager.Layout.FloatingWindows)
                if (floating.DockWindow.Host is HostWindow host) host.UpdateLayout();
        }
        public void Dispose()
        {
            Manager.Layout.FloatingWindows.Clear();
            Window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
