using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class ContentTemplateTests
{
    [AvaloniaFact]
    public void A_Precreated_View_Transfers_From_An_Application_Panel_To_The_Native_Host()
    {
        TemplateFixture window = new() { DataContext = new SourceViewModel() };
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
        LayoutItem item = manager.GetLayoutItemFromModel(document)!;
        StackPanel panel = new();
        panel.Children.Add(item.View);
        Window preview = new() { Content = panel };
        try
        {
            preview.Show();
            Dispatcher.UIThread.RunJobs();
            AssertTemplate(item, "primary-template", ((SourceContent)document.Content!).Text);
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Empty(panel.Children);
            Assert.Contains(manager.GetVisualDescendants(), visual => ReferenceEquals(visual, item.View));
            AssertTemplate(item, "primary-template", ((SourceContent)document.Content!).Text);
        }
        finally
        {
            preview.Close();
            Close(window, manager);
        }
    }

    [AvaloniaFact]
    public void Protected_Change_Notifications_Do_Not_Replace_The_Template_Update_Behavior()
    {
        TemplateFixture resources = new();
        LayoutDocument document = new() { Content = new SourceContent("Extension point") };
        TemplateNotificationsManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(new LayoutDocumentPane(document)) } };
        ContentPresenter view = manager.GetLayoutItemFromModel(document)!.View;
        view.UpdateChild();
        manager.LayoutItemTemplate = (IDataTemplate)resources.Resources["Replacement"]!;
        view.UpdateChild();
        Assert.Contains("replacement-template", Assert.IsType<TextBlock>(view.Child).Classes);
        manager.LayoutItemTemplateSelector = (DataTemplateSelector)resources.Resources["Selector"]!;
        manager.LayoutItemTemplate = null;
        view.UpdateChild();
        Assert.Contains("primary-template", Assert.IsType<TextBlock>(view.Child).Classes);
        Assert.Equal(2, manager.TemplateChanges);
        Assert.Equal(1, manager.SelectorChanges);
    }

    private sealed class TemplateNotificationsManager : DockingManager
    {
        public int TemplateChanges { get; private set; }
        public int SelectorChanges { get; private set; }
        protected override void OnLayoutItemTemplateChanged(AvaloniaPropertyChangedEventArgs e) => TemplateChanges++;
        protected override void OnLayoutItemTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e) => SelectorChanges++;
    }

    [AvaloniaFact]
    public void A_Selected_Template_Recycles_Its_Control_When_The_Content_Type_Stays_The_Same()
    {
        TemplateFixture window = Open();
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            LayoutItem item = manager.GetLayoutItemFromModel(document)!;
            Control? child = item.View.Child;
            SourceContent replacement = new("Same template, new object");
            document.Content = replacement;
            Dispatcher.UIThread.RunJobs();
            Assert.Same(child, item.View.Child);
            Assert.Same(replacement, item.View.Child!.DataContext);
            AssertTemplate(item, "primary-template", replacement.Text);
        }
        finally { Close(window, manager); }
    }

    [AvaloniaFact]
    public void A_Retained_Empty_View_Does_Not_Retain_Closed_Models_Or_Their_Manager()
    {
        var (view, model, manager) = CreateClosedView();
        for (int i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        Assert.False(model.TryGetTarget(out _));
        Assert.False(manager.TryGetTarget(out _));
        Assert.Null(view.Content);
        Assert.Null(view.Child);
        GC.KeepAlive(view);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (ContentPresenter, WeakReference<LayoutDocument>, WeakReference<DockingManager>) CreateClosedView()
    {
        LayoutDocument document = new() { Content = "Temporary content" };
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(new LayoutDocumentPane(document)) } };
        ContentPresenter view = manager.GetLayoutItemFromModel(document)!.View;
        view.UpdateChild();
        document.Content = null;
        view.UpdateChild();
        document.Close();
        Dispatcher.UIThread.RunJobs();
        return (view, new(document), new(manager));
    }

    [AvaloniaFact]
    public void Selector_Receives_Application_Content_And_The_Actual_Native_Presenter()
    {
        TemplateFixture window = Open();
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            ContentKindSelector selector = (ContentKindSelector)manager.LayoutItemTemplateSelector!;
            Assert.True(selector.Calls >= 2);
            LayoutContent[] contents = manager.Layout.Descendents().OfType<LayoutContent>().ToArray();
            foreach (LayoutContent content in contents)
            {
                LayoutItem item = manager.GetLayoutItemFromModel(content)!;
                Assert.Contains(manager.GetVisualDescendants(), visual => ReferenceEquals(visual, item.View));
                AssertTemplate(item, "primary-template", ((SourceContent)content.Content!).Text);
            }
            Assert.Contains(contents, content => ReferenceEquals(content.Content, selector.LastItem));
            Assert.Contains(contents, content => ReferenceEquals(manager.GetLayoutItemFromModel(content)!.View, selector.LastContainer));
        }
        finally { Close(window, manager); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Explicit_Template_Has_Precedence_And_Runtime_Changes_Refresh_Docked_And_Floating_Content(bool floating)
    {
        TemplateFixture window = Open();
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            LayoutContent[] contents = manager.Layout.Descendents().OfType<LayoutContent>().ToArray();
            ContentPresenter[] presenters = contents.Select(content => manager.GetLayoutItemFromModel(content)!.View).ToArray();
            if (floating)
            {
                foreach (LayoutContent content in contents) content.Float();
                Dispatcher.UIThread.RunJobs();
            }
            ContentKindSelector selector = (ContentKindSelector)manager.LayoutItemTemplateSelector!;
            int calls = selector.Calls;
            manager.LayoutItemTemplate = (IDataTemplate)window.Resources["Replacement"]!;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(calls, selector.Calls);
            foreach (LayoutContent content in contents)
                AssertTemplate(manager.GetLayoutItemFromModel(content)!, "replacement-template", ((SourceContent)content.Content!).Text);
            manager.LayoutItemTemplate = null;
            Dispatcher.UIThread.RunJobs();
            for (int i = 0; i < contents.Length; i++)
            {
                LayoutItem item = manager.GetLayoutItemFromModel(contents[i])!;
                Assert.Same(presenters[i], item.View);
                AssertTemplate(item, "primary-template", ((SourceContent)contents[i].Content!).Text);
            }
            Assert.True(selector.Calls > calls);
            if (floating)
                foreach (LayoutFloatingWindow floatingWindow in manager.Layout.FloatingWindows)
                {
                    HostWindow host = Assert.IsAssignableFrom<HostWindow>(floatingWindow.DockWindow.Host);
                    Assert.Contains(host.GetVisualDescendants().OfType<TextBlock>(), block => block.Classes.Contains("primary-template") && block.Bounds.Width > 0);
                }
        }
        finally { Close(window, manager); }
    }

    [AvaloniaFact]
    public void Selector_Changes_And_Null_Results_Use_Manager_And_Ancestor_Templates()
    {
        TemplateFixture window = Open();
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            LayoutItem item = manager.GetLayoutItemFromModel(document)!;
            manager.LayoutItemTemplateSelector = (DataTemplateSelector)window.Resources["DefaultSelector"]!;
            Dispatcher.UIThread.RunJobs();
            AssertTemplate(item, "fallback-template", ((SourceContent)document.Content!).Text);
            manager.DataTemplates.Add((IDataTemplate)window.Resources["Replacement"]!);
            document.Content = new SourceContent("Manager scope");
            Dispatcher.UIThread.RunJobs();
            AssertTemplate(item, "replacement-template", "Manager scope");
            manager.DataTemplates.Clear();
            manager.LayoutItemTemplateSelector = (DataTemplateSelector)window.Resources["Selector"]!;
            document.Content = "Default selected content";
            Dispatcher.UIThread.RunJobs();
            AssertTemplate(item, "string-template", "Default selected content");
            manager.LayoutItemTemplateSelector = null;
            document.Content = new SourceContent("No selector");
            Dispatcher.UIThread.RunJobs();
            AssertTemplate(item, "fallback-template", "No selector");
        }
        finally { Close(window, manager); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Changing_Content_Type_Selects_A_New_Compiled_Template_Without_Reusing_Another_Templates_Control(bool floating)
    {
        TemplateFixture window = Open();
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            LayoutItem item = manager.GetLayoutItemFromModel(document)!;
            if (floating) document.Float();
            Dispatcher.UIThread.RunJobs();
            ContentPresenter presenter = item.View;
            Control? initial = presenter.Child;
            AlternateContent alternate = new("Another ViewModel type");
            document.Content = alternate;
            Dispatcher.UIThread.RunJobs();
            Assert.Same(presenter, item.View);
            TextBox editor = Assert.IsType<TextBox>(presenter.Child);
            Assert.Equal(alternate.Description, editor.Text);
            Assert.Contains("alternate-template", editor.Classes);
            Assert.Same(alternate, editor.DataContext);
            document.Content = new SourceContent("Back to the first type");
            Dispatcher.UIThread.RunJobs();
            AssertTemplate(item, "primary-template", "Back to the first type");
            Assert.NotSame(initial, presenter.Child);
            Assert.NotSame(editor, presenter.Child);
        }
        finally { Close(window, manager); }
    }

    [AvaloniaFact]
    public void Inline_Control_Identity_And_Input_Survive_Template_Changes_And_Float_Dock()
    {
        TemplateFixture window = Open();
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            manager.LayoutItemTemplateSelector = null;
            manager.AnchorablesSource = null;
            LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            TextBox editor = new() { Text = "Inline editor" };
            document.Content = editor;
            LayoutItem item = manager.GetLayoutItemFromModel(document)!;
            Dispatcher.UIThread.RunJobs();
            Assert.Same(editor, item.View.Child);
            manager.LayoutItemTemplateSelector = new DataTemplateSelector();
            Dispatcher.UIThread.RunJobs();
            Assert.Same(editor, item.View.Child);
            document.Float();
            Dispatcher.UIThread.RunJobs();
            Assert.Same(editor, item.View.Child);
            editor.Text = "Edited while floating";
            document.Dock();
            Dispatcher.UIThread.RunJobs();
            Assert.Same(item, manager.GetLayoutItemFromModel(document));
            Assert.Same(editor, item.View.Child);
            Assert.True(manager.GetVisualDescendants().Any(visual => ReferenceEquals(visual, editor)),
                $"Editor top-level: {TopLevel.GetTopLevel(editor)?.GetType().Name ?? "none"}; " +
                $"presenter parent: {item.View.GetVisualParent()?.GetType().Name ?? "none"}; " +
                $"selected: {document.IsSelected}; active: {document.IsActive}; floating: {manager.Layout.FloatingWindows.Count}.");
            manager.LayoutItemTemplate = (IDataTemplate)window.Resources["Inline"]!;
            Dispatcher.UIThread.RunJobs();
            AssertTemplate(item, "inline-template", "Edited while floating");
            manager.LayoutItemTemplate = null;
            Dispatcher.UIThread.RunJobs();
            Assert.Same(editor, item.View.Child);
            Assert.Equal("Edited while floating", editor.Text);
        }
        finally { Close(window, manager); }
    }

    [AvaloniaFact]
    public void Explicit_Template_Can_Display_Null_Content()
    {
        TemplateFixture window = Open();
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            manager.LayoutItemTemplate = (IDataTemplate)window.Resources["Null"]!;
            LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            document.Content = null;
            Dispatcher.UIThread.RunJobs();
            AssertTemplate(manager.GetLayoutItemFromModel(document)!, "null-template", "Empty content");
        }
        finally { Close(window, manager); }
    }

    [AvaloniaFact]
    public void Xml_Replacement_Releases_The_Old_View_And_Renders_Restored_Content_With_Current_Templates()
    {
        TemplateFixture window = Open();
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            document.ContentId = "document";
            object content = document.Content!;
            LayoutItem old = manager.GetLayoutItemFromModel(document)!;
            ContentPresenter oldView = old.View;
            document.Float();
            Dispatcher.UIThread.RunJobs();
            XmlLayoutSerializer serializer = new(manager);
            using StringWriter writer = new();
            serializer.Serialize(writer);
            serializer.Deserialize(new StringReader(writer.ToString()));
            Dispatcher.UIThread.RunJobs();
            LayoutDocument restored = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            LayoutItem current = manager.GetLayoutItemFromModel(restored)!;
            Assert.Null(old.LayoutElement);
            Assert.Null(oldView.Content);
            Assert.Null(oldView.ContentTemplate);
            Assert.Null(oldView.Child);
            Assert.NotSame(oldView, current.View);
            Assert.Same(content, current.Model);
            AssertTemplate(current, "primary-template", ((SourceContent)content).Text);
        }
        finally { Close(window, manager); }
    }

    private static TemplateFixture Open()
    {
        TemplateFixture window = new() { DataContext = new SourceViewModel() };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static void Close(TemplateFixture window, DockingManager manager)
    {
        manager.Layout.FloatingWindows.Clear();
        window.Close();
    }

    private static void AssertTemplate(LayoutItem item, string cssClass, string text)
    {
        TextBlock child = Assert.IsType<TextBlock>(item.View.Child);
        Assert.Contains(cssClass, child.Classes);
        Assert.Equal(text, child.Text);
        Assert.True(child.Bounds.Width > 0);
    }
}
