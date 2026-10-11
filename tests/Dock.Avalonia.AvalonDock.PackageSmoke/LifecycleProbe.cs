using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Themes;

namespace PackageConsumer;

internal static class LifecycleProbe
{
    internal static void Verify()
    {
        ProbeManager manager = new();
        Window owner = new() { Width = 400, Height = 300 };
        owner.Styles.Add(new AvalonDockTheme());
        owner.Content = manager;
        try
        {
            owner.Show(); owner.UpdateLayout(); Dispatcher.UIThread.RunJobs();
            int templates = manager.Templates;
            manager.OnApplyTemplate();
            if (manager.Initializations != 1 || templates == 0 || manager.Templates != templates + 1 ||
                manager.AutoHideWindow is null || manager.LayoutRootPanel is null)
                throw new Exception("Packaged manager lifecycle hooks did not initialize their public presentations.");
        }
        finally { owner.Close(); }
        ProbeWindow host = new(new LayoutDocumentFloatingWindow()) { CancelClose = true };
        WindowClosingEventArgs? nativeArgs = null;
        host.Closing += (_, args) => nativeArgs = args;
        try
        {
            host.Show(); Dispatcher.UIThread.RunJobs();
            if (host.Initializations != 1) throw new Exception("The packaged floating initialization hook did not run.");
            host.Close();
            if (!host.IsVisible || !ReferenceEquals(host.ClosingArguments, nativeArgs) || nativeArgs?.Cancel != true)
                throw new Exception("The BCL close hook did not share native cancellation arguments.");
            host.CancelClose = false;
            host.Close();
            if (host.IsVisible) throw new Exception("The packaged floating close hook did not allow a retry.");
        }
        finally { host.CancelClose = false; host.Close(); }
    }

    private sealed class ProbeManager : DockingManager
    {
        public int Initializations { get; private set; }
        public int Templates { get; private set; }
        protected override void OnInitialized(EventArgs e) { Initializations++; base.OnInitialized(e); }
        public override void OnApplyTemplate() { Templates++; base.OnApplyTemplate(); }
    }

    private sealed class ProbeWindow(ILayoutElement model) : LayoutFloatingWindowControl(model)
    {
        public override ILayoutElement Model { get; } = model;
        public int Initializations { get; private set; }
        public bool CancelClose { get; set; }
        public CancelEventArgs? ClosingArguments { get; private set; }
        protected override void OnInitialized(EventArgs e) { Initializations++; base.OnInitialized(e); }
        protected override void OnClosing(CancelEventArgs e)
        {
            ClosingArguments = e;
            e.Cancel |= CancelClose;
            base.OnClosing(e);
        }
    }
}
