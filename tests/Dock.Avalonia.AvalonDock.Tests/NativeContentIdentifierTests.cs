using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using AvalonDock;
using AvalonDock.Layout;
using Dock.Model.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class NativeContentIdentifierTests
{
    [AvaloniaTheory]
    [InlineData(false, null)]
    [InlineData(true, null)]
    [InlineData(false, "")]
    [InlineData(true, "")]
    [InlineData(false, " ")]
    [InlineData(true, " ")]
    [InlineData(false, "explicit")]
    [InlineData(true, "explicit")]
    public void Native_Content_Changes_Apply_The_Name_Fallback_Only_To_Null_Identifiers(bool tools, string? identifier)
    {
        LayoutContent content = tools ? new LayoutAnchorable() : new LayoutDocument();
        content.ContentId = identifier;
        Border view = new() { Name = "NativeView" };
        List<string> changes = new();
        content.PropertyChanged += (_, args) => changes.Add(args.PropertyName!);

        if (content.DockModel is Document document) document.Content = view;
        else ((Tool)content.DockModel).Content = view;

        Assert.Same(view, content.Content);
        Assert.Equal(identifier ?? "NativeView", content.ContentId);
        Assert.Equal(content.ContentId, content.DockModel.Id);
        Assert.Equal(identifier is null ? new[] { "Content", "ContentId" } : new[] { "Content" }, changes);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_Identifiers_Notify_The_Compatibility_Name_And_Do_Not_Overwrite_Container_Input(bool tools)
    {
        LayoutContent content = tools ? new LayoutAnchorable() : new LayoutDocument();
        ILayoutPanelElement pane = tools ? new LayoutAnchorablePane((LayoutAnchorable)content)
            : new LayoutDocumentPane((LayoutDocument)content);
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(pane) } };
        var item = manager.GetLayoutItemFromModel(content)!;
        item.ContentId = "container";
        List<string> changes = new();
        content.PropertyChanged += (_, args) => changes.Add(args.PropertyName!);

        content.DockModel.Id = "native";
        content.DockModel.Id = null!;

        Assert.Null(content.ContentId);
        Assert.Equal("container", item.ContentId);
        Assert.Equal(new[] { "ContentId", "ContentId" }, changes);
    }
}
