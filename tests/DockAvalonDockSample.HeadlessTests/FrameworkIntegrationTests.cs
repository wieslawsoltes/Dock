using System;
using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Xunit;

namespace DockAvalonDockSample.HeadlessTests;

public class FrameworkIntegrationTests
{
    [AvaloniaFact]
    public void Public_Registrations_Preserve_Defaults_And_Coercion()
    {
        DockingManager manager = new();
        Assert.Equal(1500, manager.AutoHideWindowClosingTimer);
        manager.AutoHideWindowClosingTimer = -1;
        Assert.Equal(1500, manager.AutoHideWindowClosingTimer);
        manager.AutoHideWindowClosingTimer = 250;
        Assert.Equal(250, manager.AutoHideWindowClosingTimer);
        LayoutGridResizerControl splitter = new();
        Assert.Equal(0.5, splitter.OpacityWhileDragging);
        Assert.NotNull(splitter.BackgroundWhileDragging);
    }

    [AvaloniaFact]
    public void Logical_Projection_Uses_The_Protected_Hook_Without_Framework_Extensions()
    {
        ProjectionManager manager = new();
        TextBlock child = new();
        manager.ProjectedChild = child;
        Assert.Same(child, Assert.Single(((ILogical)manager).LogicalChildren));
        manager.ProjectedChild = null;
        Assert.Empty(((ILogical)manager).LogicalChildren);
    }

    [AvaloniaFact]
    public void Floating_Initialization_Sees_The_Model_And_Runs_Once()
    {
        LayoutDocumentFloatingWindow model = new() { RootDocument = new LayoutDocument { Title = "Document" } };
        ProbeWindow window = new(model);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, window.Initializations);
            Assert.Same(model, window.InitializedModel);
            window.Hide();
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, window.Initializations);
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Floating_Drag_Captures_The_Event_Pointer_And_Releases_It(bool close)
    {
        ProbeWindow window = new(new LayoutDocumentFloatingWindow { RootDocument = new LayoutDocument() })
        {
            Width = 400, Height = 300, Content = new Border()
        };
        IPointer? pointer = null;
        window.AddHandler(InputElement.PointerPressedEvent, (_, args) => pointer = args.Pointer,
            global::Avalonia.Interactivity.RoutingStrategies.Tunnel, handledEventsToo: true);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.MouseDown(new Point(100, 100), MouseButton.Left);
            window.ChangeDragging(true);
            Assert.NotNull(pointer);
            Assert.Same(window, pointer.Captured);
            if (close) window.Close();
            else window.ChangeDragging(false);
            Assert.NotSame(window, pointer.Captured);
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    private sealed class ProjectionManager : DockingManager
    {
        internal Control? ProjectedChild { get; set; }
        protected override IEnumerator LogicalChildren =>
            ProjectedChild is { } child ? new[] { child }.GetEnumerator() : Array.Empty<object>().GetEnumerator();
    }

    private sealed class ProbeWindow(LayoutDocumentFloatingWindow model) : LayoutDocumentFloatingWindowControl(model)
    {
        internal int Initializations { get; private set; }
        internal ILayoutElement? InitializedModel { get; private set; }
        internal void ChangeDragging(bool value) => SetIsDragging(value);
        protected override bool CanClose(object? parameter = null) => true;

        protected override void OnInitialized(EventArgs e)
        {
            InitializedModel = Model;
            Initializations++;
            base.OnInitialized(e);
        }
    }
}
