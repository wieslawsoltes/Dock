using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Logging;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using DockBrowserTheme = Dock.Avalonia.Themes.Browser.BrowserTabTheme;
using Dock.Avalonia.Themes.Simple;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class CapabilityPolicyBindingWarningsTests
{
    [AvaloniaTheory]
    [InlineData(0, false)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(2, true)]
    public void Default_And_Floating_Layouts_Do_Not_Log_Capability_Warnings(int theme, bool alternateModel)
    {
#if AVALONIA_11
        IFactory factory = alternateModel ? new Model.ReactiveUI.Factory() : new Model.Mvvm.Factory();
#else
        IFactory factory = alternateModel ? new Model.Avalonia.Factory() : new Model.Mvvm.Factory();
#endif
        var first = factory.CreateDocument();
        first.Id = first.Title = "First";
        var second = factory.CreateDocument();
        second.Id = second.Title = "Second";
        var tool = factory.CreateTool();
        tool.Id = tool.Title = "Tool";
        var documents = factory.CreateDocumentDock();
        documents.VisibleDockables = factory.CreateList<IDockable>(first, second);
        documents.ActiveDockable = first;
        var tools = factory.CreateToolDock();
        tools.VisibleDockables = factory.CreateList<IDockable>(tool);
        tools.ActiveDockable = tool;
        var proportional = factory.CreateProportionalDock();
        proportional.VisibleDockables = factory.CreateList<IDockable>(documents, factory.CreateProportionalDockSplitter(), tools);
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>(proportional);
        root.ActiveDockable = root.DefaultDockable = proportional;
        factory.InitLayout(root);

        Assert.Null(first.DockCapabilityOverrides);
        Assert.Null(documents.DockCapabilityPolicy);
        var previousSink = Logger.Sink;
        var sink = new CapabilityWarningSink();
        var window = new Window { Width = 900, Height = 700 };
        window.Styles.Add(new FluentTheme());
        window.Styles.Add(CreateTheme(theme));
        factory.DefaultHostWindowLocator = () =>
        {
            var host = new HostWindow();
            host.Styles.Add(new FluentTheme());
            host.Styles.Add(CreateTheme(theme));
            return host;
        };

        try
        {
            Logger.Sink = sink;
            window.Content = new DockControl { Factory = factory, Layout = root };
            window.Show();
            Pump(window);
            Assert.Empty(sink.Warnings);

            documents.DockCapabilityPolicy = new DockCapabilityPolicy { CanDrag = false };
            first.DockCapabilityOverrides = new DockCapabilityOverrides { CanDrag = true };
            Pump(window);
            first.DockCapabilityOverrides = null;
            documents.DockCapabilityPolicy = null;
            Pump(window);
            Assert.Empty(sink.Warnings);

            factory.FloatDockable(first);
            Pump(window);
            var floating = Assert.Single(root.Windows!);
            Assert.NotNull(floating.Host);
            Assert.NotSame(documents, first.Owner);
            Assert.Null(Assert.IsAssignableFrom<IDock>(first.Owner).DockCapabilityPolicy);
            Assert.Empty(sink.Warnings);
        }
        finally
        {
            if (root.Windows is { } windows)
            {
                foreach (var floating in new List<IDockWindow>(windows))
                    floating.Host?.Exit();
            }
            window.Close();
            Logger.Sink = previousSink;
        }
    }

    private static IStyle CreateTheme(int theme) => theme switch
    {
        0 => new DockFluentTheme(),
        1 => new DockSimpleTheme(),
        2 => new DockBrowserTheme(),
        _ => throw new ArgumentOutOfRangeException(nameof(theme))
    };

    private static void Pump(Window window)
    {
        window.UpdateLayout();
        for (var i = 0; i < 5; ++i)
            Dispatcher.UIThread.RunJobs();
    }

    internal sealed class CapabilityWarningSink : ILogSink
    {
        public List<string> Warnings { get; } = new();

        public bool IsEnabled(LogEventLevel level, string area) => level >= LogEventLevel.Warning && area == LogArea.Binding;

        public void Log(LogEventLevel level, string area, object? source, string messageTemplate) =>
            Log(level, area, source, messageTemplate, Array.Empty<object?>());

        public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
        {
            var message = messageTemplate + " " + string.Join(" | ", propertyValues);
            if (message.Contains("DockCapability", StringComparison.Ordinal))
                Warnings.Add(message);
        }
    }
}
