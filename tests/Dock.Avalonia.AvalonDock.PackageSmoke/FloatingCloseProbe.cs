using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Themes;
using Dock.Model.Core;

namespace PackageConsumer;

internal static class FloatingCloseProbe
{
    internal static void Verify()
    {
        LayoutDocument document = new() { Title = "Document", Content = "Content" };
        DockingManager manager = new() { Layout = new() { RootPanel = new(new LayoutDocumentPane(document)) } };
        Window owner = new() { Width = 640, Height = 400 };
        owner.Styles.Add(new AvalonDockTheme());
        owner.Content = manager;
        try
        {
            owner.Show(); Pump();
            document.Float(); Pump();
            LayoutFloatingWindowControl host = manager.FloatingWindows.Single();
            LayoutFloatingWindow model = (LayoutFloatingWindow)host.Model;
            int closing = 0;
            document.Closing += (_, args) => { closing++; args.Cancel = true; };
            if (manager.DockFactory is not IDockWindowClosePolicy policy || !policy.PreserveLayoutOnHostClose(model.DockWindow))
                throw new Exception("The packaged factory did not opt into preserving the floating layout.");
            host.Close();
            if (host.IsVisible || closing != 0 || !ReferenceEquals(document.Root, manager.Layout) ||
                model.DockWindow.Host is not null || manager.FloatingWindows.Any())
                throw new Exception("Window.Close did not preserve content independently of its close command.");
            owner.Content = null;
            owner.Content = manager; Pump();
            LayoutFloatingWindowControl replacement = manager.FloatingWindows.Single();
            if (ReferenceEquals(host, replacement) || !ReferenceEquals(replacement.Model, model))
                throw new Exception("The preserved model did not receive a new host after reattachment.");
            manager.Layout = new(); Pump();
            if (replacement.IsVisible || document.Root is not null || closing != 0)
                throw new Exception("Layout replacement did not detach the old floating model without content closure.");

            LayoutAnchorable tool = new() { Title = "Tool", Content = "Tool content", CanClose = false };
            manager.Layout.RootPanel.Children.Add(new LayoutAnchorablePane(tool));
            tool.Float(); Pump();
            LayoutFloatingWindowControl toolHost = manager.FloatingWindows.Single();
            List<string> events = new();
            tool.Hiding += (_, _) => events.Add("hiding");
            toolHost.Closing += (_, args) => events.Add(args.Cancel ? "canceled" : "closing");
            toolHost.Close();
            if (!events.SequenceEqual(new[] { "hiding", "canceled" }) || toolHost.IsVisible || !tool.IsHidden ||
                ((IHostWindowCloseState)toolHost).IsClosed)
                throw new Exception("Tool hiding did not precede the canceled window Closing event.");
            tool.Show();
            if (!toolHost.IsVisible) throw new Exception("The hidden tool host could not be restored.");
            tool.Close();
            if (((IHostWindowCloseState)toolHost).IsClosed || tool.Root is not null ||
                manager.Layout.FloatingWindows.Count != 0 || !ReferenceEquals(manager.FloatingWindows.Single(), toolHost))
                throw new Exception("Empty tool removal did not precede deferred host closure.");
            Pump();
            if (!((IHostWindowCloseState)toolHost).IsClosed || manager.DockFactory.HostWindows.Count != 0 ||
                !ReferenceEquals(manager.FloatingWindows.Single(), toolHost))
                throw new Exception("Deferred tool closure did not release native tracking while preserving the public entry.");

            manager.Layout = new();
            LayoutDocument deferred = new() { Title = "Deferred document" };
            ((LayoutDocumentPane)manager.Layout.RootPanel.Children[0]).Children.Add(deferred);
            deferred.Float(); Pump();
            LayoutFloatingWindowControl deferredHost = manager.FloatingWindows.Single();
            bool cancelHost = true;
            deferredHost.Closing += (_, args) => args.Cancel = cancelHost;
            deferred.Close();
            if (!deferredHost.IsVisible || !ReferenceEquals(deferred.Root, manager.Layout))
                throw new Exception("Document closure detached its public root before the deferred host close.");
            Pump();
            if (((IHostWindowCloseState)deferredHost).IsClosed || !ReferenceEquals(deferred.Root, manager.Layout))
                throw new Exception("Canceled deferred document closure did not preserve the original root.");
            cancelHost = false;
            deferredHost.Close();
            if (deferred.Root is not null || !ReferenceEquals(deferred.Parent, deferredHost.Model) ||
                manager.FloatingWindows.Any() || manager.DockFactory.HostWindows.Count != 0)
                throw new Exception("Explicit document host retry did not complete deferred cleanup.");
        }
        finally { manager.Layout.FloatingWindows.Clear(); owner.Close(); Pump(); }
    }

    private static void Pump() => Dispatcher.UIThread.RunJobs();
}
