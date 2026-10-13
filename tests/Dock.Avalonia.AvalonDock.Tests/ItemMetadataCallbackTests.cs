using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class ItemMetadataCallbackTests
{
    [AvaloniaTheory]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 2)]
    [InlineData(false, 3)]
    [InlineData(true, 3)]
    public void Clearing_The_Container_Title_Preserves_Null_In_The_Public_And_Native_Model(bool tool, int operation)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        DockingManager manager = Create(content);
        LayoutItem item = manager.GetLayoutItemFromModel(content)!;
        item.Title = "previous";
        List<(string, string?)> changes = new();
        content.PropertyChanging += (_, args) =>
        {
            if (args.PropertyName == nameof(LayoutContent.Title)) changes.Add(("changing", content.Title));
        };
        content.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(LayoutContent.Title)) changes.Add(("changed", content.Title));
        };

        if (operation == 0) item.Title = null;
        else if (operation == 1) item.SetValue(LayoutItem.TitleProperty, null);
        else if (operation == 2) item.ClearValue(LayoutItem.TitleProperty);
        else item.SetValue((AvaloniaProperty)LayoutItem.TitleProperty, AvaloniaProperty.UnsetValue);

        Assert.Null(item.Title);
        Assert.Null(content.Title);
        Assert.Null(content.DockModel.Title);
        Assert.Equal(new (string, string?)[] { ("changing", "previous"), ("changed", null) }, changes);
        item.Title = "";
        Assert.Equal("", content.Title);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Document_Visibility_And_Pane_State_Change_Before_Cancelable_Closure(bool grouped, bool managerCancellation)
    {
        LayoutDocument document = new() { Content = new object(), CanClose = false };
        LayoutDocumentPane pane = new(document);
        DockingManager manager = new() { Layout = new() { RootPanel = new(grouped ? new LayoutDocumentPaneGroup(pane) : pane) } };
        LayoutItem item = manager.GetLayoutItemFromModel(document)!;
        int closing = 0;
        void Cancel(CancelEventArgs args)
        {
            closing++;
            Assert.False(item.IsVisible);
            Assert.False(document.IsVisible);
            Assert.Equal(!grouped, pane.IsVisible);
            Assert.Same(pane, document.Parent);
            args.Cancel = true;
        }
        EventHandler<CancelEventArgs> cancelModel = (_, args) => Cancel(args);
        EventHandler<DocumentClosingEventArgs> cancelManager = (_, args) => Cancel(args);
        if (managerCancellation) manager.DocumentClosing += cancelManager;
        else document.Closing += cancelModel;

        item.IsVisible = false;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(1, closing);
        Assert.False(document.IsVisible);
        Assert.False(item.IsVisible);
        Assert.Same(pane, document.Parent);
        Assert.Same(document.DockModel, Assert.Single(((Dock.Model.Avalonia.Controls.DocumentDock)pane.DockModel).VisibleDockables!));

        item.IsVisible = true;
        Assert.True(document.IsVisible);
        Assert.True(pane.IsVisible);
        if (managerCancellation) manager.DocumentClosing -= cancelManager;
        else document.Closing -= cancelModel;
        item.IsVisible = false;
        Assert.Null(document.Parent);
        Assert.False(document.IsVisible);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Document_Closing_Failure_Retains_Visibility_And_Can_Be_Retried(bool managerFailure)
    {
        LayoutDocument document = new() { Content = new object() };
        DockingManager manager = Create(document);
        LayoutItem item = manager.GetLayoutItemFromModel(document)!;
        EventHandler<CancelEventArgs> modelFailure = (_, _) => throw new InvalidOperationException("closing");
        EventHandler<DocumentClosingEventArgs> managerHandler = (_, _) => throw new InvalidOperationException("closing");
        if (managerFailure) manager.DocumentClosing += managerHandler;
        else document.Closing += modelFailure;
        Assert.Throws<InvalidOperationException>(() => item.IsVisible = false);
        Assert.False(document.IsVisible);
        Assert.False(item.IsVisible);
        Assert.NotNull(document.Parent);
        if (managerFailure) manager.DocumentClosing -= managerHandler;
        else document.Closing -= modelFailure;
        item.IsVisible = true;
        item.IsVisible = false;
        Assert.Null(document.Parent);
    }

    [AvaloniaTheory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public void Tool_Container_Visibility_Hides_Without_Capability_Or_Cancellation_Gates(bool canHide, bool canClose, bool cancel)
    {
        LayoutAnchorable tool = new() { Content = new object(), CanHide = canHide, CanClose = canClose };
        DockingManager manager = Create(tool);
        LayoutItem item = manager.GetLayoutItemFromModel(tool)!;
        ILayoutContainer pane = tool.Parent!;
        int hiding = 0, hidden = 0, closing = 0;
        tool.Hiding += (_, args) => { hiding++; args.Cancel = cancel; };
        tool.Hidden += (_, _) => hidden++;
        tool.Closing += (_, _) => closing++;

        item.IsVisible = false;

        Assert.Equal(0, hiding);
        Assert.Equal(0, closing);
        Assert.Equal(1, hidden);
        Assert.True(tool.IsHidden);
        Assert.False(item.IsVisible);
        Assert.Same(tool, Assert.Single(manager.Layout.Hidden));
        Assert.Same(pane, tool.PreviousContainer);
        Assert.Same(tool.DockModel, Assert.Single(manager.DockLayout.HiddenDockables!));
        item.IsVisible = true;
        Assert.True(tool.IsVisible);
        Assert.Same(pane, tool.Parent);
        Assert.Empty(manager.Layout.Hidden);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Tool_Visibility_Guard_Is_Released_After_Hidden_Callback_Failure(bool canHide)
    {
        LayoutAnchorable tool = new() { Content = new object(), CanHide = canHide };
        DockingManager manager = Create(tool);
        LayoutItem item = manager.GetLayoutItemFromModel(tool)!;
        EventHandler fail = (_, _) => throw new InvalidOperationException("hidden");
        tool.Hidden += fail;
        Assert.Throws<InvalidOperationException>(() => item.IsVisible = false);
        Assert.True(tool.IsHidden);
        Assert.False(item.IsVisible);
        tool.Hidden -= fail;
        item.IsVisible = true;
        Assert.True(tool.IsVisible);
        item.IsVisible = false;
        Assert.True(tool.IsHidden);
    }

    private static DockingManager Create(LayoutContent content)
    {
        ILayoutPanelElement pane = content is LayoutAnchorable tool
            ? new LayoutAnchorablePane(tool) : new LayoutDocumentPane(content);
        return new() { Layout = new() { RootPanel = new(pane) } };
    }
}
