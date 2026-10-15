using System;
using System.Collections.Generic;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

// A creation request affects only hosts containing its content. Nested event
// handlers may create or float other contents without inheriting its options.
internal sealed class LayoutFloatingWindowCreation(DockingManager manager, LayoutHostWindowFactory hosts)
{
    private Request? _request;

    internal LayoutFloatingWindowControl? Create(LayoutContent content, bool immutable)
    {
        ArgumentNullException.ThrowIfNull(content);
        LayoutRoot layout = manager.Layout;
        layout.Context.Adopt(content.Context);
        if (content is LayoutAnchorable tool && content.Parent is not ILayoutPane) _ = new LayoutAnchorablePane(tool);
        Request request = new(content, immutable, _request);
        _request = request;
        try
        {
            LayoutFloatingWindow? previous = content.FindParent<LayoutFloatingWindow>();
            using IDockOperationScope? notification = layout.Operations.Begin(content.DockModel, LayoutOperation.Float);
            if (!ReferenceEquals(manager.Layout, layout) || !content.CanFloat) return null;
            if (content is LayoutDocument && content.Parent is null)
                throw new InvalidOperationException("A document must belong to a pane before creating its floating window.");
            if (!ReferenceEquals(content.Root, layout))
            {
                if (!TransferToWindow(layout, content, previous is null)) return null;
            }
            else
            {
                content.DockModel.SetVisibleBounds(content.FloatingLeft, content.FloatingTop, content.FloatingWidth, content.FloatingHeight);
                layout.DockFactory.FloatDockable(content.DockModel);
            }

            if (!ReferenceEquals(manager.Layout, layout) || content.FindParent<LayoutFloatingWindow>() is not { } model ||
                ReferenceEquals(model, previous)) return null;
            if (model.DockWindow.Host is null)
            {
                model.DockWindow.Host = hosts.Create(model);
                model.DockWindow.Present(model.DockWindow.IsModal);
            }
            if (model.DockWindow.Host is not LayoutFloatingWindowControl control)
                throw new InvalidOperationException("The native host locator must create an AvalonDock floating-window control.");
            layout.CollectGarbage();
            notification?.Complete();
            return control.HasClosed || !ReferenceEquals(manager.Layout, layout) ? null : control;
        }
        finally
        {
            _request = request.Previous;
            foreach (LayoutFloatingWindowControl control in request.Hosts) control.DeferNativePresentation = false;
        }
    }

    private static bool TransferToWindow(LayoutRoot layout, LayoutContent content, bool remember)
    {
        LayoutRoot? sourceRoot = content.Root as LayoutRoot;
        ILayoutContainer? parent = content.Parent;
        int index = parent is ILayoutGroup group ? group.IndexOfChild(content) : 0;
        double x = content.FloatingLeft;
        double y = content.FloatingTop;
        double width = content.FloatingWidth;
        double height = content.FloatingHeight;
        bool maximized = content.IsMaximized;
        IDockWindow? window = layout.DockFactory.CreateWindowFrom(content.DockModel);
        if (window is null) return false;
        parent?.RemoveChild(content);
        window.X = x;
        window.Y = y;
        if (width > 0 && double.IsFinite(width)) window.Width = width;
        if (height > 0 && double.IsFinite(height)) window.Height = height;
        window.WindowState = maximized ? DockWindowState.Maximized : DockWindowState.Normal;
        LayoutFloatingWindow model = layout.Context.GetWindow(window);
        if (parent is ILayoutPositionableGroup position && model is LayoutAnchorableFloatingWindow { RootPanel: { } panel })
        {
            LayoutFloatingPlacement.CopyPosition(position, panel);
            if (panel.Children.Count == 1 && panel.Children[0] is ILayoutPositionableGroup pane)
                LayoutFloatingPlacement.CopyPosition(position, pane);
        }
        layout.FloatingWindows.Add(model);
        if (remember) content.DockingHistory.Remember(parent, index);
        sourceRoot?.CollectGarbage();
        return true;
    }

    internal void Configure(LayoutFloatingWindow model, LayoutFloatingWindowControl host)
    {
        for (Request? request = _request; request is not null; request = request.Previous)
        {
            foreach (ILayoutElement child in model.Descendents())
            {
                if (!ReferenceEquals(child, request.Content)) continue;
                host.SetCurrentValue(LayoutFloatingWindowControl.IsContentImmutableProperty, request.Immutable);
                host.DeferNativePresentation = true;
                request.Hosts.Add(host);
                return;
            }
        }
    }

    private sealed class Request(LayoutContent content, bool immutable, Request? previous)
    {
        internal LayoutContent Content { get; } = content;
        internal bool Immutable { get; } = immutable;
        internal Request? Previous { get; } = previous;
        internal List<LayoutFloatingWindowControl> Hosts { get; } = new(1);
    }
}
