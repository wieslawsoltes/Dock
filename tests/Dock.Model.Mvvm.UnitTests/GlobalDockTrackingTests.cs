using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Core.Events;
using Dock.Model.Mvvm.Core;
using Xunit;

namespace Dock.Model.Mvvm.UnitTests;

public class GlobalDockTrackingTests
{
    [Fact]
    public void Initial_State_Is_Empty()
    {
        var factory = new TrackingTestFactory();

        Assert.Null(factory.CurrentDockable);
        Assert.Null(factory.CurrentRootDock);
        Assert.Null(factory.CurrentDockWindow);
        Assert.Null(factory.CurrentHostWindow);
        Assert.Null(factory.GlobalDockTrackingState.Dockable);
        Assert.Null(factory.GlobalDockTrackingState.RootDock);
        Assert.Null(factory.GlobalDockTrackingState.Window);
        Assert.Null(factory.GlobalDockTrackingState.HostWindow);
    }

    [Fact]
    public void WindowActivated_Updates_State_And_Raises_Reason()
    {
        var factory = new TrackingTestFactory();
        var context = CreateContext(factory, "A");
        var host = new TestHostWindow { Window = context.Window };
        context.Window.Host = host;

        GlobalDockTrackingChangedEventArgs? raised = null;
        factory.GlobalDockTrackingChanged += (_, args) => raised = args;

        factory.OnWindowActivated(context.Window);

        Assert.NotNull(raised);
        Assert.Equal(DockTrackingChangeReason.WindowActivated, raised!.Reason);
        Assert.Null(raised.Previous.Dockable);
        Assert.Same(context.Dockable1, raised.Current.Dockable);
        Assert.Same(context.Root, raised.Current.RootDock);
        Assert.Same(context.Window, raised.Current.Window);
        Assert.Same(host, raised.Current.HostWindow);
        Assert.Same(context.Dockable1, factory.CurrentDockable);
        Assert.Same(context.Root, factory.CurrentRootDock);
        Assert.Same(context.Window, factory.CurrentDockWindow);
        Assert.Same(host, factory.CurrentHostWindow);
    }

    [Fact]
    public void WindowActivated_With_Same_Context_Does_Not_Raise_Duplicate_Event()
    {
        var factory = new TrackingTestFactory();
        var context = CreateContext(factory, "A");
        var raised = 0;
        factory.GlobalDockTrackingChanged += (_, _) => raised++;

        factory.OnWindowActivated(context.Window);
        factory.OnWindowActivated(context.Window);

        Assert.Equal(1, raised);
    }

    [Fact]
    public void DockableActivated_From_Tracked_Root_Updates_State_With_Reason()
    {
        var factory = new TrackingTestFactory();
        var context = CreateContext(factory, "A");
        factory.OnWindowActivated(context.Window);

        GlobalDockTrackingChangedEventArgs? raised = null;
        factory.GlobalDockTrackingChanged += (_, args) => raised = args;

        factory.OnDockableActivated(context.Dockable2);

        Assert.NotNull(raised);
        Assert.Equal(DockTrackingChangeReason.DockableActivated, raised!.Reason);
        Assert.Same(context.Dockable1, raised.Previous.Dockable);
        Assert.Same(context.Dockable2, raised.Current.Dockable);
        Assert.Same(context.Dockable2, factory.CurrentDockable);
        Assert.Same(context.Root, factory.CurrentRootDock);
        Assert.Same(context.Window, factory.CurrentDockWindow);
    }

    [Fact]
    public void DockableActivated_From_Different_Root_Does_Not_Change_State()
    {
        var factory = new TrackingTestFactory();
        var first = CreateContext(factory, "A");
        var second = CreateContext(factory, "B");
        factory.OnWindowActivated(first.Window);

        var raised = 0;
        factory.GlobalDockTrackingChanged += (_, _) => raised++;

        factory.OnDockableActivated(second.Dockable1);

        Assert.Equal(0, raised);
        Assert.Same(first.Dockable1, factory.CurrentDockable);
        Assert.Same(first.Root, factory.CurrentRootDock);
        Assert.Same(first.Window, factory.CurrentDockWindow);
    }

