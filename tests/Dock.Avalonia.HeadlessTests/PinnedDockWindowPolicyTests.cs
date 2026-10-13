using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class PinnedDockWindowPolicyTests
{
    [AvaloniaTheory]
    [InlineData(false, null, false)]
    [InlineData(true, null, true)]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public void Per_Control_Policy_Overrides_The_Global_Default(bool global, bool? local, bool expectedWindow)
    {
        using Workspace workspace = new(global, local);
        workspace.AssertPresentation(expectedWindow);
        Assert.Equal(local, workspace.Control.UsePinnedDockWindow);
        Assert.Equal(global, DockSettings.UsePinnedDockWindow);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Live_Changes_And_Clear_Preserve_The_Open_Preview(bool global)
    {
        using Workspace workspace = new(global, false);
        ToolDock preview = Assert.IsType<ToolDock>(workspace.Root.PinnedDock);
        workspace.AssertPresentation(false);
        workspace.Control.UsePinnedDockWindow = true;
        workspace.Update();
        PinnedDockWindow firstWindow = Assert.Single(workspace.Owner.OwnedWindows.OfType<PinnedDockWindow>());
        bool closed = false;
        firstWindow.Closed += (_, _) => closed = true;
        workspace.AssertPresentation(true);

        workspace.Control.UsePinnedDockWindow = false;
        workspace.Update();
        Assert.True(closed);
        workspace.AssertPresentation(false);
        Assert.Same(preview, workspace.Root.PinnedDock);
        Assert.Same(workspace.Tool, preview.ActiveDockable);

        workspace.Control.ClearValue(PinnedDockControl.UsePinnedDockWindowProperty);
        workspace.Update();
        workspace.AssertPresentation(global);
        Assert.Same(preview, workspace.Root.PinnedDock);
        workspace.Tool.GetPinnedBounds(out _, out _, out double width, out double height);
        Assert.Equal(220, width);
        Assert.Equal(190, height);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Inline_Presentation_Does_Not_Create_A_Separate_Window(bool global)
    {
        using Workspace workspace = new(global, true, PinnedDockDisplayMode.Inline);
        workspace.AssertPresentation(false);
        workspace.Control.PinnedDockDisplayMode = PinnedDockDisplayMode.Overlay;
        workspace.Update();
        workspace.AssertPresentation(true);
        workspace.Control.PinnedDockDisplayMode = PinnedDockDisplayMode.Inline;
        workspace.Update();
        workspace.AssertPresentation(false);
    }

    [AvaloniaFact]
    public void Style_Removal_Restores_Global_Window_Presentation()
    {
        using Workspace workspace = new(true, null);
        Style style = new(selector => selector.OfType<PinnedDockControl>().Class("embedded-preview"))
        {
            Setters = { new Setter(PinnedDockControl.UsePinnedDockWindowProperty, false) }
        };
        workspace.Owner.Styles.Add(style);
        workspace.Control.Classes.Add("embedded-preview");
        workspace.Update();
        workspace.AssertPresentation(false);
        workspace.Control.Classes.Remove("embedded-preview");
        workspace.Update();
        Assert.Null(workspace.Control.UsePinnedDockWindow);
        workspace.AssertPresentation(true);
    }

    [AvaloniaFact]
    public void Embedded_Presentation_Preserves_Template_Opacity_And_Hit_Test_Values()
    {
        using Workspace workspace = new(true, false);
        ContentControl content = workspace.Content;
        content.Opacity = 0;
        content.IsHitTestVisible = false;
        workspace.Control.UsePinnedDockWindow = null;
        workspace.Control.UsePinnedDockWindow = false;
        // The real window has now restored the normal template presentation.
        // Subsequent layout without a window must preserve application changes.
        content.Opacity = 0;
        content.IsHitTestVisible = false;
        workspace.Control.InvalidateMeasure();
        workspace.Update();
        Assert.Equal(0, content.Opacity);
        Assert.False(content.IsHitTestVisible);
        Assert.Empty(workspace.Owner.OwnedWindows.OfType<PinnedDockWindow>());
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Managed_Presentation_Responds_To_The_Same_Per_Control_Policy(bool global)
    {
        using Workspace workspace = new(global, false, managed: true);
        Assert.Empty(workspace.Layer.GetVisualDescendants().OfType<ToolDockControl>());
        workspace.Control.UsePinnedDockWindow = true;
        workspace.Update();
        ToolDockControl overlay = Assert.Single(workspace.Layer.GetVisualDescendants().OfType<ToolDockControl>());
        Assert.Same(workspace.Root.PinnedDock, overlay.DataContext);
        Assert.Equal(0, workspace.Content.Opacity);
        Assert.Empty(workspace.Owner.OwnedWindows);

        workspace.Control.UsePinnedDockWindow = false;
        workspace.Update();
        Assert.Empty(workspace.Layer.GetVisualDescendants().OfType<ToolDockControl>());
        Assert.Equal(1, workspace.Content.Opacity);
        Assert.True(workspace.Content.IsHitTestVisible);
        Assert.Same(workspace.Tool, workspace.Root.PinnedDock!.ActiveDockable);

        workspace.Control.ClearValue(PinnedDockControl.UsePinnedDockWindowProperty);
        workspace.Update();
        Assert.Equal(global ? 1 : 0, workspace.Layer.GetVisualDescendants().OfType<ToolDockControl>().Count());
        Assert.Empty(workspace.Owner.OwnedWindows);
    }

    private sealed class Workspace : IDisposable
    {
        private readonly bool _previous = DockSettings.UsePinnedDockWindow;
        private readonly bool _previousManaged = DockSettings.UseManagedWindows;
        public Tool Tool { get; } = new() { KeepPinnedDockableVisible = true };
        public RootDock Root { get; }
        public PinnedDockControl Control { get; }
        public Window Owner { get; }
        public ManagedWindowLayer Layer { get; } = new();
        public ContentControl Content => Control.GetVisualDescendants().OfType<ContentControl>()
            .Single(control => control.Name == "PART_PinnedDock");

        public Workspace(bool global, bool? local, PinnedDockDisplayMode mode = PinnedDockDisplayMode.Overlay, bool managed = false)
        {
            DockSettings.UsePinnedDockWindow = global;
            DockSettings.UseManagedWindows = managed;
            Factory factory = new();
            Tool.SetPinnedBounds(0, 0, 220, 190);
            Root = new RootDock
            {
                LeftPinnedDockables = factory.CreateList<IDockable>(Tool),
                VisibleDockables = factory.CreateList<IDockable>()
            };
            factory.InitLayout(Root);
            Control = new PinnedDockControl { DataContext = Root, PinnedDockDisplayMode = mode };
            if (local.HasValue) Control.UsePinnedDockWindow = local;
            Owner = new Window
            {
                Width = 800, Height = 600,
                Content = managed ? new Grid { Children = { Control, Layer } } : Control
            };
            Owner.Show();
            factory.PreviewPinnedDockable(Tool);
            Update();
        }

        public void Update()
        {
            Dispatcher.UIThread.RunJobs();
            Owner.UpdateLayout();
        }

        public void AssertPresentation(bool window)
        {
            Assert.Equal(window ? 1 : 0, Owner.OwnedWindows.OfType<PinnedDockWindow>().Count());
            Assert.Equal(window ? 0 : 1, Content.Opacity);
            Assert.Equal(!window, Content.IsHitTestVisible);
            Assert.Same(Tool, Root.PinnedDock!.ActiveDockable);
        }

        public void Dispose()
        {
            Owner.Close();
            DockSettings.UsePinnedDockWindow = _previous;
            DockSettings.UseManagedWindows = _previousManaged;
        }
    }
}
