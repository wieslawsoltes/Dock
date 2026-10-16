using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Generated;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.PackageTests;

public class PackageConsumerTests
{
    [AvaloniaFact]
    public void Packaged_Xaml_And_Themes_Render_And_Persist_Layout()
    {
        ConsumerWindow window = new();
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            DockingManager manager = window.FindControl<DockingManager>("Manager")!;
            LayoutDocument document = Assert.Single(manager.Layout.Descendents().OfType<LayoutDocument>());
            TextBox editor = Assert.IsType<TextBox>(document.Content);
            Assert.True(editor.Bounds.Width > 0);
            Assert.True(editor.Bounds.Height > 0);
            editor.Text = "Preserved editor content";
            XmlLayoutSerializer serializer = new(manager);
            using StringWriter saved = new();
            serializer.Serialize(saved);
            using StringReader reader = new(saved.ToString());
            serializer.Deserialize(reader);
            Dispatcher.UIThread.RunJobs();
            LayoutDocument restored = Assert.Single(manager.Layout.Descendents().OfType<LayoutDocument>());
            Assert.Equal("editor", restored.ContentId);
            Assert.Same(editor, restored.Content);
            Assert.Equal("Preserved editor content", editor.Text);
            Assert.Single(manager.Layout.Descendents().OfType<LayoutAnchorable>());
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Custom_Layout_Types_Roundtrip_With_Package_Registration()
    {
        DockingManager manager = new()
        {
            Layout = new LayoutRoot
            {
                RootPanel = new LayoutPanel(new LayoutDocumentPane(new PackageDocument
                {
                    ContentId = "custom", Title = "Custom", Content = new TextBlock()
                }))
            }
        };
        XmlLayoutSerializer serializer = new(manager);
#if TEST_GENERATED_CATALOG
        // This interface is added to the application by the packaged analyzer.
        Assert.IsAssignableFrom<IAvalonDockGeneratedCatalogProvider>(Application.Current);
#else
        Assert.False(Application.Current is IAvalonDockGeneratedCatalogProvider);
        serializer.KnownTypes.Register("PackageDocument", static () => new PackageDocument());
#endif
        using StringWriter saved = new();
        serializer.Serialize(saved);
        Assert.Contains("<PackageDocument", saved.ToString());
        using StringReader reader = new(saved.ToString());
        serializer.Deserialize(reader);
        PackageDocument restored = Assert.IsType<PackageDocument>(
            Assert.Single(manager.Layout.Descendents().OfType<LayoutDocument>()));
        Assert.Equal("custom", restored.ContentId);
    }

#if !TEST_GENERATED_CATALOG
    [AvaloniaFact]
    public void Default_Package_Settings_Support_Ordinary_Resource_Dictionaries()
    {
        ResourceInclude resources = new(new Uri("avares://Dock.Avalonia.AvalonDock.PackageTests/"))
        {
            Source = new Uri("avares://Dock.Avalonia.AvalonDock.PackageTests/ConsumerResources.axaml")
        };
        ISolidColorBrush brush = Assert.IsAssignableFrom<ISolidColorBrush>(resources.Loaded["PackageAccent"]);
        Assert.Equal(Color.Parse("#123456"), brush.Color);
    }
#endif
}

public class PackageDocument : LayoutDocument;
