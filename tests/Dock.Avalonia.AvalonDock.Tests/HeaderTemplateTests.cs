using System;
using System.IO;
using System.Linq;
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
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class HeaderTemplateTests
{
    [AvaloniaFact]
    public void Derived_Managers_Can_Observe_All_Header_Template_Changes()
    {
        HeaderTemplateFixture resources = new();
        HeaderNotificationsManager manager = new();
        manager.DocumentHeaderTemplate = (IDataTemplate)resources.Resources["DocumentHeader"]!;
        manager.AnchorableHeaderTemplate = (IDataTemplate)resources.Resources["ToolHeader"]!;
        manager.DocumentHeaderTemplateSelector = new DataTemplateSelector();
        manager.AnchorableHeaderTemplateSelector = new DataTemplateSelector();
        Assert.Equal(2, manager.DocumentChanges);
        Assert.Equal(2, manager.ToolChanges);
        Assert.Equal(1, manager.DocumentSelectorChanges);
        Assert.Equal(1, manager.ToolSelectorChanges);
        Assert.Null(manager.DocumentHeaderTemplate);
        Assert.Null(manager.AnchorableHeaderTemplate);
    }

    private sealed class HeaderNotificationsManager : DockingManager
    {
        public int DocumentChanges { get; private set; }
        public int ToolChanges { get; private set; }
        public int DocumentSelectorChanges { get; private set; }
        public int ToolSelectorChanges { get; private set; }
        protected override void OnDocumentHeaderTemplateChanged(AvaloniaPropertyChangedEventArgs e) => DocumentChanges++;
        protected override void OnAnchorableHeaderTemplateChanged(AvaloniaPropertyChangedEventArgs e) => ToolChanges++;
        protected override void OnDocumentHeaderTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e)
        {
            DocumentSelectorChanges++;
            base.OnDocumentHeaderTemplateSelectorChanged(e);
        }
        protected override void OnAnchorableHeaderTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e)
        {
            ToolSelectorChanges++;
            base.OnAnchorableHeaderTemplateSelectorChanged(e);
        }
    }

    [AvaloniaFact]
    public void Pointer_Input_On_Compiled_Headers_Selects_And_Activates_Native_Content()
    {
        HeaderTemplateFixture window = Open();
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            window.Width = 1100;
            LayoutDocument first = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            LayoutDocument second = new() { Title = "Second document", Content = new SourceContent("Second content") };
            ((LayoutDocumentPane)first.Parent!).Children.Add(second);
            first.IsActive = true;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Click(window, Header(manager, "document-header", second));
            Assert.True(second.IsActive);
            Assert.True(second.IsSelected);
            Assert.Same(second.DockModel, manager.DockLayout.FocusedDockable);
            LayoutAnchorable tool = manager.Layout.Descendents().OfType<LayoutAnchorable>().First();
            Click(window, Header(manager, "tool-header", tool));
            Assert.True(tool.IsActive);
            Assert.True(tool.IsSelected);
            Assert.Same(tool.DockModel, manager.DockLayout.FocusedDockable);
        }
        finally { Close(window, manager); }
    }

    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Native_Tool_Menu_Releases_Its_Button_On_Detach_And_Works_After_Reattachment(int menuChange)
    {
        LayoutAnchorablePane pane = new(new LayoutAnchorable { Title = "Menu lifetime" });
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(pane) } };
        ToolChromeControl chrome = new() { DataContext = pane.DockModel };
        Window window = new() { Content = chrome, Width = 500, Height = 300 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Button button = chrome.GetVisualDescendants().OfType<Button>().Single(control => control.Name == "PART_MenuButton");
            MenuFlyout? flyout = Assert.IsType<MenuFlyout>(button.Flyout);
            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            Assert.Null(button.Flyout);
            if (menuChange != 0)
            {
                flyout = menuChange == 1 ? new MenuFlyout() : null;
                chrome.ToolFlyout = flyout;
            }
            window.Content = chrome;
            Dispatcher.UIThread.RunJobs();
            Assert.Same(flyout, button.Flyout);
            if (flyout is not null)
            {
                Click(window, button);
                Assert.True(flyout.IsOpen);
                flyout.Hide();
            }
            GC.KeepAlive(manager);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Compiled_Document_And_Tool_Headers_Receive_Public_Models_And_Observe_Changes()
    {
        HeaderTemplateFixture window = Open();
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            LayoutAnchorable tool = manager.Layout.Descendents().OfType<LayoutAnchorable>().First();
            TextBlock documentHeader = Header(manager, "document-header");
            TextBlock toolHeader = Header(manager, "tool-header", tool);
            Assert.Same(document, documentHeader.DataContext);
            Assert.Same(tool, toolHeader.DataContext);
            Assert.Equal("Document: " + document.Title, documentHeader.Text);
            Assert.Equal("Tool: " + tool.Title, toolHeader.Text);
            Assert.IsType<DocumentTabStripItem>(documentHeader.GetVisualAncestors().OfType<DocumentTabStripItem>().Single());
            Assert.IsType<ToolTabStripItem>(toolHeader.GetVisualAncestors().OfType<ToolTabStripItem>().Single());
            document.Title = "Renamed document";
            tool.Title = "Renamed tool";
            Assert.Equal("Document: Renamed document", documentHeader.Text);
            Assert.Equal("Tool: Renamed tool", toolHeader.Text);
        }
        finally { Close(window, manager); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Runtime_Header_Changes_Reach_Native_Docked_And_Floating_Tabs(bool floating)
    {
        HeaderTemplateFixture window = Open();
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            LayoutAnchorable tool = manager.Layout.Descendents().OfType<LayoutAnchorable>().First();
            if (floating)
            {
                document.Float();
                manager.DockFactory.FloatAllDockables(tool.DockModel);
            }
            Dispatcher.UIThread.RunJobs();
            Control documentScope = floating ? Host(document) : manager;
            Control toolScope = floating ? Host(tool) : manager;
            TextBlock oldDocumentHeader = Header(documentScope, "document-header");
            TextBlock oldToolHeader = Header(toolScope, "tool-header", tool);
            document.ContentId = "document-id";
            tool.ContentId = "tool-id";
            manager.DocumentHeaderTemplate = (IDataTemplate)window.Resources["AlternateHeader"]!;
            manager.AnchorableHeaderTemplate = (IDataTemplate)window.Resources["AlternateHeader"]!;
            Dispatcher.UIThread.RunJobs();
            TextBlock newDocumentHeader = documentScope.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Text == "ID: document-id");
            TextBlock newToolHeader = toolScope.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Text == "ID: tool-id");
            Assert.NotSame(oldDocumentHeader, newDocumentHeader);
            Assert.NotSame(oldToolHeader, newToolHeader);
            Assert.Same(document, newDocumentHeader.DataContext);
            Assert.Same(tool, newToolHeader.DataContext);
        }
        finally { Close(window, manager); }
    }

    [AvaloniaFact]
    public void Header_Selectors_Take_Precedence_And_Receive_The_Layout_Content_And_Presenter()
    {
        HeaderTemplateFixture window = Open();
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            HeaderKindSelector selector = (HeaderKindSelector)window.Resources["Selector"]!;
            manager.DocumentHeaderTemplateSelector = selector;
            manager.AnchorableHeaderTemplateSelector = selector;
            Assert.Null(manager.DocumentHeaderTemplate);
            Assert.Null(manager.AnchorableHeaderTemplate);
            manager.DocumentHeaderTemplate = (IDataTemplate)window.Resources["AlternateHeader"]!;
            manager.AnchorableHeaderTemplate = (IDataTemplate)window.Resources["AlternateHeader"]!;
            Assert.Null(manager.DocumentHeaderTemplate);
            Assert.Null(manager.AnchorableHeaderTemplate);
            Dispatcher.UIThread.RunJobs();
            LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            LayoutAnchorable tool = manager.Layout.Descendents().OfType<LayoutAnchorable>().First();
            Assert.Contains(selector.Calls, call => ReferenceEquals(call.Item, document) &&
                call.Container is ContentPresenter view && ReferenceEquals(view.Child, Header(manager, "document-header")));
            Assert.Contains(selector.Calls, call => ReferenceEquals(call.Item, tool) &&
                call.Container is ContentPresenter view && ReferenceEquals(view.Child, Header(manager, "tool-header", tool)));
            Assert.DoesNotContain(manager.GetVisualDescendants().OfType<TextBlock>(), text => text.Classes.Contains("alternate-header"));
        }
        finally { Close(window, manager); }
    }

    [AvaloniaFact]
    public void A_Null_Selector_Result_Uses_The_Default_Title_Header()
    {
        HeaderTemplateFixture window = Open();
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            LayoutAnchorable tool = manager.Layout.Descendents().OfType<LayoutAnchorable>().First();
            manager.DocumentHeaderTemplateSelector = new DataTemplateSelector();
            manager.AnchorableHeaderTemplateSelector = new DataTemplateSelector();
            Dispatcher.UIThread.RunJobs();
            TextBlock documentHeader = manager.GetVisualDescendants().OfType<DocumentTabStripItem>().Single()
                .GetVisualDescendants().OfType<TextBlock>().Single(text => ReferenceEquals(text.DataContext, document));
            TextBlock toolHeader = manager.GetVisualDescendants().OfType<ToolTabStripItem>().Single(tab => ReferenceEquals(tab.DataContext, tool.DockModel))
                .GetVisualDescendants().OfType<TextBlock>().Single(text => ReferenceEquals(text.DataContext, tool));
            Assert.Equal(document.Title, documentHeader.Text);
            Assert.Equal(tool.Title, toolHeader.Text);
            document.Title = "Default header update";
            Assert.Equal(document.Title, documentHeader.Text);
        }
        finally { Close(window, manager); }
    }

    [AvaloniaFact]
    public void Auto_Hide_Headers_Use_The_Anchorable_Template_And_Keep_Native_Preview_Input()
    {
        HeaderTemplateFixture window = Open();
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            LayoutAnchorable tool = manager.Layout.Descendents().OfType<LayoutAnchorable>().First();
            tool.ToggleAutoHide();
            Dispatcher.UIThread.RunJobs();
            ToolPinItemControl pin = manager.GetVisualDescendants().OfType<ToolPinItemControl>().Single();
            TextBlock header = Header(pin, "tool-header");
            Assert.Same(tool, header.DataContext);
            Point point = header.TranslatePoint(new Point(header.Bounds.Width / 2, header.Bounds.Height / 2), window)!.Value;
            window.MouseDown(point, MouseButton.Left);
            window.MouseUp(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            Assert.Same(tool.DockModel, manager.DockLayout.PinnedDock?.ActiveDockable);
            Assert.True(tool.IsAutoHidden);
            manager.AnchorableHeaderTemplate = (IDataTemplate)window.Resources["AlternateHeader"]!;
            tool.ContentId = "auto-hidden";
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("ID: auto-hidden", Header(pin, "alternate-header").Text);
            tool.ToggleAutoHide();
            Dispatcher.UIThread.RunJobs();
            Assert.False(tool.IsAutoHidden);
            Assert.Same(tool, Header(manager, "alternate-header", tool).DataContext);
        }
        finally { Close(window, manager); }
    }

    [AvaloniaFact]
    public void A_Tool_Docked_As_A_Document_Uses_The_Document_Header_Selector_With_Its_Tool_Model()
    {
        HeaderTemplateFixture window = Open();
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            manager.DocumentHeaderTemplateSelector = (HeaderKindSelector)window.Resources["Selector"]!;
            LayoutAnchorable tool = manager.Layout.Descendents().OfType<LayoutAnchorable>().First();
            tool.DockAsDocument();
            Dispatcher.UIThread.RunJobs();
            DocumentTabStripItem tab = manager.GetVisualDescendants().OfType<DocumentTabStripItem>()
                .Single(item => ReferenceEquals(item.DataContext, tool.DockModel));
            Assert.Same(tool, Header(tab, "tool-header").DataContext);
        }
        finally { Close(window, manager); }
    }

    [AvaloniaFact]
    public void XML_Restoration_Releases_Old_Headers_And_Binds_The_New_Layout_Models()
    {
        HeaderTemplateFixture window = Open();
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            LayoutDocument oldDocument = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            ContentPresenter oldHeader = Presenter(Header(manager, "document-header"));
            XmlLayoutSerializer serializer = new(manager);
            using StringWriter writer = new();
            serializer.Serialize(writer);
            serializer.Deserialize(new StringReader(writer.ToString()));
            Dispatcher.UIThread.RunJobs();
            LayoutDocument restored = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            Assert.NotSame(oldDocument, restored);
            Assert.Null(oldHeader.Content);
            Assert.Null(oldHeader.Child);
            TextBlock header = Header(manager, "document-header");
            Assert.Same(restored, header.DataContext);
            restored.Title = "Restored title";
            Assert.Equal("Document: Restored title", header.Text);
        }
        finally { Close(window, manager); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Retaining_A_Closed_Header_Does_Not_Retain_Its_Layout_Or_Manager(bool retainHeader)
    {
        var result = CreateClosedHeader(retainHeader);
        Dispatcher.UIThread.RunJobs();
        for (int i = 0; i < 3; i++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        Assert.Null(result.Header.Content);
        Assert.Null(result.Header.Child);
        Assert.Null(result.Header.Parent);
        Assert.Null(result.Header.GetVisualParent());
        Assert.Null(result.Header.TemplatedParent);
        Assert.Null(result.Header.DataContext);
        Assert.False(result.Manager.TryGetTarget(out _));
        Assert.False(result.Document.TryGetTarget(out _));
        GC.KeepAlive(result.Header);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (ContentPresenter Header, WeakReference<DockingManager> Manager, WeakReference<LayoutDocument> Document) CreateClosedHeader(bool retainHeader)
    {
        HeaderTemplateFixture window = Open();
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
        ContentPresenter header = Presenter(Header(manager, "document-header"));
        document.Close();
        Dispatcher.UIThread.RunJobs();
        window.Close();
        return (retainHeader ? header : new ContentPresenter(), new(manager), new(document));
    }

    private static ContentPresenter Presenter(Control header) =>
        header.GetVisualAncestors().OfType<ContentPresenter>().First(view => ReferenceEquals(view.Child, header));
    private static void Click(Window window, Control control)
    {
        Point point = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), window)!.Value;
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }
    private static TextBlock Header(Control scope, string name, LayoutContent? model = null) =>
        scope.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Classes.Contains(name) &&
            (model is null || ReferenceEquals(text.DataContext, model)));
    private static HostWindow Host(LayoutContent content) =>
        (HostWindow)content.FindParent<LayoutFloatingWindow>()!.DockWindow.Host!;
    private static HeaderTemplateFixture Open()
    {
        SourceViewModel source = new();
        source.Tools.Add(new SourceContent("Second tool"));
        HeaderTemplateFixture window = new() { DataContext = source };
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        foreach (LayoutContent content in manager.Layout.Descendents().OfType<LayoutContent>())
            content.Title = ((SourceContent)content.Content!).Text;
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
    private static void Close(HeaderTemplateFixture window, DockingManager manager)
    {
        manager.Layout.FloatingWindows.Clear();
        window.Close();
    }
}
