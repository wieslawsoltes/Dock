using System;
using System.IO;
using System.Linq;
using System.Xml;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using AvalonDock.Themes;

namespace PackageConsumer;

internal static class RegisteredExtensionsProbe
{
    internal static void Verify()
    {
        EditorDocument document = new() { ContentId = "editor", Content = new object(), FileName = "editor.cs" };
        LayoutAnchorable tool = new() { ContentId = "tool", Content = new object() };
        LayoutPanel panel = new() { Children = { new LayoutDocumentPane(document), new LayoutAnchorablePane(tool) } };
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = panel } };
        Uri uri = new("avares://Dock.Avalonia.AvalonDock.PackageSmoke/RegisteredResources.axaml");
        manager.ThemeResources.Register(uri, static () => new RegisteredResources());
        manager.Theme = new ResourceTheme(uri);
        Window owner = new() { Content = manager, Width = 640, Height = 480 };
        owner.Styles.Add(new AvalonDockTheme());
        try
        {
            owner.Show();
            Dispatcher.UIThread.RunJobs();
            owner.UpdateLayout();
            manager.DockFactory.FloatDockable(panel.DockModel);
            Dispatcher.UIThread.RunJobs();
            if (manager.Layout.FloatingWindows.Single() is not LayoutGroupFloatingWindow group ||
                !ReferenceEquals(panel, group.RootPanels.Single()) ||
                manager.FloatingWindows.Single() is not LayoutGroupFloatingWindowControl host)
                throw new Exception("Packaged mixed floating groups did not retain their model and host.");
            if (host.FindResource("RegisteredBrush") is not ISolidColorBrush { Color: var color } || color != Color.Parse("#2468AC"))
                throw new Exception("Packaged compiled URI resources did not reach the grouped host.");
            XmlLayoutSerializer serializer = new(manager);
            serializer.KnownTypes.Register("EditorDocument", static () => new EditorDocument());
            using StringWriter text = new();
            serializer.Serialize(text);
            serializer.Deserialize(new StringReader(text.ToString()));
            Dispatcher.UIThread.RunJobs();
            EditorDocument restored = manager.Layout.Descendents().OfType<EditorDocument>().Single();
            if (restored.FileName != "editor.cs" || !ReferenceEquals(document.Content, restored.Content) || !restored.IsFloating)
                throw new Exception("Packaged custom XML registration lost type, metadata, content, or floating state.");
            foreach (LayoutContent content in manager.Layout.FloatingWindows.Single().Descendents().OfType<LayoutContent>().ToArray()) content.Dock();
            Dispatcher.UIThread.RunJobs();
            if (manager.FloatingWindows.Any(window => window.IsVisible) || restored.IsFloating)
                throw new Exception("Packaged mixed group contents did not return to the main layout.");
        }
        finally
        {
            owner.Content = null;
            Dispatcher.UIThread.RunJobs();
            owner.Close();
        }
    }

    private sealed class ResourceTheme(Uri uri) : Theme { public override Uri GetResourceUri() => uri; }
    private sealed class EditorDocument : LayoutDocument
    {
        public string? FileName { get; set; }
        public override void WriteXml(XmlWriter writer) { writer.WriteAttributeString("FileName", FileName); base.WriteXml(writer); }
        public override void ReadXml(XmlReader reader) { FileName = reader.GetAttribute("FileName"); base.ReadXml(reader); }
    }
}
