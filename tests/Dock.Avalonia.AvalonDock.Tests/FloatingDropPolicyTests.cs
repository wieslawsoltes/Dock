using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Model;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class FloatingDropPolicyTests
{
    [AvaloniaTheory]
    [InlineData(false, DockOperation.Fill)]
    [InlineData(false, DockOperation.Left)]
    [InlineData(false, DockOperation.Top)]
    [InlineData(false, DockOperation.Right)]
    [InlineData(false, DockOperation.Bottom)]
    [InlineData(true, DockOperation.Fill)]
    [InlineData(true, DockOperation.Left)]
    [InlineData(true, DockOperation.Top)]
    [InlineData(true, DockOperation.Right)]
    [InlineData(true, DockOperation.Bottom)]
    public void Document_Floating_Windows_Do_Not_Accept_Extra_Content(bool tool, DockOperation operation)
    {
        using Workspace workspace = new();
        workspace.TargetDocument.Float(); workspace.Update();
        LayoutContent source = tool ? workspace.SourceTool : workspace.SourceDocument;
        IDockable target = workspace.TargetDocument.DockModel.Owner!;
        ILayoutContainer? sourceParent = source.Parent;
        string before = workspace.Save();
        DockManager engine = new(new DockService());
        Assert.False(engine.IsDockTargetVisible(source.DockModel, target, operation));
        Assert.False(engine.ValidateDockable(source.DockModel, target, DragAction.Move, operation, false));
        Assert.False(engine.ValidateDockable(source.DockModel, target, DragAction.Move, operation, true));
        Assert.Same(sourceParent, source.Parent);
        Assert.Same(workspace.TargetDocument, Assert.IsType<LayoutDocumentFloatingWindow>(Assert.Single(workspace.Manager.Layout.FloatingWindows)).RootDocument);
        Assert.Equal(before, workspace.Save());
    }

    [AvaloniaTheory]
    [InlineData(DockOperation.Fill)]
    [InlineData(DockOperation.Left)]
    [InlineData(DockOperation.Top)]
    [InlineData(DockOperation.Right)]
    [InlineData(DockOperation.Bottom)]
    public void Tool_Floating_Windows_Reject_Document_Drops_Before_Mutating_The_Layout(DockOperation operation)
    {
        using Workspace workspace = new();
        workspace.SourceDocument.Float();
        workspace.TargetTool.Float(); workspace.Update();
        IDockable target = workspace.TargetTool.DockModel.Owner!;
        string before = workspace.Save();
        DockManager engine = new(new DockService());
        Assert.False(engine.IsDockTargetVisible(workspace.SourceDocument.DockModel, target, operation));
        Assert.False(engine.ValidateDockable(workspace.SourceDocument.DockModel, target, DragAction.Move, operation, true));
        Assert.Equal(before, workspace.Save());
        Assert.Equal(2, workspace.Manager.FloatingWindows.Count());
    }

    [AvaloniaTheory]
    [InlineData(false, DockOperation.Fill)]
    [InlineData(false, DockOperation.Left)]
    [InlineData(false, DockOperation.Top)]
    [InlineData(false, DockOperation.Right)]
    [InlineData(false, DockOperation.Bottom)]
    [InlineData(true, DockOperation.Fill)]
    [InlineData(true, DockOperation.Left)]
    [InlineData(true, DockOperation.Top)]
    [InlineData(true, DockOperation.Right)]
    [InlineData(true, DockOperation.Bottom)]
    public void Tool_Drops_Into_Floating_Windows_Preserve_Typed_Groups_Content_And_Xml(bool fromFloating, DockOperation operation)
    {
        using Workspace workspace = new();
        workspace.TargetTool.Float();
        if (fromFloating) workspace.SourceTool.Float();
        workspace.Update();
        LayoutAnchorableFloatingWindow floating = workspace.TargetTool.FindParent<LayoutAnchorableFloatingWindow>()!;
        object? item = workspace.Manager.GetLayoutItemFromModel(workspace.SourceTool);
        int closed = 0;
        workspace.SourceTool.Closed += (_, _) => closed++;
        workspace.TargetTool.Closed += (_, _) => closed++;
        DockManager engine = new(new DockService());
        IDockable target = workspace.TargetTool.DockModel.Owner!;
        Assert.True(engine.ValidateDockable(workspace.SourceTool.DockModel, target, DragAction.Move, operation, false));
        Assert.True(engine.ValidateDockable(workspace.SourceTool.DockModel, target, DragAction.Move, operation, true));
        workspace.Update();
        Assert.Same(floating, workspace.SourceTool.FindParent<LayoutAnchorableFloatingWindow>());
        Assert.Same(floating, workspace.TargetTool.FindParent<LayoutAnchorableFloatingWindow>());
        Assert.Same(item, workspace.Manager.GetLayoutItemFromModel(workspace.SourceTool));
        Assert.Equal(2, floating.Descendents().OfType<LayoutAnchorable>().Count());
        Assert.DoesNotContain(floating.Descendents(), node => node is LayoutPanel);
        Assert.True(Assert.IsAssignableFrom<Window>(floating.DockWindow.Host).IsVisible);
        Assert.Equal(0, closed);
        string saved = workspace.Save();
        workspace.Reload(saved);
        LayoutAnchorable source = workspace.Manager.Layout.Descendents().OfType<LayoutAnchorable>().Single(x => x.ContentId == "source-tool");
        LayoutAnchorable destination = workspace.Manager.Layout.Descendents().OfType<LayoutAnchorable>().Single(x => x.ContentId == "target-tool");
        Assert.Same(source.FindParent<LayoutAnchorableFloatingWindow>(), destination.FindParent<LayoutAnchorableFloatingWindow>());
        Assert.Equal("Source tool content", source.Content);
        Assert.Equal("Target tool content", destination.Content);
        source.Dock(); destination.Dock(); workspace.Update();
        Assert.False(source.IsFloating); Assert.False(destination.IsFloating);
        workspace.Save();
    }

    [AvaloniaTheory]
    [InlineData(DockOperation.Left)]
    [InlineData(DockOperation.Top)]
    [InlineData(DockOperation.Right)]
    [InlineData(DockOperation.Bottom)]
    public void Native_Splits_Inside_Document_Groups_Keep_Their_Public_Group_Type(DockOperation operation)
    {
        using Workspace workspace = new();
        LayoutDocumentPane target = (LayoutDocumentPane)workspace.TargetDocument.Parent!;
        LayoutDocumentPane sibling = new(new LayoutDocument { ContentId = "third-document", Content = "Third" });
        LayoutDocumentPaneGroup group = new(target);
        group.Children.Add(sibling);
        workspace.Manager.Layout.RootPanel.Children.Add(group);
        workspace.Manager.AllowMixedOrientation = true;
        DockManager engine = new(new DockService());
        Assert.True(engine.ValidateDockable(workspace.SourceDocument.DockModel, target.DockModel, DragAction.Move, operation, true));
        workspace.Update();
        Assert.Contains(workspace.SourceDocument, group.Descendents().OfType<LayoutDocument>());
        Assert.DoesNotContain(group.Descendents(), node => node is LayoutPanel);
        workspace.Reload(workspace.Save());
        Assert.Equal(3, workspace.Manager.Layout.Descendents().OfType<LayoutDocument>().Count());
    }

    [AvaloniaFact]
    public void A_Different_Manager_Cannot_Drop_Into_The_Compatibility_Layout()
    {
        using Workspace source = new();
        using Workspace target = new();
        target.TargetTool.Float(); target.Update();
        DockManager engine = new(new DockService());
        Assert.False(engine.ValidateDockable(source.SourceTool.DockModel, target.TargetTool.DockModel.Owner!, DragAction.Move, DockOperation.Fill, true));
        Assert.False(source.SourceTool.IsFloating);
        Assert.Same(source.Manager.Layout, source.SourceTool.Root);
        Assert.Single(target.Manager.Layout.FloatingWindows);
    }

    [AvaloniaTheory]
    [InlineData(false, true, DockOperation.Left, true)]
    [InlineData(false, true, DockOperation.Right, true)]
    [InlineData(false, true, DockOperation.Top, false)]
    [InlineData(false, true, DockOperation.Bottom, false)]
    [InlineData(true, true, DockOperation.Top, true)]
    [InlineData(true, true, DockOperation.Bottom, true)]
    [InlineData(false, false, DockOperation.Top, true)]
    [InlineData(false, false, DockOperation.Bottom, true)]
    public void Native_Document_Split_Visibility_And_Execution_Honor_Mixed_Orientation(bool mixed, bool populated, DockOperation operation, bool allowed)
    {
        using Workspace workspace = new();
        LayoutDocumentPane target = (LayoutDocumentPane)workspace.TargetDocument.Parent!;
        LayoutDocumentPane sibling = populated ? new(new LayoutDocument { ContentId = "third-document" }) : new();
        LayoutDocumentPaneGroup group = new(target);
        group.Children.Add(sibling);
        workspace.Manager.Layout.RootPanel.Children.Add(group);
        workspace.Manager.AllowMixedOrientation = mixed;
        DockManager engine = new(new DockService());
        ILayoutContainer? previous = workspace.SourceDocument.Parent;
        Assert.Equal(allowed, engine.IsDockTargetVisible(workspace.SourceDocument.DockModel, target.DockModel, operation));
        Assert.Equal(allowed, engine.ValidateDockable(workspace.SourceDocument.DockModel, target.DockModel, DragAction.Move, operation, false));
        Assert.Equal(allowed, engine.ValidateDockable(workspace.SourceDocument.DockModel, target.DockModel, DragAction.Move, operation, true));
        if (!allowed)
        {
            Assert.Same(previous, workspace.SourceDocument.Parent);
            workspace.Manager.AllowMixedOrientation = true;
            Assert.True(engine.IsDockTargetVisible(workspace.SourceDocument.DockModel, target.DockModel, operation));
        }
        else Assert.NotSame(previous, workspace.SourceDocument.Parent);
        workspace.Update(); workspace.Save();
    }

    [AvaloniaFact]
    public void A_Floating_Document_Can_Still_Redock_Or_Move_To_Another_Native_Window()
    {
        using Workspace workspace = new();
        workspace.SourceDocument.Float(); workspace.Update();
        DockManager engine = new(new DockService());
        Assert.True(engine.ValidateDockable(workspace.SourceDocument.DockModel, workspace.SourceDocument.DockModel.Owner!, DragAction.Move, DockOperation.Window, true));
        Assert.True(workspace.SourceDocument.IsFloating);
        Assert.True(engine.ValidateDockable(workspace.SourceDocument.DockModel, workspace.TargetDocument.DockModel.Owner!, DragAction.Move, DockOperation.Fill, true));
        Assert.False(workspace.SourceDocument.IsFloating);
        workspace.Save();
    }

    private sealed class Workspace : IDisposable
    {
        public DockingManager Manager { get; } = new();
        public LayoutDocument SourceDocument { get; } = new() { ContentId = "source-document", Content = "Source document" };
        public LayoutDocument TargetDocument { get; } = new() { ContentId = "target-document", Content = "Target document" };
        public LayoutAnchorable SourceTool { get; } = new() { ContentId = "source-tool", Content = "Source tool content" };
        public LayoutAnchorable TargetTool { get; } = new() { ContentId = "target-tool", Content = "Target tool content" };
        private readonly Window _view;
        public Workspace()
        {
            Manager.Layout = new() { RootPanel = new() { Children =
            {
                new LayoutDocumentPane(SourceDocument), new LayoutDocumentPane(TargetDocument),
                new LayoutAnchorablePane(SourceTool), new LayoutAnchorablePane(TargetTool)
            } } };
            _view = new() { Width = 1000, Height = 600, Content = Manager };
            _view.Show(); Update();
        }
        public void Update() { Dispatcher.UIThread.RunJobs(); _view.UpdateLayout(); }
        public string Save() { using StringWriter writer = new(); new XmlLayoutSerializer(Manager).Serialize(writer); return writer.ToString(); }
        public void Reload(string xml) { new XmlLayoutSerializer(Manager).Deserialize(new StringReader(xml)); Update(); }
        public void Dispose() { Manager.Layout.FloatingWindows.Clear(); _view.Close(); Dispatcher.UIThread.RunJobs(); }
    }

}