    [Fact]
    public void ActiveDockableChanged_From_Tracked_Root_Updates_State_With_Reason()
    {
        var factory = new TrackingTestFactory();
        var context = CreateContext(factory, "A");
        factory.OnWindowActivated(context.Window);

        GlobalDockTrackingChangedEventArgs? raised = null;
        factory.GlobalDockTrackingChanged += (_, args) => raised = args;

        factory.OnActiveDockableChanged(context.Dockable2);

        Assert.NotNull(raised);
        Assert.Equal(DockTrackingChangeReason.ActiveDockableChanged, raised!.Reason);
        Assert.Same(context.Dockable1, raised.Previous.Dockable);
        Assert.Same(context.Dockable2, raised.Current.Dockable);
        Assert.Same(context.Dockable2, factory.CurrentDockable);
        Assert.Same(context.Root, factory.CurrentRootDock);
        Assert.Same(context.Window, factory.CurrentDockWindow);
    }

    [Fact]
    public void FocusedDockableChanged_From_Tracked_Root_Updates_State_With_Reason()
    {
        var factory = new TrackingTestFactory();
        var context = CreateContext(factory, "A");
        factory.OnWindowActivated(context.Window);

        GlobalDockTrackingChangedEventArgs? raised = null;
        factory.GlobalDockTrackingChanged += (_, args) => raised = args;

        factory.OnFocusedDockableChanged(context.Dockable2);

        Assert.NotNull(raised);
        Assert.Equal(DockTrackingChangeReason.FocusedDockableChanged, raised!.Reason);
        Assert.Same(context.Dockable1, raised.Previous.Dockable);
        Assert.Same(context.Dockable2, raised.Current.Dockable);
        Assert.Same(context.Dockable2, factory.CurrentDockable);
        Assert.Same(context.Root, factory.CurrentRootDock);
        Assert.Same(context.Window, factory.CurrentDockWindow);
    }

    [Fact]
    public void ActiveDockableChanged_Null_Without_Current_Root_Does_Not_Raise_Global_Event()
    {
        var factory = new TrackingTestFactory();
        var raised = 0;
        factory.GlobalDockTrackingChanged += (_, _) => raised++;

        factory.OnActiveDockableChanged(null);

        Assert.Equal(0, raised);
        Assert.Null(factory.CurrentDockable);
        Assert.Null(factory.CurrentRootDock);
        Assert.Null(factory.CurrentDockWindow);
    }

    [Fact]
    public void FocusedDockableChanged_Null_Without_Current_Root_Does_Not_Raise_Global_Event()
    {
        var factory = new TrackingTestFactory();
        var raised = 0;
        factory.GlobalDockTrackingChanged += (_, _) => raised++;

        factory.OnFocusedDockableChanged(null);

        Assert.Equal(0, raised);
        Assert.Null(factory.CurrentDockable);
        Assert.Null(factory.CurrentRootDock);
        Assert.Null(factory.CurrentDockWindow);
    }

    [Fact]
    public void FocusedDockableChanged_Null_Reanchors_To_Current_Root()
    {
        var factory = new TrackingTestFactory();
        var context = CreateContext(factory, "A");
        factory.OnWindowActivated(context.Window);
        context.Root.FocusedDockable = context.Dockable2;

        GlobalDockTrackingChangedEventArgs? raised = null;
        factory.GlobalDockTrackingChanged += (_, args) => raised = args;

        factory.OnFocusedDockableChanged(null);

        Assert.NotNull(raised);
        Assert.Equal(DockTrackingChangeReason.FocusedDockableChanged, raised!.Reason);
        Assert.Same(context.Dockable1, raised.Previous.Dockable);
        Assert.Same(context.Dockable2, raised.Current.Dockable);
        Assert.Same(context.Dockable2, factory.CurrentDockable);
        Assert.Same(context.Root, factory.CurrentRootDock);
        Assert.Same(context.Window, factory.CurrentDockWindow);
    }

