using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Themes;

namespace PackageConsumer;

internal static class ThemeLifetimeProbe
{
    internal static void Verify()
    {
        foreach (bool duringCreation in new[] { false, true })
        {
            DockingManager manager = new() { Theme = new Dictionary(new() { ["ThemeMarker"] = "previous" }) };
            LayoutDocument document = new() { Title = "Theme lifetime", Content = "Document" };
            manager.Layout = new() { RootPanel = new(new LayoutDocumentPane(document)) };
            Window owner = new() { Width = 800, Height = 500, Content = manager };
            owner.Show();
            Dispatcher.UIThread.RunJobs();
            document.Float();
            Dispatcher.UIThread.RunJobs();
            Window target = manager.FloatingWindows.Single();
            bool closing = false;
            try
            {
                if (!duringCreation) target.ResourcesChanged += (_, _) => CloseAfterRemoval();
                manager.Theme = duringCreation
                    ? new CallbackTheme(CloseAfterRemoval)
                    : new Dictionary(new() { ["ThemeMarker"] = "replacement" });
                if (!closing || target.IsVisible || target.Resources.MergedDictionaries.Count != 0)
                    throw new Exception("A floating window retained replacement resources after closing during a theme change.");
                manager.Theme = new GenericTheme();
                if (target.Resources.MergedDictionaries.Count != 0)
                    throw new Exception("A later theme change reattached resources to the closed window.");
            }
            finally
            {
                target.Close();
                manager.Layout.FloatingWindows.Clear();
                manager.Theme = null;
                owner.Close();
                Dispatcher.UIThread.RunJobs();
            }

            void CloseAfterRemoval()
            {
                if (closing || target.Resources.TryGetResource("ThemeMarker", null, out _)) return;
                closing = true;
                target.Close();
            }
        }
    }

    private sealed class Dictionary(ResourceDictionary resources) : DictionaryTheme(resources) { }

    private sealed class CallbackTheme(Action callback) : Theme
    {
        public override Uri GetResourceUri()
        {
            callback();
            return new GenericTheme().GetResourceUri();
        }
    }
}
