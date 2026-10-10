using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class ContainerStyleTests
{
    [AvaloniaFact]
    public void A_Derived_Manager_Renders_Native_Controls_And_Can_Observe_Style_Changes()
    {
        ContainerStyleFixture resources = new();
        StyledContentViewModel content = new("Derived manager");
        LayoutDocument document = new() { Content = content };
        StyleNotificationsManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(new LayoutDocumentPane(document)) } };
        manager.LayoutItemContainerStyleSelector = (StyleSelector)resources.Resources["Selector"]!;
        manager.LayoutItemContainerStyle = (ControlTheme)resources.Resources["BaseItemStyle"]!;
        Window window = new() { Content = manager, Width = 600, Height = 400 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Single(manager.GetVisualDescendants().OfType<DockControl>());
            Assert.Equal(content.Title, document.Title);
            Assert.Equal(1, manager.StyleChanges);
            Assert.Equal(1, manager.SelectorChanges);
        }
        finally { window.Close(); }
    }

    private sealed class StyleNotificationsManager : DockingManager
    {
        public int StyleChanges { get; private set; }
        public int SelectorChanges { get; private set; }
        protected override void OnLayoutItemContainerStyleChanged(AvaloniaPropertyChangedEventArgs e)
        {
            StyleChanges++;
            base.OnLayoutItemContainerStyleChanged(e);
        }
        protected override void OnLayoutItemContainerStyleSelectorChanged(AvaloniaPropertyChangedEventArgs e)
        {
            SelectorChanges++;
            base.OnLayoutItemContainerStyleSelectorChanged(e);
        }
    }

    [AvaloniaFact]
    public void A_Retained_ViewModel_Does_Not_Retain_An_Unused_Styled_Manager()
    {
        StyledContentViewModel content = new("Retained ViewModel");
        WeakReference<DockingManager> manager = CreateUnusedStyledManager(content);
        Dispatcher.UIThread.RunJobs();
        for (int i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        Assert.False(manager.TryGetTarget(out _));
        content.Title = "Updated without a manager";
        GC.KeepAlive(content);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<DockingManager> CreateUnusedStyledManager(StyledContentViewModel content)
    {
        ContainerStyleFixture resources = new();
        LayoutDocument document = new() { Content = content };
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(new LayoutDocumentPane(document)) } };
        manager.LayoutItemContainerStyle = (ControlTheme)resources.Resources["CommandStyle"]!;
        Assert.Equal(content.Title, document.Title);
        Assert.Same(content.CloseCommand, manager.GetLayoutItemFromModel(document)!.CloseCommand);
        return new(manager);
    }

    [AvaloniaFact]
    public void Compiled_Styles_Connect_ViewModel_Metadata_And_Capabilities_To_Native_Models()
    {
        StyledWorkspaceViewModel source = new();
        source.Documents[0].CanClose = false;
        source.Tools[0].CanFloat = false;
        source.Tools[0].CanHide = false;
        ContainerStyleFixture window = Open(source);
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            LayoutAnchorable tool = manager.Layout.Descendents().OfType<LayoutAnchorable>().Single();
            LayoutDocumentItem documentItem = (LayoutDocumentItem)manager.GetLayoutItemFromModel(document)!;
            LayoutAnchorableItem toolItem = (LayoutAnchorableItem)manager.GetLayoutItemFromModel(tool)!;
            Assert.Equal(source.Documents[0].Title, document.Title);
            Assert.Equal(source.Documents[0].Identifier, document.ContentId);
            Assert.Equal("Description: Styled document", document.Description);
            Assert.False(document.DockModel.CanClose);
            Assert.False(documentItem.CloseCommand!.CanExecute(null));
            Assert.False(tool.DockModel.CanFloat);
            Assert.False(tool.CanHide);
            Assert.False(toolItem.HideCommand!.CanExecute(null));
            Assert.Same(manager, documentItem.Parent);
            Assert.Same(manager, toolItem.Parent);
            Assert.Contains(documentItem, manager.GetLogicalChildren());

            source.Documents[0].Title = "Renamed in ViewModel";
            source.Tools[0].CanFloat = true;
            source.Tools[0].CanHide = true;
            Assert.Equal("Renamed in ViewModel", document.DockModel.Title);
            Assert.Equal("Description: Renamed in ViewModel", document.Description);
            Assert.True(tool.DockModel.CanFloat);
            Assert.True(tool.CanHide);
            Assert.True(toolItem.FloatCommand!.CanExecute(null));
            document.DockModel.CanClose = true;
            Assert.True(documentItem.CanClose);
            Assert.True(source.Documents[0].CanClose);
        }
        finally { Close(window, manager); }
    }

    [AvaloniaFact]
    public void Selector_Receives_Application_Content_And_The_Correct_Container_Type()
    {
        ContainerStyleFixture window = Open(new());
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            ContainerStyleSelector selector = (ContainerStyleSelector)manager.LayoutItemContainerStyleSelector!;
            Assert.True(selector.Calls >= 2);
            LayoutItem last = Assert.IsAssignableFrom<LayoutItem>(selector.LastContainer);
            Assert.Same(last.Model, selector.LastItem);
            foreach (LayoutContent content in manager.Layout.Descendents().OfType<LayoutContent>())
            {
                LayoutItem item = manager.GetLayoutItemFromModel(content)!;
                Assert.Same(content is LayoutDocument ? selector.DocumentStyle : selector.ToolStyle, item.Theme);
            }
        }
        finally { Close(window, manager); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Explicit_Style_Takes_Precedence_And_Resolves_Dynamic_Resources_While_Docked_Or_Floating(bool floating)
    {
        StyledWorkspaceViewModel source = new();
        ContainerStyleFixture window = Open(source);
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            LayoutItem item = manager.GetLayoutItemFromModel(document)!;
            if (floating) document.Float();
            Dispatcher.UIThread.RunJobs();
            ContainerStyleSelector selector = (ContainerStyleSelector)manager.LayoutItemContainerStyleSelector!;
            int calls = selector.Calls;
            manager.LayoutItemContainerStyle = (ControlTheme)window.Resources["ExplicitStyle"]!;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(calls, selector.Calls);
            Assert.Equal("Title from resource", document.Title);
            Assert.False(document.CanFloat);
            window.Resources["ContainerTitle"] = "Changed resource";
            Assert.Equal("Changed resource", document.Title);
            Assert.Same(item, manager.GetLayoutItemFromModel(document));
            Assert.Same(manager, item.Parent);
            manager.LayoutItemContainerStyle = null;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(source.Documents[0].Title, document.Title);
            Assert.True(document.CanFloat);
            Assert.Same(selector.DocumentStyle, item.Theme);
        }
        finally { Close(window, manager); }
    }

    [AvaloniaFact]
    public void Application_Commands_Are_Installed_By_Styles_And_Defaults_Return_When_Style_Changes()
    {
        StyledWorkspaceViewModel source = new();
        ContainerStyleFixture window = Open(source);
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            LayoutItem item = manager.GetLayoutItemFromModel(document)!;
            ICommand defaultClose = item.CloseCommand!;
            manager.LayoutItemContainerStyle = (ControlTheme)window.Resources["CommandStyle"]!;
            Assert.Same(source.Documents[0].CloseCommand, item.CloseCommand);
            item.CloseCommand!.Execute(null);
            Assert.Equal(1, source.Documents[0].CloseRequests);
            Assert.Same(manager.Layout, document.Root);
            manager.LayoutItemContainerStyle = null;
            Assert.Same(defaultClose, item.CloseCommand);
            Assert.True(((ICommand)source.Documents[0].CloseCommand).CanExecute(null));
            defaultClose.Execute(null);
            Dispatcher.UIThread.RunJobs();
            Assert.Null(document.Parent);
            Assert.Null(item.Parent);
            Assert.Null(item.LayoutElement);
            Assert.DoesNotContain(item, manager.GetLogicalChildren());
        }
        finally { Close(window, manager); }
    }

    [AvaloniaFact]
    public void Local_Application_Command_Values_Survive_Style_Replacement_And_Layout_Detachment()
    {
        StyledWorkspaceViewModel source = new();
        ContainerStyleFixture window = Open(source);
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            LayoutItem item = manager.GetLayoutItemFromModel(document)!;
            item.CloseCommand = source.Documents[0].CloseCommand;
            manager.LayoutItemContainerStyle = (ControlTheme)window.Resources["ExplicitStyle"]!;
            Assert.Same(source.Documents[0].CloseCommand, item.CloseCommand);
            manager.Layout = new LayoutRoot();
            Assert.Null(item.Parent);
            Assert.Null(item.Theme);
            Assert.Null(item.Model);
            Assert.Same(source.Documents[0].CloseCommand, item.CloseCommand);
            item.CloseCommand!.Execute(null);
            Assert.Equal(1, source.Documents[0].CloseRequests);
        }
        finally { Close(window, manager); }
    }

    [AvaloniaFact]
    public void Changing_Model_Type_Replaces_Compiled_Style_Bindings_And_Releases_The_Previous_Content()
    {
        StyledWorkspaceViewModel source = new();
        ContainerStyleFixture window = Open(source);
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            LayoutItem item = manager.GetLayoutItemFromModel(document)!;
            AlternateContent alternate = new("Different content type");
            document.Content = alternate;
            Assert.Same(alternate, item.Model);
            Assert.Equal(alternate.Description, document.Title);
            Assert.Same(window.Resources["AlternateStyle"], item.Theme);
            source.Documents[0].Title = "Old content changed";
            Assert.Equal(alternate.Description, document.Title);
            StyledContentViewModel replacement = new("Replacement styled content") { CanFloat = false };
            document.Content = replacement;
            Assert.Equal(replacement.Title, document.Title);
            Assert.Equal(replacement.Identifier, document.ContentId);
            Assert.False(document.CanFloat);
            replacement.Title = "Replacement renamed";
            Assert.Equal(replacement.Title, document.Title);
        }
        finally { Close(window, manager); }
    }

    [AvaloniaFact]
    public void Null_Selection_Removes_Previous_Style_Bindings_And_Keeps_The_Live_Model_State()
    {
        StyledWorkspaceViewModel source = new();
        ContainerStyleFixture window = Open(source);
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            LayoutItem item = manager.GetLayoutItemFromModel(document)!;
            string title = document.Title;
            manager.LayoutItemContainerStyleSelector = (StyleSelector)window.Resources["DefaultStyleSelector"]!;
            Assert.Null(item.Theme);
            Assert.Equal(title, document.Title);
            source.Documents[0].Title = "A detached binding";
            Assert.Equal(title, document.Title);
            document.Title = "Native title";
            Assert.Equal("Native title", item.Title);
            Assert.NotNull(item.CloseCommand);
        }
        finally { Close(window, manager); }
    }

    [AvaloniaFact]
    public void New_Source_Entries_And_Hidden_Tools_Receive_Container_Styles()
    {
        StyledWorkspaceViewModel source = new();
        ContainerStyleFixture window = Open(source);
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            source.Documents.Add(new("Added document") { CanClose = false });
            source.Tools.Add(new("Added tool") { CanHide = false });
            Dispatcher.UIThread.RunJobs();
            LayoutDocument added = manager.Layout.Descendents().OfType<LayoutDocument>().Single(document => ReferenceEquals(document.Content, source.Documents[1]));
            LayoutAnchorable tool = manager.Layout.Descendents().OfType<LayoutAnchorable>().Single(content => ReferenceEquals(content.Content, source.Tools[1]));
            Assert.Equal(source.Documents[1].Title, added.Title);
            Assert.False(added.CanClose);
            Assert.Equal(source.Tools[1].Title, tool.Title);
            Assert.False(tool.CanHide);
            LayoutAnchorable original = manager.Layout.Descendents().OfType<LayoutAnchorable>().Single(content => ReferenceEquals(content.Content, source.Tools[0]));
            original.Hide();
            Dispatcher.UIThread.RunJobs();
            source.Tools[0].Title = "Updated hidden tool";
            Assert.Equal(source.Tools[0].Title, original.Title);
            Assert.Same(manager, manager.GetLayoutItemFromModel(original)!.Parent);
        }
        finally { Close(window, manager); }
    }

    [AvaloniaFact]
    public void Restored_Layouts_Get_New_Styled_Containers_With_The_Original_ViewModels()
    {
        StyledWorkspaceViewModel source = new();
        ContainerStyleFixture window = Open(source);
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            LayoutDocument oldDocument = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            LayoutItem old = manager.GetLayoutItemFromModel(oldDocument)!;
            XmlLayoutSerializer serializer = new(manager);
            using StringWriter writer = new();
            serializer.Serialize(writer);
            serializer.Deserialize(new StringReader(writer.ToString()));
            Dispatcher.UIThread.RunJobs();
            LayoutDocument restored = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
            LayoutItem item = manager.GetLayoutItemFromModel(restored)!;
            Assert.NotSame(old, item);
            Assert.Null(old.Parent);
            Assert.Null(old.Theme);
            Assert.Same(source.Documents[0], item.Model);
            source.Documents[0].Title = "Restored title";
            Assert.Equal("Restored title", restored.Title);
            Assert.NotEqual("Restored title", oldDocument.Title);
        }
        finally { Close(window, manager); }
    }

    private static ContainerStyleFixture Open(StyledWorkspaceViewModel source)
    {
        ContainerStyleFixture window = new() { DataContext = source };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static void Close(ContainerStyleFixture window, DockingManager manager)
    {
        manager.Layout.FloatingWindows.Clear();
        window.Close();
    }
}
