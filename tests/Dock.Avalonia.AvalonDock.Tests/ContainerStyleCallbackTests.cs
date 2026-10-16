using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class ContainerStyleCallbackTests
{
    [AvaloniaTheory]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    public void Clearing_The_Last_Style_Provider_Retains_Existing_Containers_But_Does_Not_Style_New_Ones(bool documents, bool selector)
    {
        (StyleManager manager, LayoutContent content, LayoutItem item) = Create(documents);
        Command command = new();
        ControlTheme theme = Theme("retained", command);
        if (selector) manager.LayoutItemContainerStyleSelector = new Selector { Selecting = _ => theme };
        else manager.LayoutItemContainerStyle = theme;

        if (selector) manager.LayoutItemContainerStyleSelector = null;
        else manager.LayoutItemContainerStyle = null;
        Assert.Same(theme, item.Theme);
        Assert.Same(command, item.CloseCommand);
        Assert.Equal("retained", content.Title);
        LayoutContent added = documents ? new LayoutDocument() : new LayoutAnchorable();
        ((ILayoutGroup)content.Parent!).InsertChildAt(1, added);
        LayoutItem addedItem = Assert.IsAssignableFrom<LayoutItem>(manager.GetLayoutItemFromModel(added));
        Assert.Null(addedItem.Theme);
        Assert.NotSame(command, addedItem.CloseCommand);

        content.Content = new object();
        Assert.Same(theme, item.Theme);
        Assert.Same(command, item.CloseCommand);
        Assert.Equal("retained", content.Title);
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void A_Selector_Returning_Null_Clears_The_Existing_Style(bool documents)
    {
        (StyleManager manager, _, LayoutItem item) = Create(documents);
        ICommand defaultClose = item.CloseCommand!;
        Command command = new();
        manager.LayoutItemContainerStyle = Theme("explicit", command);
        manager.LayoutItemContainerStyle = null;
        Assert.Same(command, item.CloseCommand);

        manager.LayoutItemContainerStyleSelector = new Selector();
        Assert.Null(item.Theme);
        Assert.Same(defaultClose, item.CloseCommand);
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Selectors_See_Cleared_Defaults_And_Preserve_Application_Commands(bool documents)
    {
        (StyleManager manager, _, LayoutItem item) = Create(documents);
        ICommand defaultClose = item.CloseCommand!;
        Command application = new();
        item.FloatCommand = application;
        Selector selector = new()
        {
            Selecting = current =>
            {
                Assert.Null(current.CloseCommand);
                Assert.Null(current.ActivateCommand);
                Assert.Same(application, current.FloatCommand);
                return null;
            }
        };

        manager.LayoutItemContainerStyleSelector = selector;
        Assert.Equal(1, selector.Calls);
        Assert.Same(defaultClose, item.CloseCommand);
        Assert.Same(application, item.FloatCommand);
        manager.Apply(manager.LastChange!, true);
        Assert.Equal(2, selector.Calls);
        Assert.Same(defaultClose, item.CloseCommand);
    }

    [AvaloniaFact]
    public void Each_Attachment_Refreshes_All_Registered_Items_In_Registration_Order()
    {
        LayoutDocument first = new() { Content = "first" };
        LayoutDocument second = new() { Content = "second" };
        LayoutAnchorable tool = new() { Content = "tool" };
        LayoutDocumentPane documents = new(first);
        documents.Children.Add(second);
        LayoutPanel panel = new(new LayoutAnchorablePane(tool));
        panel.Children.Add(documents);
        DockingManager manager = new() { Layout = new() { RootPanel = panel } };
        List<object?> calls = new();
        manager.LayoutItemContainerStyleSelector = new Selector
        {
            Selecting = item => { calls.Add(item.Model); return null; }
        };
        Assert.Equal(new[] { "first", "second", "tool", "first", "second", "tool", "first", "second", "tool" }, calls);
    }

    [AvaloniaFact]
    public void Restyling_Preserves_Detached_Containers_Until_Deferred_Cleanup()
    {
        (StyleManager manager, LayoutContent content, _) = Create(true);
        LayoutDocumentPane pane = (LayoutDocumentPane)content.Parent!;
        LayoutDocument removed = new() { Content = "removed" };
        pane.Children.Add(removed);
        LayoutItem retained = manager.GetLayoutItemFromModel(removed)!;
        pane.Children.Remove(removed);
        List<LayoutItem> styled = new();

        manager.LayoutItemContainerStyleSelector = new Selector
        {
            Selecting = item => { styled.Add(item); return null; }
        };
        Assert.Equal(2, styled.Count);
        Assert.Same(retained, styled[1]);
        Assert.Same(retained, manager.GetLayoutItemFromModel(removed));
        Dispatcher.UIThread.RunJobs();
        Assert.Null(manager.GetLayoutItemFromModel(removed));
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void A_Throwing_Selector_Leaves_Defaults_Cleared_And_Preserves_The_Previous_Theme(bool documents)
    {
        (StyleManager manager, _, LayoutItem item) = Create(documents);
        ICommand defaultClose = item.CloseCommand!;
        ControlTheme theme = Theme("previous");
        manager.LayoutItemContainerStyle = theme;
        manager.LayoutItemContainerStyle = null;
        Selector failure = new() { Selecting = _ => throw new InvalidOperationException("Style failure") };

        Assert.Throws<InvalidOperationException>(() => manager.LayoutItemContainerStyleSelector = failure);
        Assert.Same(theme, item.Theme);
        Assert.Null(item.CloseCommand);
        manager.LayoutItemContainerStyleSelector = null;
        Assert.Same(theme, item.Theme);
        Assert.Same(defaultClose, item.CloseCommand);
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void A_Selector_Result_Is_Applied_After_Its_Reentrant_Manager_Style_Change(bool documents)
    {
        (StyleManager manager, _, LayoutItem item) = Create(documents);
        ControlTheme selected = Theme("selected");
        ControlTheme redirected = Theme("redirected");
        Selector selector = new()
        {
            Selecting = current =>
            {
                Assert.Null(current.CloseCommand);
                manager.LayoutItemContainerStyle = redirected;
                Assert.Same(redirected, current.Theme);
                Assert.NotNull(current.CloseCommand);
                return selected;
            }
        };

        manager.LayoutItemContainerStyleSelector = selector;
        Assert.Equal(1, selector.Calls);
        Assert.Same(redirected, manager.LayoutItemContainerStyle);
        Assert.Same(selected, item.Theme);
        Assert.NotNull(item.CloseCommand);
    }

    [AvaloniaTheory]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    public void Suppressed_Or_Throwing_Style_Hooks_Retain_Existing_Styles_While_New_Items_Read_Current_Settings(bool documents, bool fail)
    {
        (StyleManager manager, LayoutContent content, LayoutItem item) = Create(documents);
        ControlTheme original = Theme("original");
        ControlTheme current = Theme("current");
        ObservableCollection<object> source = new();
        if (documents) manager.DocumentsSource = source;
        else manager.AnchorablesSource = source;
        manager.LayoutItemContainerStyle = original;
        manager.Suppress = !fail;
        manager.Fail = fail;
        if (fail) Assert.Throws<InvalidOperationException>(() => manager.LayoutItemContainerStyle = current);
        else manager.LayoutItemContainerStyle = current;
        Assert.Same(original, item.Theme);

        object newContent = new();
        source.Add(newContent);
        LayoutContent added = manager.Layout.Descendents().OfType<LayoutContent>()
            .Single(value => ReferenceEquals(value.Content, newContent));
        Assert.Same(current, manager.GetLayoutItemFromModel(added)!.Theme);
        Assert.Same(original, item.Theme);
        manager.Suppress = manager.Fail = false;
        manager.Apply(manager.LastChange!, false);
        Assert.Same(current, item.Theme);
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Native_Membership_Changes_Reapply_Current_Styles_After_A_Suppressed_Manager_Callback(bool documents)
    {
        (StyleManager manager, LayoutContent content, LayoutItem item) = Create(documents);
        ControlTheme original = Theme("original");
        ControlTheme current = Theme("current");
        manager.LayoutItemContainerStyle = original;
        manager.Suppress = true;
        manager.LayoutItemContainerStyle = current;
        Assert.Same(original, item.Theme);

        LayoutContent added = documents ? new LayoutDocument() : new LayoutAnchorable();
        ((ILayoutGroup)content.Parent!).InsertChildAt(1, added);
        Assert.Same(current, item.Theme);
        Assert.Same(current, manager.GetLayoutItemFromModel(added)!.Theme);
        Assert.Equal("current", content.Title);
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Replaying_A_Style_Callback_Uses_Current_Manager_Settings(bool selector)
    {
        (StyleManager manager, _, LayoutItem item) = Create(true);
        ControlTheme original = Theme("original");
        ControlTheme current = Theme("current");
        if (selector) manager.LayoutItemContainerStyleSelector = new Selector { Selecting = _ => original };
        else manager.LayoutItemContainerStyle = original;
        AvaloniaPropertyChangedEventArgs captured = manager.LastChange!;
        manager.Suppress = true;
        if (selector) manager.LayoutItemContainerStyleSelector = new Selector { Selecting = _ => current };
        else manager.LayoutItemContainerStyle = current;
        manager.Suppress = false;

        manager.Apply(captured, selector);
        Assert.Same(current, item.Theme);
    }

    private static (StyleManager Manager, LayoutContent Content, LayoutItem Item) Create(bool documents)
    {
        LayoutContent content = documents ? new LayoutDocument { Content = new object() } : new LayoutAnchorable { Content = new object() };
        ILayoutPanelElement pane = documents ? new LayoutDocumentPane((LayoutDocument)content) : new LayoutAnchorablePane((LayoutAnchorable)content);
        StyleManager manager = new() { Layout = new() { RootPanel = new LayoutPanel(pane) } };
        return (manager, content, manager.GetLayoutItemFromModel(content)!);
    }

    private static ControlTheme Theme(string title, ICommand? command = null)
    {
        ControlTheme theme = new(typeof(LayoutItem)) { Setters = { new Setter(LayoutItem.TitleProperty, title) } };
        if (command is not null) theme.Setters.Add(new Setter(LayoutItem.CloseCommandProperty, command));
        return theme;
    }

    private sealed class Selector : StyleSelector
    {
        internal Func<LayoutItem, ControlTheme?>? Selecting { get; set; }
        internal int Calls { get; private set; }
        public override ControlTheme? SelectStyle(object? item, AvaloniaObject container)
        {
            Calls++;
            return Selecting?.Invoke((LayoutItem)container);
        }
    }

    private sealed class StyleManager : DockingManager
    {
        internal bool Suppress { get; set; }
        internal bool Fail { get; set; }
        internal AvaloniaPropertyChangedEventArgs? LastChange { get; private set; }
        internal void Apply(AvaloniaPropertyChangedEventArgs e, bool selector)
        {
            if (selector) base.OnLayoutItemContainerStyleSelectorChanged(e);
            else base.OnLayoutItemContainerStyleChanged(e);
        }
        protected override void OnLayoutItemContainerStyleChanged(AvaloniaPropertyChangedEventArgs e) => Change(e, false);
        protected override void OnLayoutItemContainerStyleSelectorChanged(AvaloniaPropertyChangedEventArgs e) => Change(e, true);
        private void Change(AvaloniaPropertyChangedEventArgs e, bool selector)
        {
            LastChange = e;
            if (Fail) throw new InvalidOperationException("Style hook failed");
            if (!Suppress) Apply(e, selector);
        }
    }

    private sealed class Command : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) { }
    }
}
