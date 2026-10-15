using System;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class FloatingInitializationTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_Initialization_Can_Precede_The_Typed_Constructor_Without_Losing_Model_Subscriptions(bool tools)
    {
        LayoutDocumentFloatingWindow document = new() { RootDocument = new LayoutDocument() };
        LayoutAnchorableFloatingWindow tool = new() { RootPanel = new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(new LayoutAnchorable())) };
        using Probe probe = new(tools ? new ToolHost(tool) : new DocumentHost(document));
        probe.Host.Window.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.True(probe.Host.Window.IsVisible);
        if (tools) tool.RootPanel = null;
        else document.RootDocument = null;
        Assert.False(probe.Host.UserOrigin);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(1, probe.Closed);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Suppressing_The_Typed_Initialization_Hook_Suppresses_Model_Subscriptions(bool tools)
    {
        LayoutDocumentFloatingWindow document = new() { RootDocument = new LayoutDocument() };
        LayoutAnchorableFloatingWindow tool = new() { RootPanel = new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(new LayoutAnchorable())) };
        using Probe probe = new(tools ? new ToolHost(tool, true) : new DocumentHost(document, true));
        probe.Host.Window.Show();
        document.RootDocument = null;
        tool.RootPanel = null;
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(0, probe.Closed);
        Assert.True(probe.Host.UserOrigin);
        Assert.True(probe.Host.Window.IsVisible);
    }

    [AvaloniaFact]
    public void Initializing_An_Empty_Document_Host_Queues_Closure()
    {
        using Probe probe = new(new DocumentHost(new LayoutDocumentFloatingWindow()));
        probe.Host.Window.Show();
        Assert.False(probe.Host.UserOrigin);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(1, probe.Closed);
    }

    private interface IProbeHost
    {
        LayoutFloatingWindowControl Window { get; }
        bool UserOrigin { get; }
    }

    private sealed class DocumentHost(LayoutDocumentFloatingWindow model, bool suppress = false) : LayoutDocumentFloatingWindowControl(model), IProbeHost
    {
        LayoutFloatingWindowControl IProbeHost.Window => this;
        public bool UserOrigin => CloseInitiatedByUser;
        protected override bool CanClose(object? parameter = null) => true;
        protected override void OnInitialized(EventArgs e) { if (!suppress) base.OnInitialized(e); }
    }

    private sealed class ToolHost(LayoutAnchorableFloatingWindow model, bool suppress = false) : LayoutAnchorableFloatingWindowControl(model), IProbeHost
    {
        LayoutFloatingWindowControl IProbeHost.Window => this;
        public bool UserOrigin => CloseInitiatedByUser;
        protected override bool CanClose(object? parameter = null) => true;
        protected override void OnInitialized(EventArgs e) { if (!suppress) base.OnInitialized(e); }
    }

    private sealed class Probe : IDisposable
    {
        public IProbeHost Host { get; }
        public int Closed { get; private set; }
        public Probe(IProbeHost host) { Host = host; host.Window.Closed += (_, _) => Closed++; }
        public void Dispose() => Host.Window.Close();
    }
}
