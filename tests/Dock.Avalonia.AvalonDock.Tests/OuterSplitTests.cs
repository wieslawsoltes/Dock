using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Model;
using Dock.Model.Core;
using Xunit;
using Orientation = Avalonia.Layout.Orientation;

namespace Dock.Avalonia.AvalonDock.Tests;

public class OuterSplitTests
{
    public static IEnumerable<object[]> Cases()
    {
        foreach (DockOperation operation in new[] { DockOperation.OuterLeft, DockOperation.OuterTop, DockOperation.OuterRight, DockOperation.OuterBottom })
            for (int shape = 0; shape < 3; shape++)
                foreach (bool floating in new[] { false, true })
                    foreach (bool nested in new[] { false, true }) yield return new object[] { operation, shape, floating, nested };
    }

    [AvaloniaTheory]
    [MemberData(nameof(Cases))]
    public void Tool_Drops_Beside_Document_Regions_Preserve_Panes_And_Groups(DockOperation operation, int shape, bool floating, bool nested)
    {
        using Workspace workspace = new(shape, nested);
        if (floating) workspace.Manager.DockFactory.FloatDockable(workspace.Source.DockModel);
        workspace.Update();
        object[] containers = workspace.Tools.Select(tool => workspace.Manager.GetLayoutItemFromModel(tool)!).ToArray();
        ILayoutContainer? previous = workspace.Source.Parent;
        DockManager engine = new(new DockService());
        string before = workspace.Save();
        Assert.True(engine.IsDockTargetVisible(workspace.Source.DockModel, workspace.Target.DockModel, operation));
        Assert.True(engine.ValidateDockable(workspace.Source.DockModel, workspace.Target.DockModel, DragAction.Move, operation, false));
        Assert.Equal(before, workspace.Save());
        Assert.Same(previous, workspace.Source.Parent);
        Assert.True(engine.ValidateDockable(workspace.Source.DockModel, workspace.Target.DockModel, DragAction.Move, operation, true));
        workspace.Update();

        LayoutElement moved = shape == 0 ? Assert.IsType<LayoutAnchorablePane>(workspace.Source.Parent) : workspace.Source;
        LayoutPanel parent = Assert.IsType<LayoutPanel>(moved.Parent);
        Assert.Same(parent, workspace.Region.Parent);
        Assert.Equal(operation is DockOperation.OuterLeft or DockOperation.OuterRight ? Orientation.Horizontal : Orientation.Vertical, parent.Orientation);
        Assert.Equal(operation is DockOperation.OuterLeft or DockOperation.OuterTop,
            parent.IndexOfChild(moved) < parent.IndexOfChild(workspace.Region));
        Assert.Same(workspace.Region, nested ? workspace.Target.Parent!.Parent : workspace.Target);
        if (shape > 0)
        {
            Assert.Same(workspace.FirstPane, workspace.Tools[0].Parent);
            Assert.Equal(workspace.Tools.Take(3), workspace.FirstPane.Children);
        }
        for (int i = 0; i < workspace.Tools.Length; i++)
        {
            Assert.IsType<LayoutAnchorablePane>(workspace.Tools[i].Parent);
            Assert.False(workspace.Tools[i].IsFloating);
            Assert.Same(containers[i], workspace.Manager.GetLayoutItemFromModel(workspace.Tools[i]));
        }
        Assert.Empty(workspace.Manager.Layout.FloatingWindows);
        Assert.Same(workspace.Tools[^1], workspace.Manager.Layout.ActiveContent);
        new XmlLayoutSerializer(workspace.Manager).Deserialize(new StringReader(workspace.Save()));
        workspace.Update();
        LayoutAnchorable[] restored = workspace.Manager.Layout.Descendents().OfType<LayoutAnchorable>().ToArray();
        Assert.Equal(workspace.Tools.Length, restored.Length);
        Assert.All(restored, tool => Assert.IsType<LayoutAnchorablePane>(tool.Parent));
        foreach (LayoutAnchorable tool in restored) Assert.Equal(tool.ContentId, tool.Content);
    }