    [Fact]
    public void WindowDeactivated_For_NonTracked_Window_Does_Not_Change_State()
    {
        var factory = new TrackingTestFactory();
        var first = CreateContext(factory, "A");
        var second = CreateContext(factory, "B");
        factory.OnWindowActivated(first.Window);

        var raised = 0;
        factory.GlobalDockTrackingChanged += (_, _) => raised++;

        factory.OnWindowDeactivated(second.Window);

        Assert.Equal(0, raised);
        Assert.Same(first.Dockable1, factory.CurrentDockable);
        Assert.Same(first.Root, factory.CurrentRootDock);
        Assert.Same(first.Window, factory.CurrentDockWindow);
    }

    [Fact]
    public void WindowDeactivated_For_Tracked_Window_Clears_State_With_Reason()
    {
        var factory = new TrackingTestFactory();
        var context = CreateContext(factory, "A");
        factory.OnWindowActivated(context.Window);

        GlobalDockTrackingChangedEventArgs? raised = null;
        factory.GlobalDockTrackingChanged += (_, args) => raised = args;

        factory.OnWindowDeactivated(context.Window);

        Assert.NotNull(raised);
        Assert.Equal(DockTrackingChangeReason.WindowDeactivated, raised!.Reason);
        Assert.Same(context.Dockable1, raised.Previous.Dockable);
        Assert.Null(raised.Current.Dockable);
        Assert.Null(raised.Current.RootDock);
        Assert.Null(raised.Current.Window);
        Assert.Null(factory.CurrentDockable);
        Assert.Null(factory.CurrentRootDock);
        Assert.Null(factory.CurrentDockWindow);
        Assert.Null(factory.CurrentHostWindow);
    }

    [Fact]
    public void WindowClosed_For_NonTracked_Window_Does_Not_Change_State()
    {
        var factory = new TrackingTestFactory();
        var first = CreateContext(factory, "A");
        var second = CreateContext(factory, "B");
        factory.OnWindowActivated(first.Window);

        var raised = 0;
        factory.GlobalDockTrackingChanged += (_, _) => raised++;

        factory.OnWindowClosed(second.Window);

        Assert.Equal(0, raised);
        Assert.Same(first.Dockable1, factory.CurrentDockable);
        Assert.Same(first.Root, factory.CurrentRootDock);
        Assert.Same(first.Window, factory.CurrentDockWindow);
    }

    [Fact]
    public void WindowClosed_For_Tracked_Window_Clears_State_With_Reason()
    {
        var factory = new TrackingTestFactory();
        var context = CreateContext(factory, "A");
        factory.OnWindowActivated(context.Window);

        GlobalDockTrackingChangedEventArgs? raised = null;
        factory.GlobalDockTrackingChanged += (_, args) => raised = args;

        factory.OnWindowClosed(context.Window);

        Assert.NotNull(raised);
        Assert.Equal(DockTrackingChangeReason.WindowClosed, raised!.Reason);
        Assert.Same(context.Dockable1, raised.Previous.Dockable);
        Assert.Null(raised.Current.Dockable);
        Assert.Null(raised.Current.RootDock);
        Assert.Null(raised.Current.Window);
        Assert.Null(factory.CurrentDockable);
        Assert.Null(factory.CurrentRootDock);
        Assert.Null(factory.CurrentDockWindow);
    }

