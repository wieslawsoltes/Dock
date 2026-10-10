using System;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

AppBuilder.Configure<PackageConsumer.ConsumerApp>().UseHeadless(new AvaloniaHeadlessPlatformOptions()).SetupWithoutStarting();
PackageConsumer.ConsumerWindow window = new();
window.Show();
Dispatcher.UIThread.RunJobs();
DockingManager manager = window.FindControl<DockingManager>("Manager")!;
XmlLayoutSerializer serializer = new(manager);
using StringWriter writer = new();
serializer.Serialize(writer);
serializer.Deserialize(new StringReader(writer.ToString()));
LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
if (((TextBox)document.Content!).Text != "Package consumer") throw new Exception("Content was not preserved.");
LayoutAnchorable tool = manager.Layout.LeftSide.Children.Single().Children.Single();
if (!tool.IsAutoHidden || tool.ContentId != "agenda") throw new Exception("Auto-hide content was not preserved.");
ObservableCollection<string> documents = new() { "Source document content" };
manager.DocumentsSource = documents;
LayoutDocument fromSource = manager.Layout.Descendents().OfType<LayoutDocument>().Single(x => Equals(x.Content, documents[0]));
fromSource.ContentId = "source-document";
fromSource.IsActive = true;
Dispatcher.UIThread.RunJobs();
if (!window.GetVisualDescendants().OfType<TextBlock>().Any(x => x.Text == documents[0] && x.Bounds.Width > 0))
    throw new Exception("Source content was not rendered.");
fromSource.Float();
Dispatcher.UIThread.RunJobs();
AssertFloatingSourceTemplate(manager, documents[0]);
HostWindow sourceHost = (HostWindow)manager.Layout.FloatingWindows.Single().DockWindow.Host!;
EventHandler<CancelEventArgs> cancelClose = (_, args) => args.Cancel = true;
fromSource.Closing += cancelClose;
sourceHost.Close();
if (!sourceHost.IsVisible || !ReferenceEquals(fromSource.Root, manager.Layout))
    throw new Exception("The native host ignored content closing cancellation.");
fromSource.Closing -= cancelClose;
fromSource.Dock();
if (fromSource.IsFloating || manager.Layout.FloatingWindows.Count != 0)
    throw new Exception("The source document did not return to its original pane.");
fromSource.Float();
XmlLayoutSerializer sourceSerializer = new(manager);
using StringWriter sourceWriter = new();
sourceSerializer.Serialize(sourceWriter);
sourceSerializer.Deserialize(new StringReader(sourceWriter.ToString()));
Dispatcher.UIThread.RunJobs();
AssertFloatingSourceTemplate(manager, documents[0]);
documents.RemoveAt(0);
if (manager.Layout.Descendents().OfType<LayoutDocument>().Count() != 1) throw new Exception("Source changes were not observed.");
if (manager.Layout.FloatingWindows.Count != 0) throw new Exception("The removed source left an empty floating window.");
window.Close();

DockingManager floatingManager = new();
LayoutDocument floatingDocument = new()
{
    ContentId = "floating", Content = "Floating package content", FloatingWidth = 520,
    FloatingHeight = 380, FloatingLeft = 50, FloatingTop = 70, IsMaximized = true
};
floatingManager.Layout.FloatingWindows.Add(new LayoutDocumentFloatingWindow { RootDocument = floatingDocument });
XmlLayoutSerializer floatingSerializer = new(floatingManager);
using StringWriter floatingWriter = new();
floatingSerializer.Serialize(floatingWriter);
floatingSerializer.Deserialize(new StringReader(floatingWriter.ToString()));
LayoutDocumentFloatingWindow restoredWindow = (LayoutDocumentFloatingWindow)floatingManager.Layout.FloatingWindows.Single();
if (!Equals(restoredWindow.RootDocument!.Content, "Floating package content") || restoredWindow.DockWindow.Width != 520 ||
    restoredWindow.DockWindow.Height != 380 || restoredWindow.DockWindow.X != 50 || restoredWindow.DockWindow.Y != 70 ||
    !restoredWindow.RootDocument.IsMaximized)
    throw new Exception("Floating layout content or window state was not preserved.");
Console.WriteLine("NuGet consumer: default XML namespace, compiled XAML, Dock rendering, auto-hide and floating XML, observable sources, floating templates, docking, and close cancellation passed.");

static void AssertFloatingSourceTemplate(DockingManager manager, string content)
{
    LayoutFloatingWindow floatingWindow = manager.Layout.FloatingWindows.Single();
    if (floatingWindow.DockWindow.Host is not HostWindow host || !host.IsVisible ||
        !host.GetVisualDescendants().OfType<TextBlock>().Any(x =>
            Equals(x.DataContext, content) && x.Text == content && x.Classes.Contains("source-template") && x.Bounds.Width > 0))
        throw new Exception("The floating host did not render the manager's compiled content template.");
}

namespace PackageConsumer
{
    public class ConsumerApp : Application
    {
        public override void Initialize()
        {
            Styles.Add(new FluentTheme());
        }
    }
    public partial class ConsumerWindow : Window
    {
        public ConsumerWindow() => InitializeComponent();
    }
}