    [AvaloniaTheory]
    [InlineData(false, 0, DockOperation.OuterLeft, true)]
    [InlineData(false, 0, DockOperation.OuterRight, false)]
    [InlineData(false, 1, DockOperation.OuterRight, true)]
    [InlineData(false, 1, DockOperation.OuterTop, false)]
    [InlineData(true, 0, DockOperation.OuterTop, true)]
    [InlineData(true, 0, DockOperation.OuterBottom, false)]
    [InlineData(true, 1, DockOperation.OuterBottom, true)]
    [InlineData(true, 1, DockOperation.OuterLeft, false)]
    public void Non_Mixed_Document_Groups_Expose_Only_Their_Outer_Edges(bool vertical, int index, DockOperation operation, bool allowed)
    {
        using Workspace workspace = new(1, false);
        LayoutDocumentPane other = new(new LayoutDocument { ContentId = "other" });
        LayoutDocumentPaneGroup group = new(workspace.Target) { Orientation = vertical ? Orientation.Vertical : Orientation.Horizontal };
        group.Children.Add(other);
        workspace.Manager.Layout.RootPanel.Children.Add(group);
        workspace.Manager.AllowMixedOrientation = false;
        LayoutDocumentPane target = index == 0 ? workspace.Target : other;
        DockManager engine = new(new DockService());
        string before = workspace.Save();
        Assert.Equal(allowed, engine.IsDockTargetVisible(workspace.Source.DockModel, target.DockModel, operation));
        Assert.Equal(allowed, engine.ValidateDockable(workspace.Source.DockModel, target.DockModel, DragAction.Move, operation, false));
        Assert.Equal(before, workspace.Save());
        workspace.Manager.AllowMixedOrientation = true;
        Assert.True(engine.IsDockTargetVisible(workspace.Source.DockModel, target.DockModel, operation));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Document_Content_And_Floating_Document_Targets_Reject_Outer_Tool_Drops(bool documentSource)
    {
        using Workspace workspace = new(1, false);
        LayoutDocument document = (LayoutDocument)workspace.Target.Children[0];
        LayoutDocumentPane other = new(new LayoutDocument { ContentId = "other" });
        workspace.Manager.Layout.RootPanel.Children.Add(other);
        if (!documentSource) document.Float();
        IDockable source = documentSource ? document.DockModel : workspace.Source.DockModel;
        IDockable target = documentSource ? other.DockModel : document.DockModel.Owner!;
        DockManager engine = new(new DockService());
        string before = workspace.Save();
        Assert.False(engine.IsDockTargetVisible(source, target, DockOperation.OuterLeft));
        Assert.False(engine.ValidateDockable(source, target, DragAction.Move, DockOperation.OuterLeft, true));
        Assert.Equal(before, workspace.Save());
    }

    private sealed class Workspace : IDisposable
    {
        internal DockingManager Manager { get; } = new() { AllowMixedOrientation = true };
        internal LayoutDocumentPane Target { get; } = new(new LayoutDocument { ContentId = "target", Content = "target" });
        internal LayoutElement Region { get; }
        internal LayoutElement Source { get; }
        internal LayoutAnchorablePane FirstPane { get; }
        internal LayoutAnchorable[] Tools { get; }
        private readonly Window _window;
        internal Workspace(int shape, bool nested)
        {
            Tools = Enumerable.Range(0, shape == 0 ? 1 : shape == 1 ? 3 : 4)
                .Select(i => new LayoutAnchorable { ContentId = "tool" + i, Content = "tool" + i, CanDockAsTabbedDocument = false }).ToArray();
            FirstPane = new(Tools[0]);
            if (shape > 0) { FirstPane.Children.Add(Tools[1]); FirstPane.Children.Add(Tools[2]); }
            Source = shape switch
            {
                0 => Tools[0], 1 => FirstPane,
                _ => new LayoutAnchorablePaneGroup(FirstPane) { Orientation = Orientation.Vertical,
                    Children = { new LayoutAnchorablePane(Tools[3]) } }
            };
            Region = nested ? new LayoutDocumentPaneGroup(new LayoutDocumentPane(new LayoutDocument { ContentId = "sibling", Content = "sibling" }))
                { Children = { new LayoutDocumentPaneGroup(Target) } } : Target;
            Manager.Layout.RootPanel = new() { Children = { (ILayoutPanelElement)Region, (ILayoutPanelElement)(shape == 0 ? FirstPane : Source) } };
            _window = new() { Width = 1400, Height = 800, Content = Manager };
            _window.Show(); Update();
            Manager.Layout.ActiveContent = Tools[^1];
        }
        internal void Update() { Dispatcher.UIThread.RunJobs(); _window.UpdateLayout(); }
        internal string Save() { using StringWriter writer = new(); new XmlLayoutSerializer(Manager).Serialize(writer); return writer.ToString(); }
        public void Dispose() { Manager.Layout.FloatingWindows.Clear(); _window.Close(); Dispatcher.UIThread.RunJobs(); }
    }
}
