using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Model.Avalonia.Controls;
using ReactiveUI.Reactive;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class LayoutItemTests
{
    [AvaloniaFact]
    public void Manager_Exposes_Typed_Stable_Containers_Only_For_Its_Own_Content()
    {
        LayoutDocument document = new();
        LayoutAnchorable tool = new();
        DockingManager manager = CreateManager(document, tool);
        LayoutDocumentItem documentItem = Assert.IsType<LayoutDocumentItem>(manager.GetLayoutItemFromModel(document));
        LayoutAnchorableItem toolItem = Assert.IsType<LayoutAnchorableItem>(manager.GetLayoutItemFromModel(tool));
        Assert.Same(document, documentItem.LayoutElement);
        Assert.Same(tool, toolItem.LayoutElement);
        Assert.Same(documentItem, manager.GetLayoutItemFromModel(document));
        Assert.Same(toolItem, manager.GetLayoutItemFromModel(tool));
        Assert.Null(manager.GetLayoutItemFromModel(new LayoutDocument()));
        Assert.Null(new DockingManager().GetLayoutItemFromModel(document));
    }

    [AvaloniaFact]
    public void Metadata_Changes_Propagate_Between_Containers_Models_And_Native_Dock()
    {
        object content = new();
        LayoutDocument document = new() { Title = "Before", ContentId = "before", Content = content, Description = "Details" };
        DockingManager manager = CreateManager(document);
        LayoutDocumentItem item = Assert.IsType<LayoutDocumentItem>(manager.GetLayoutItemFromModel(document));
        Assert.Equal("Before", item.Title);
        Assert.Null(item.ContentId);
        Assert.Equal("Details", item.Description);
        Assert.Same(content, item.Model);
        Assert.Same(item, item.DataContext);

        item.Title = "After";
        item.ContentId = "after";
        item.Description = "New details";
        item.CanClose = false;
        item.CanFloat = false;
        DrawingImage icon = new();
        item.IconSource = icon;
        ToolTip.SetTip(item, "Tip");
        Assert.Equal("After", document.DockModel.Title);
        Assert.Equal("after", document.DockModel.Id);
        Assert.Equal("New details", document.Description);
        Assert.False(document.DockModel.CanClose);
        Assert.False(document.DockModel.CanFloat);
        Assert.Same(icon, document.IconSource);
        Assert.Equal("Tip", document.ToolTip);

        object replacement = new();
        ((Document)document.DockModel).Content = replacement;
        document.DockModel.Title = "Native change";
        document.DockModel.CanClose = true;
        document.DockModel.CanFloat = true;
        document.Description = "Model change";
        Assert.Same(replacement, item.Model);
        Assert.Equal("Native change", item.Title);
        Assert.True(item.CanClose);
        Assert.True(item.CanFloat);
        Assert.Equal("Model change", item.Description);
    }

    [AvaloniaFact]
    public void Selection_And_Activation_Follow_The_Live_Pane()
    {
        LayoutDocument first = new() { ContentId = "first" };
        LayoutDocument second = new() { ContentId = "second" };
        DockingManager manager = CreateManager(first);
        ((LayoutDocumentPane)first.Parent!).Children.Add(second);
        LayoutItem firstItem = manager.GetLayoutItemFromModel(first)!;
        LayoutItem secondItem = manager.GetLayoutItemFromModel(second)!;
        secondItem.IsSelected = true;
        Execute(secondItem.ActivateCommand);
        Assert.Same(second, manager.Layout.ActiveContent);
        Assert.True(secondItem.IsActive);
        Assert.True(second.IsSelected);
        first.IsActive = true;
        Assert.True(firstItem.IsActive);
        Assert.False(secondItem.IsActive);
        Assert.Same(first, manager.Layout.ActiveContent);
    }

    [AvaloniaFact]
    public void Close_Command_Observes_Native_Capability_And_Manager_Cancellation()
    {
        LayoutDocument document = new();
        DockingManager manager = CreateManager(document);
        LayoutItem item = manager.GetLayoutItemFromModel(document)!;
        ICommand close = item.CloseCommand!;
        int changes = 0;
        close.CanExecuteChanged += (_, _) => changes++;
        document.DockModel.CanClose = false;
        Assert.False(close.CanExecute(null));
        Assert.Same(manager.Layout, document.Root);
        document.CanClose = true;
        Assert.True(close.CanExecute(null));
        EventHandler<DocumentClosingEventArgs> veto = (_, e) => e.Cancel = true;
        manager.DocumentClosing += veto;
        close.Execute(null);
        Assert.Same(document, item.LayoutElement);
        Assert.Same(manager.Layout, document.Root);
        manager.DocumentClosing -= veto;
        close.Execute(null);
        Dispatcher.UIThread.RunJobs();
        Assert.Null(document.Root);
        Assert.Null(item.LayoutElement);
        Assert.Null(item.Model);
        Assert.Null(item.CloseCommand);
        Assert.False(close.CanExecute(null));
        Assert.True(changes >= 3);
    }

    [AvaloniaFact]
    public void Container_And_Custom_Command_Survive_Native_Floating_And_Returning()
    {
        LayoutAnchorable tool = new() { CanClose = true };
        DockingManager manager = CreateManager(new LayoutDocument(), tool);
        LayoutAnchorableItem item = (LayoutAnchorableItem)manager.GetLayoutItemFromModel(tool)!;
        object native = tool.DockModel;
        using var custom = ReactiveCommand.Create(() => { }, outputScheduler: ImmediateScheduler.Instance);
        item.CloseCommand = custom;
        Execute(item.FloatCommand);
        Dispatcher.UIThread.RunJobs();
        Assert.True(tool.IsFloating);
        Assert.False(item.FloatCommand!.CanExecute(null));
        Assert.True(item.DockCommand!.CanExecute(null));
        Assert.False(item.AutoHideCommand!.CanExecute(null));
        Assert.Same(item, manager.GetLayoutItemFromModel(tool));
        Execute(item.DockCommand);
        Dispatcher.UIThread.RunJobs();
        Assert.False(tool.IsFloating);
        Assert.Same(native, tool.DockModel);
        Assert.Same(item, manager.GetLayoutItemFromModel(tool));
        Assert.Same(custom, item.CloseCommand);
        Assert.False(item.DockCommand.CanExecute(null));
    }

    [AvaloniaFact]
    public void Tool_Commands_Honor_Hide_Cancellation_Capabilities_And_AutoHide_State()
    {
        LayoutAnchorable tool = new();
        DockingManager manager = CreateManager(new LayoutDocument(), tool);
        LayoutAnchorableItem item = (LayoutAnchorableItem)manager.GetLayoutItemFromModel(tool)!;
        item.CanHide = false;
        Assert.False(tool.CanHide);
        Assert.False(item.HideCommand!.CanExecute(null));
        Assert.True(tool.IsVisible);
        tool.CanHide = true;
        Assert.True(item.CanHide);
        EventHandler<CancelEventArgs> veto = (_, e) => e.Cancel = true;
        tool.Hiding += veto;
        Execute(item.HideCommand);
        Assert.True(item.IsVisible);
        tool.Hiding -= veto;
        Execute(item.AutoHideCommand);
        Dispatcher.UIThread.RunJobs();
        Assert.True(tool.IsAutoHidden);
        Assert.Same(item, manager.GetLayoutItemFromModel(tool));
        Execute(item.AutoHideCommand);
        Assert.False(tool.IsAutoHidden);
        Execute(item.HideCommand);
        Dispatcher.UIThread.RunJobs();
        Assert.True(tool.IsHidden);
        Assert.False(item.IsVisible);
        Assert.False(item.AutoHideCommand!.CanExecute(null));
        item.IsVisible = true;
        Assert.True(tool.IsVisible);
        Assert.Same(item, manager.GetLayoutItemFromModel(tool));
        tool.CanAutoHide = false;
        Assert.False(item.AutoHideCommand.CanExecute(null));
    }

    [AvaloniaFact]
    public void Tool_DockAsDocument_Command_Respects_Capability_And_Uses_Native_Model()
    {
        LayoutAnchorable tool = new() { CanDockAsTabbedDocument = false };
        DockingManager manager = CreateManager(new LayoutDocument(), tool);
        LayoutItem item = manager.GetLayoutItemFromModel(tool)!;
        Assert.False(item.DockAsDocumentCommand!.CanExecute(null));
        tool.CanDockAsTabbedDocument = true;
        Execute(item.DockAsDocumentCommand);
        Assert.IsType<LayoutDocumentPane>(tool.Parent);
        Assert.IsType<DocumentDock>(tool.DockModel.Owner);
        Assert.False(item.DockAsDocumentCommand.CanExecute(null));
        Assert.Same(item, manager.GetLayoutItemFromModel(tool));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Close_Document_Set_Uses_Custom_Commands_Capabilities_And_Cancellation(bool excludeCurrent)
    {
        LayoutDocument selected = new();
        LayoutDocument ordinary = new();
        LayoutDocument vetoed = new();
        LayoutDocument protectedDocument = new() { CanClose = false };
        LayoutDocument customized = new();
        LayoutAnchorable tool = new() { CanClose = true };
        DockingManager manager = CreateManager(selected, tool);
        LayoutDocumentPane pane = (LayoutDocumentPane)selected.Parent!;
        foreach (LayoutDocument document in new[] { ordinary, vetoed, protectedDocument, customized }) pane.Children.Add(document);
        vetoed.Closing += (_, e) => e.Cancel = true;
        int calls = 0;
        using var command = ReactiveCommand.Create(() => calls++, outputScheduler: ImmediateScheduler.Instance);
        manager.GetLayoutItemFromModel(customized)!.CloseCommand = command;
        manager.GetLayoutItemFromModel(protectedDocument)!.CloseCommand = command;
        manager.GetLayoutItemFromModel(ordinary)!.CloseCommand = null;
        LayoutItem item = manager.GetLayoutItemFromModel(selected)!;
        Execute(excludeCurrent ? item.CloseAllButThisCommand : item.CloseAllCommand);
        Assert.Equal(1, calls);
        Assert.Null(ordinary.Root);
        Assert.Equal(excludeCurrent, selected.Root is not null);
        Assert.Same(manager.Layout, vetoed.Root);
        Assert.Same(manager.Layout, protectedDocument.Root);
        Assert.Same(manager.Layout, customized.Root);
        Assert.Same(manager.Layout, tool.Root);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Document_Group_Commands_Split_And_Move_Using_Existing_Native_Content(bool horizontal)
    {
        LayoutDocument first = new();
        LayoutDocument second = new();
        LayoutDocument third = new();
        DockingManager manager = CreateManager(first);
        LayoutDocumentPane original = (LayoutDocumentPane)first.Parent!;
        original.Children.Add(second);
        original.Children.Add(third);
        LayoutItem item = manager.GetLayoutItemFromModel(second)!;
        ICommand split = (horizontal ? item.NewHorizontalTabGroupCommand : item.NewVerticalTabGroupCommand)!;
        second.CanMove = false;
        Assert.False(split.CanExecute(null));
        second.CanMove = true;
        Execute(split);
        Dispatcher.UIThread.RunJobs();
        LayoutDocumentPane destination = Assert.IsType<LayoutDocumentPane>(second.Parent);
        LayoutDocumentPaneGroup group = Assert.IsType<LayoutDocumentPaneGroup>(destination.Parent);
        Assert.Equal(horizontal ? Orientation.Vertical : Orientation.Horizontal, group.Orientation);
        Assert.Equal(new[] { original, destination }, group.Children);
        Assert.Same(second.DockModel, Assert.Single(((DocumentDock)destination.DockModel).VisibleDockables!));
        Assert.Same(second, manager.Layout.ActiveContent);
        Assert.False(item.MoveToNextTabGroupCommand!.CanExecute(null));
        Assert.True(item.MoveToPreviousTabGroupCommand!.CanExecute(null));
        Execute(manager.GetLayoutItemFromModel(third)!.MoveToNextTabGroupCommand);
        Assert.Equal(new LayoutContent[] { third, second }, destination.Children);
        Execute(item.MoveToPreviousTabGroupCommand);
        Assert.Equal(new LayoutContent[] { second, first }, original.Children);
        Assert.Same(item, manager.GetLayoutItemFromModel(second));
    }

    [AvaloniaFact]
    public void Group_Command_Enablement_Tracks_Orientation_And_Mixed_Orientation_Policy()
    {
        LayoutDocument selected = new();
        LayoutDocumentPane left = new(selected);
        left.Children.Add(new LayoutDocument());
        LayoutDocumentPane right = new(new LayoutDocument());
        LayoutDocumentPaneGroup group = new(left) { Orientation = Orientation.Horizontal };
        group.Children.Add(right);
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(group) } };
        LayoutItem item = manager.GetLayoutItemFromModel(selected)!;
        Assert.False(item.NewHorizontalTabGroupCommand!.CanExecute(null));
        Assert.True(item.NewVerticalTabGroupCommand!.CanExecute(null));
        group.Orientation = Orientation.Vertical;
        Assert.True(item.NewHorizontalTabGroupCommand.CanExecute(null));
        Assert.False(item.NewVerticalTabGroupCommand.CanExecute(null));
        manager.AllowMixedOrientation = true;
        Execute(item.NewVerticalTabGroupCommand);
        Assert.Equal(Orientation.Horizontal, group.Orientation);
        Assert.Equal(3, group.Children.Count);
    }

    [AvaloniaFact]
    public void Layout_Replacement_Detaches_Containers_And_Retains_Application_Commands()
    {
        LayoutDocument document = new() { Content = new object() };
        DockingManager manager = CreateManager(document);
        LayoutItem item = manager.GetLayoutItemFromModel(document)!;
        ICommand activate = item.ActivateCommand!;
        int calls = 0;
        using var custom = ReactiveCommand.Create(() => calls++, outputScheduler: ImmediateScheduler.Instance);
        item.CloseCommand = custom;
        manager.Layout = new LayoutRoot();
        Assert.Null(item.LayoutElement);
        Assert.Null(item.Model);
        Assert.Null(item.DataContext);
        Assert.False(activate.CanExecute(null));
        Assert.Same(custom, item.CloseCommand);
        Execute(custom);
        Assert.Equal(1, calls);
        Assert.Null(manager.GetLayoutItemFromModel(document));
    }

    [AvaloniaFact]
    public void Visibility_Changes_Honor_Close_And_Hide_Cancellation()
    {
        LayoutDocument document = new();
        LayoutAnchorable tool = new();
        DockingManager manager = CreateManager(document, tool);
        LayoutItem documentItem = manager.GetLayoutItemFromModel(document)!;
        LayoutItem toolItem = manager.GetLayoutItemFromModel(tool)!;
        EventHandler<CancelEventArgs> veto = (_, args) => args.Cancel = true;
        document.Closing += veto;
        tool.Hiding += veto;
        documentItem.IsVisible = false;
        toolItem.IsVisible = false;
        Assert.True(documentItem.IsVisible);
        Assert.True(toolItem.IsVisible);
        Assert.Same(manager.Layout, document.Root);
        Assert.True(tool.IsVisible);
        document.Closing -= veto;
        tool.Hiding -= veto;
        documentItem.IsVisible = false;
        toolItem.IsVisible = false;
        Dispatcher.UIThread.RunJobs();
        Assert.Null(document.Root);
        Assert.Null(documentItem.LayoutElement);
        Assert.True(tool.IsHidden);
        Assert.False(toolItem.IsVisible);
        Assert.Same(tool, toolItem.LayoutElement);
    }

    [AvaloniaFact]
    public void Source_Removal_Detaches_Container_After_The_Mutation_Completes()
    {
        ObservableCollection<object> source = new() { new object() };
        DockingManager manager = new() { DocumentsSource = source };
        LayoutDocument document = Assert.Single(manager.Layout.Descendents().OfType<LayoutDocument>());
        LayoutItem item = manager.GetLayoutItemFromModel(document)!;
        source.Clear();
        Dispatcher.UIThread.RunJobs();
        Assert.Null(item.LayoutElement);
        Assert.Null(item.Model);
        Assert.Null(manager.GetLayoutItemFromModel(document));
    }

    [AvaloniaFact]
    public void Xml_Restore_Creates_Containers_For_Replacement_Nodes_And_Reuses_Application_Content()
    {
        object content = new();
        LayoutDocument document = new() { Content = content, ContentId = "document" };
        DockingManager manager = CreateManager(document);
        LayoutItem old = manager.GetLayoutItemFromModel(document)!;
        XmlLayoutSerializer serializer = new(manager);
        using StringWriter writer = new();
        serializer.Serialize(writer);
        serializer.Deserialize(new StringReader(writer.ToString()));
        LayoutDocument restored = Assert.Single(manager.Layout.Descendents().OfType<LayoutDocument>());
        LayoutItem current = manager.GetLayoutItemFromModel(restored)!;
        Assert.NotSame(old, current);
        Assert.Null(old.LayoutElement);
        Assert.Same(content, current.Model);
        Assert.Null(current.ContentId);
        Assert.Equal("document", restored.ContentId);
    }

    [AvaloniaFact]
    public void Compiled_Container_Bindings_Render_Content_And_Execute_Commands_From_Pointer_Input()
    {
        SourcesFixture source = new() { DataContext = new SourceViewModel() };
        DockingManager manager = source.FindControl<DockingManager>("Manager")!;
        LayoutDocument document = Assert.Single(manager.Layout.Descendents().OfType<LayoutDocument>());
        LayoutItem item = manager.GetLayoutItemFromModel(document)!;
        LayoutItemFixture view = new() { DataContext = item };
        try
        {
            view.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("Unprefixed container", Assert.IsType<LayoutDocumentItem>(view.Resources["DocumentContainer"]).Description);
            Assert.False(Assert.IsType<LayoutAnchorableItem>(view.Resources["ToolContainer"]).CanHide);
            Assert.Same(document.Content, item.View.Content);
            Assert.Contains(view.GetVisualDescendants().OfType<TextBlock>(), text => text.Classes.Contains("manager-template"));
            document.Title = "Changed document title";
            Assert.Equal(document.Title, view.FindControl<TextBlock>("TitleText")!.Text);
            Button button = view.FindControl<Button>("FloatButton")!;
            Point point = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), view)!.Value;
            view.MouseDown(point, MouseButton.Left);
            view.MouseUp(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            Assert.True(document.IsFloating);
            Assert.False(button.IsEffectivelyEnabled);
            SourceContent replacement = new("Replacement content");
            document.Content = replacement;
            Dispatcher.UIThread.RunJobs();
            Assert.Same(replacement, item.View.Content);
            Assert.Contains(view.GetVisualDescendants().OfType<TextBlock>(), text => text.Text == replacement.Text);
        }
        finally
        {
            manager.Layout.FloatingWindows.Clear();
            view.Close();
        }
    }

    [AvaloniaFact]
    public void Detaching_An_Item_View_Releases_Its_Rendered_Children_And_Reattachment_Renders_The_Editor()
    {
        TextBox editor = new() { Text = "Retained editor" };
        LayoutDocument document = new() { Content = editor };
        DockingManager manager = CreateManager(document);
        LayoutItem item = manager.GetLayoutItemFromModel(document)!;
        Window window = new() { Width = 700, Height = 500, Content = manager };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.CaptureRenderedFrame()?.Dispose();
            CompositionContainerVisual original = Assert.IsAssignableFrom<CompositionContainerVisual>(ElementComposition.GetElementVisual(item.View));
            Assert.NotEmpty(original.Children);
            window.Content = null;
            Assert.Empty(original.Children);
            Assert.Same(editor, item.View.Content);
            window.Content = manager;
            Dispatcher.UIThread.RunJobs();
            window.CaptureRenderedFrame()?.Dispose();
            CompositionContainerVisual current = Assert.IsAssignableFrom<CompositionContainerVisual>(ElementComposition.GetElementVisual(item.View));
            Assert.NotSame(original, current);
            Assert.NotEmpty(current.Children);
            Assert.Contains(editor, item.View.GetVisualDescendants());
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    private static DockingManager CreateManager(LayoutDocument document, LayoutAnchorable? tool = null)
    {
        LayoutPanel panel = new(new LayoutDocumentPane(document));
        if (tool is not null) panel.Children.Insert(0, new LayoutAnchorablePane(tool));
        return new DockingManager { Layout = new LayoutRoot { RootPanel = panel } };
    }

    private static void Execute(ICommand? command)
    {
        Assert.NotNull(command);
        Assert.True(command.CanExecute(null));
        command.Execute(null);
    }
}