    [Fact]
    public void WindowRemoved_For_NonTracked_Window_Does_Not_Change_State()
    {
        var factory = new TrackingTestFactory();
        var first = CreateContext(factory, "A");
        var second = CreateContext(factory, "B");
        factory.OnWindowActivated(first.Window);

        var raised = 0;
        factory.GlobalDockTrackingChanged += (_, _) => raised++;

        factory.OnWindowRemoved(second.Window);

        Assert.Equal(0, raised);
        Assert.Same(first.Dockable1, factory.CurrentDockable);
        Assert.Same(first.Root, factory.CurrentRootDock);
        Assert.Same(first.Window, factory.CurrentDockWindow);
    }

    [Fact]
    public void WindowRemoved_For_Tracked_Window_Clears_State_With_Reason()
    {
        var factory = new TrackingTestFactory();
        var context = CreateContext(factory, "A");
        factory.OnWindowActivated(context.Window);

        GlobalDockTrackingChangedEventArgs? raised = null;
        factory.GlobalDockTrackingChanged += (_, args) => raised = args;

        factory.OnWindowRemoved(context.Window);

        Assert.NotNull(raised);
        Assert.Equal(DockTrackingChangeReason.WindowRemoved, raised!.Reason);
        Assert.Same(context.Dockable1, raised.Previous.Dockable);
        Assert.Null(raised.Current.Dockable);
        Assert.Null(raised.Current.RootDock);
        Assert.Null(raised.Current.Window);
        Assert.Null(factory.CurrentDockable);
        Assert.Null(factory.CurrentRootDock);
        Assert.Null(factory.CurrentDockWindow);
    }

    [Fact]
    public void DockableDeactivated_For_Tracked_Dockable_Falls_Back_To_Focused_Dockable()
    {
        var factory = new TrackingTestFactory();
        var context = CreateContext(factory, "A");
        factory.OnWindowActivated(context.Window);
        context.Root.FocusedDockable = context.Dockable2;

        GlobalDockTrackingChangedEventArgs? raised = null;
        factory.GlobalDockTrackingChanged += (_, args) => raised = args;

        factory.OnDockableDeactivated(context.Dockable1);

        Assert.NotNull(raised);
        Assert.Equal(DockTrackingChangeReason.DockableDeactivated, raised!.Reason);
        Assert.Same(context.Dockable1, raised.Previous.Dockable);
        Assert.Same(context.Dockable2, raised.Current.Dockable);
        Assert.Same(context.Dockable2, factory.CurrentDockable);
        Assert.Same(context.Root, factory.CurrentRootDock);
        Assert.Same(context.Window, factory.CurrentDockWindow);
    }

    [Fact]
    public void DockableDeactivated_For_Tracked_Dockable_Falls_Back_To_Active_Dockable()
    {
        var factory = new TrackingTestFactory();
        var context = CreateContext(factory, "A");
        factory.OnWindowActivated(context.Window);
        context.Root.FocusedDockable = context.Dockable1;
        context.Root.ActiveDockable = context.Dockable2;

        GlobalDockTrackingChangedEventArgs? raised = null;
        factory.GlobalDockTrackingChanged += (_, args) => raised = args;

        factory.OnDockableDeactivated(context.Dockable1);

        Assert.NotNull(raised);
        Assert.Equal(DockTrackingChangeReason.DockableDeactivated, raised!.Reason);
        Assert.Same(context.Dockable1, raised.Previous.Dockable);
        Assert.Same(context.Dockable2, raised.Current.Dockable);
        Assert.Same(context.Dockable2, factory.CurrentDockable);
        Assert.Same(context.Root, factory.CurrentRootDock);
        Assert.Same(context.Window, factory.CurrentDockWindow);
    }

