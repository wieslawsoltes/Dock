using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Generated;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using AvalonDock.Themes;
using Dock.Avalonia.AvalonDock.CatalogFixtures;
using Xunit;

namespace Dock.Avalonia.AvalonDock.CatalogTests;

public class ApplicationDocument : LayoutDocument { }

public class GeneratedCatalogTests
{
    [AvaloniaFact]
    public void Catalog_Belongs_To_Each_Application_Instance()
    {
        IAvalonDockGeneratedCatalogProvider first = new CatalogApplication();
        IAvalonDockGeneratedCatalogProvider second = new CatalogApplication();
        Assert.Same(first.AvalonDockCatalog, first.AvalonDockCatalog);
        Assert.NotSame(first.AvalonDockCatalog, second.AvalonDockCatalog);
    }

    [AvaloniaFact]
    public void Unregistered_Application_And_Library_Layouts_Roundtrip_With_Virtual_Hooks()
    {
        DockingManager manager = new();
        LibraryDocument library = new() { ContentId = "library", Category = "custom", Content = "library content" };
        ApplicationDocument local = new() { ContentId = "local", Content = "local content" };
        LibraryTool tool = new() { ContentId = "tool", Content = "tool content" };
        LibraryDocumentGroup group = new() { Children = { new LayoutDocumentPane { Children = { library, local } } } };
        manager.Layout.RootPanel = new LayoutPanel { Children = { group, new LayoutAnchorablePane(tool) } };
        XmlLayoutSerializer serializer = new(manager);
        StringWriter output = new();
        serializer.Serialize(output);
        string xml = output.ToString();
        Assert.Contains("LibraryDocumentGroup", xml);
        Assert.Contains("LibraryDocument", xml);
        Assert.Contains("ApplicationDocument", xml);
        Assert.Contains("LibraryTool", xml);
        serializer.Deserialize(new StringReader(xml));
        Assert.Single(manager.Layout.Descendents().OfType<LibraryDocumentGroup>());
        Assert.Equal("custom", Assert.Single(manager.Layout.Descendents().OfType<LibraryDocument>()).Category);
        Assert.Single(manager.Layout.Descendents().OfType<ApplicationDocument>());
        Assert.Single(manager.Layout.Descendents().OfType<LibraryTool>());
    }

    [AvaloniaFact]
    public void Explicit_Layout_Factory_Overrides_Generated_Factory_And_Name()
    {
        DockingManager manager = new();
        manager.Layout.RootPanel = new LayoutPanel(new LayoutDocumentPane(new LibraryDocument { ContentId = "doc", Category = "saved" }));
        XmlLayoutSerializer serializer = new(manager);
        LibraryDocument? restored = null;
        serializer.KnownTypes.Register("CustomName", () => restored = new LibraryDocument());
        StringWriter output = new();
        serializer.Serialize(output);
        Assert.Contains("CustomName", output.ToString());
        Assert.DoesNotContain("<LibraryDocument ", output.ToString());
        serializer.Deserialize(new StringReader(output.ToString()));
        Assert.Same(restored, Assert.Single(manager.Layout.Descendents().OfType<LibraryDocument>()));
        Assert.Equal("saved", restored!.Category);
    }

    [AvaloniaTheory]
    [InlineData("Dock.Avalonia.AvalonDock.CatalogTests/ApplicationTheme.axaml", "application")]
    [InlineData("Dock.Avalonia.AvalonDock.CatalogFixtures/LibraryTheme.axaml", "library")]
    public void Unregistered_Compiled_Theme_Reaches_Manager_Floating_Host_And_Navigator(string path, string marker)
    {
        DockingManager manager = new();
        LayoutDocument document = new() { ContentId = "document", Content = "editor" };
        manager.Layout.RootPanel = new LayoutPanel(new LayoutDocumentPane(document));
        Window window = new() { Width = 640, Height = 480, Content = manager };
        window.Show();
        NavigatorWindow? navigator = null;
        try
        {
            manager.Theme = new UriTheme(new Uri("avares://" + path));
            document.Float();
            Dispatcher.UIThread.RunJobs();
            navigator = manager.CreateNavigatorWindow();
            Assert.Equal(marker, manager.FindResource("CatalogMarker"));
            Assert.Equal(marker, Assert.Single(manager.FloatingWindows).FindResource("CatalogMarker"));
            Assert.Equal(marker, navigator.FindResource("CatalogMarker"));
        }
        finally
        {
            navigator?.Close();
            window.Content = null;
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Explicit_Theme_Factory_Overrides_Generated_Resource()
    {
        DockingManager manager = new();
        Uri uri = new("avares://Dock.Avalonia.AvalonDock.CatalogFixtures/LibraryTheme.axaml");
        manager.ThemeResources.Register(uri, static () => new ResourceDictionary { ["CatalogMarker"] = "explicit" });
        manager.Theme = new UriTheme(uri);
        Assert.Equal("explicit", manager.FindResource("CatalogMarker"));
    }

    private sealed class UriTheme(Uri uri) : Theme { public override Uri GetResourceUri() => uri; }
}