    [Fact]
    public void DockableDeactivated_For_Tracked_Dockable_Clears_Current_Dockable_When_No_Fallback()
    {
        var factory = new TrackingTestFactory();
        var context = CreateContext(factory, "A");
        factory.OnWindowActivated(context.Window);
        context.Root.FocusedDockable = context.Dockable1;
        context.Root.ActiveDockable = context.Dockable1;

        GlobalDockTrackingChangedEventArgs? raised = null;
        factory.GlobalDockTrackingChanged += (_, args) => raised = args;

        factory.OnDockableDeactivated(context.Dockable1);

        Assert.NotNull(raised);
        Assert.Equal(DockTrackingChangeReason.DockableDeactivated, raised!.Reason);
        Assert.Same(context.Dockable1, raised.Previous.Dockable);
        Assert.Null(raised.Current.Dockable);
        Assert.Same(context.Root, raised.Current.RootDock);
        Assert.Same(context.Window, raised.Current.Window);
        Assert.Null(factory.CurrentDockable);
        Assert.Same(context.Root, factory.CurrentRootDock);
        Assert.Same(context.Window, factory.CurrentDockWindow);
    }

    [Fact]
    public void DockableDeactivated_For_Untracked_Dockable_Does_Not_Change_State()
    {
        var factory = new TrackingTestFactory();
        var first = CreateContext(factory, "A");
        var second = CreateContext(factory, "B");
        factory.OnWindowActivated(first.Window);

        var raised = 0;
        factory.GlobalDockTrackingChanged += (_, _) => raised++;

        factory.OnDockableDeactivated(second.Dockable1);

        Assert.Equal(0, raised);
        Assert.Same(first.Dockable1, factory.CurrentDockable);
        Assert.Same(first.Root, factory.CurrentRootDock);
        Assert.Same(first.Window, factory.CurrentDockWindow);
    }

    private static TrackingContext CreateContext(TrackingTestFactory factory, string idSuffix)
    {
        var root = factory.CreateRootDock();
        root.Id = $"root-{idSuffix}";

        var window = factory.CreateDockWindow();
        window.Id = $"window-{idSuffix}";

        var dock = factory.CreateDocumentDock();
        dock.Id = $"dock-{idSuffix}";

        var dockable1 = factory.CreateDocument();
        dockable1.Id = $"doc-{idSuffix}-1";
        dockable1.Title = $"Document-{idSuffix}-1";

        var dockable2 = factory.CreateDocument();
        dockable2.Id = $"doc-{idSuffix}-2";
        dockable2.Title = $"Document-{idSuffix}-2";

        dock.VisibleDockables = factory.CreateList<IDockable>(dockable1, dockable2);
        dock.Owner = root;
        dock.ActiveDockable = dockable1;
        dock.FocusedDockable = dockable1;
        dockable1.Owner = dock;
        dockable2.Owner = dock;

        root.VisibleDockables = factory.CreateList<IDockable>(dock);
        root.ActiveDockable = dock;
        root.FocusedDockable = dockable1;

        window.Layout = root;
        root.Window = window;

        return new TrackingContext(root, window, dock, dockable1, dockable2);
    }

    private sealed record TrackingContext(
        IRootDock Root,
        IDockWindow Window,
        IDock Dock,
        IDockable Dockable1,
        IDockable Dockable2);

    private sealed class TrackingTestFactory : Factory
    {
    }

    private sealed class TestHostWindow : IHostWindow
    {
        public IHostWindowState? HostWindowState => null;

        public bool IsTracked { get; set; }

        public IDockWindow? Window { get; set; }

        public void Present(bool isDialog)
        {
        }

        public void Exit()
        {
        }

        public void SetPosition(double x, double y)
        {
        }

        public void GetPosition(out double x, out double y)
        {
            x = 0;
            y = 0;
        }

        public void SetSize(double width, double height)
        {
        }

        public void GetSize(out double width, out double height)
        {
            width = 0;
            height = 0;
        }

        public void SetWindowState(DockWindowState windowState)
        {
        }

        public DockWindowState GetWindowState() => DockWindowState.Normal;

        public void SetTitle(string? title)
        {
        }

        public void SetLayout(IDock layout)
        {
        }

        public void SetActive()
        {
        }
    }
}
