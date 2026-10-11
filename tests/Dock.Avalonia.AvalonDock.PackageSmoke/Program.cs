using System;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
using System.Reactive.Concurrency;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using ReactiveUI.Reactive;

AppBuilder.Configure<PackageConsumer.ConsumerApp>().UseHeadless(new AvaloniaHeadlessPlatformOptions()).SetupWithoutStarting();
PackageConsumer.ConsumerWindow window = new();
if (window.Resources["Container"] is not LayoutDocumentItem { Description: "Unprefixed packaged container" })
    throw new Exception("The packaged item container did not compile in the default XML namespace.");
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
Dispatcher.UIThread.RunJobs();
if (!window.GetVisualDescendants().OfType<TextBlock>().Any(text =>
        text.Classes.Contains("package-tool-header") && ReferenceEquals(text.DataContext, tool) && text.Text == "Tool: Agenda"))
    throw new Exception("The compiled auto-hide header did not receive the public anchorable model.");
ToolPinItemControl pin = manager.GetVisualDescendants().OfType<ToolPinItemControl>()
    .Single(control => ReferenceEquals(control.DataContext, tool.DockModel));
ClickNative(pin, MouseButton.Right);
ContextMenu toolMenu = manager.AnchorableContextMenu!;
if (!toolMenu.IsOpen || !ReferenceEquals(toolMenu.DataContext, manager.GetLayoutItemFromModel(tool)))
    throw new Exception("The packaged anchorable menu did not open for the auto-hide item.");
ClickNative(toolMenu.Items.OfType<MenuItem>().Single());
if (tool.IsAutoHidden || toolMenu.DataContext is not null || toolMenu.PlacementTarget is not null)
    throw new Exception("The custom tool menu did not restore the tool and clear its invocation.");
tool.IsActive = true;
Dispatcher.UIThread.RunJobs();
AssertToolTitle(manager, tool);
tool.Float();
Dispatcher.UIThread.RunJobs();
HostWindow toolHost = (HostWindow)tool.FindParent<LayoutFloatingWindow>()!.DockWindow.Host!;
AssertToolTitle(toolHost, tool);
tool.Title = "Floating agenda";
AssertToolTitle(toolHost, tool);
manager.AnchorableTitleTemplateSelector = new DataTemplateSelector();
Dispatcher.UIThread.RunJobs();
if (manager.AnchorableTitleTemplate is not null || toolHost.GetVisualDescendants().OfType<TextBlock>()
    .Any(text => text.Classes.Contains("package-tool-title")))
    throw new Exception("The tool title selector did not take precedence over its explicit template.");
manager.AnchorableTitleTemplateSelector = null;
manager.AnchorableTitleTemplate = (IDataTemplate)window.Resources["ToolTitle"]!;
Dispatcher.UIThread.RunJobs();
AssertToolTitle(toolHost, tool);
tool.Dock();
tool.Title = "Agenda";
Dispatcher.UIThread.RunJobs();
AssertToolTitle(manager, tool);
tool.ToggleAutoHide();
ObservableCollection<string> documents = new() { "Source document content" };
manager.DocumentsSource = documents;
LayoutDocument fromSource = manager.Layout.Descendents().OfType<LayoutDocument>().Single(x => Equals(x.Content, documents[0]));
fromSource.ContentId = "source-document";
fromSource.IsActive = true;
LayoutDocumentItem sourceItem = (LayoutDocumentItem)manager.GetLayoutItemFromModel(fromSource)!;
if (sourceItem.Title != "Styled: " + documents[0] || fromSource.Title != sourceItem.Title ||
    !ReferenceEquals(sourceItem.Model, fromSource.Content))
    throw new Exception("The packaged compiled container style did not update the live source model.");
manager.LayoutItemContainerStyle = (ControlTheme)window.Resources["ExplicitContainerStyle"]!;
if (sourceItem.Title != "Explicit package container" || fromSource.Title != sourceItem.Title ||
    sourceItem.CanFloat || fromSource.CanFloat || sourceItem.FloatCommand?.CanExecute(null) != false)
    throw new Exception("The explicit container style did not take precedence over its selector.");
manager.LayoutItemContainerStyle = null;
if (sourceItem.Title != "Styled: " + documents[0] || !fromSource.CanFloat)
    throw new Exception("Clearing the explicit container style did not restore the selected style.");
Dispatcher.UIThread.RunJobs();
if (!window.GetVisualDescendants().OfType<TextBlock>().Any(x => x.Text == documents[0] && x.Bounds.Width > 0))
    throw new Exception("Source content was not rendered.");
if (sourceItem.FloatCommand?.CanExecute(null) != true) throw new Exception("The source item cannot float.");
sourceItem.FloatCommand.Execute(null);
Dispatcher.UIThread.RunJobs();
if (!ReferenceEquals(sourceItem, manager.GetLayoutItemFromModel(fromSource)) || sourceItem.FloatCommand.CanExecute(null))
    throw new Exception("Floating did not preserve the item container and command state.");
AssertFloatingSourceTemplate(manager, documents[0]);
HostWindow sourceHost = (HostWindow)manager.Layout.FloatingWindows.Single().DockWindow.Host!;
// The compiled title is wider than the native 300-pixel default host. Keep the
// close target inside the viewport before exercising actual pointer input.
sourceHost.Width = 640;
sourceHost.UpdateLayout();
System.Windows.Input.ICommand defaultClose = sourceItem.CloseCommand!;
int closeRequests = 0;
using var applicationClose = ReactiveCommand.Create(() => closeRequests++, outputScheduler: ImmediateScheduler.Instance);
sourceItem.CloseCommand = applicationClose;
Button nativeClose = sourceHost.GetVisualDescendants().OfType<Button>().Single(button =>
    button.IsEffectivelyVisible && button.Classes.Contains("avalondock-close") && ReferenceEquals(button.DataContext, sourceItem));
sourceHost.UpdateLayout();
Point closePoint = nativeClose.TranslatePoint(new Point(nativeClose.Bounds.Width / 2, nativeClose.Bounds.Height / 2), sourceHost)!.Value;
sourceHost.MouseMove(closePoint);
IInputElement? closeHit = sourceHost.InputHitTest(closePoint);
if (!ReferenceEquals(closeHit, nativeClose) &&
    (closeHit is not Visual closeVisual || !closeVisual.GetVisualAncestors().Contains(nativeClose)))
    throw new Exception($"The floating close button at {closePoint} is not hit-testable: {closeHit?.GetType().Name}, window {sourceHost.Bounds}.");
sourceHost.MouseDown(closePoint, MouseButton.Left);
sourceHost.MouseUp(closePoint, MouseButton.Left);
Dispatcher.UIThread.RunJobs();
if (closeRequests != 1 || !ReferenceEquals(fromSource.Root, manager.Layout))
    throw new Exception("The native floating tab did not execute the application container command.");
DocumentTabStripItem sourceTab = sourceHost.GetVisualDescendants().OfType<DocumentTabStripItem>()
    .Single(tab => ReferenceEquals(tab.DataContext, fromSource.DockModel));
ClickNative(sourceTab, MouseButton.Right);
ContextMenu documentMenu = manager.DocumentContextMenu!;
MenuItem menuClose = documentMenu.Items.OfType<MenuItem>().Single();
if (!documentMenu.IsOpen || !ReferenceEquals(documentMenu.DataContext, sourceItem) || !ReferenceEquals(menuClose.Command, applicationClose))
    throw new Exception("The packaged document menu did not bind the floating item's current command.");
ClickNative(menuClose);
if (closeRequests != 2 || documentMenu.IsOpen || documentMenu.DataContext is not null || documentMenu.PlacementTarget is not null)
    throw new Exception("The custom document menu did not execute the application command and release its invocation.");
sourceItem.CloseCommand = defaultClose;
if (!sourceHost.GetVisualDescendants().OfType<TextBlock>().Any(text =>
        text.Classes.Contains("package-document-header") && ReferenceEquals(text.DataContext, fromSource) && text.Text == "Tab: " + fromSource.Title))
    throw new Exception("The compiled floating tab header did not receive the public document model.");
manager.DocumentHeaderTemplateSelector = new DataTemplateSelector();
Dispatcher.UIThread.RunJobs();
if (manager.DocumentHeaderTemplate is not null || sourceHost.GetVisualDescendants().OfType<TextBlock>()
    .Any(text => text.Classes.Contains("package-document-header")))
    throw new Exception("The header selector did not take precedence over the explicit header template.");
manager.DocumentHeaderTemplateSelector = null;
manager.DocumentHeaderTemplate = (IDataTemplate)window.Resources["DocumentHeader"]!;
manager.LayoutItemTemplate = (IDataTemplate)window.Resources["ExplicitTemplate"]!;
Dispatcher.UIThread.RunJobs();
if (!sourceHost.GetVisualDescendants().Contains(sourceItem.View) ||
    sourceItem.View.Child is not TextBlock { Text: "Explicit package template" })
    throw new Exception("The manager template did not update the native floating item presenter.");
manager.LayoutItemTemplate = null;
manager.LayoutItemTemplateSelector = new DataTemplateSelector();
Dispatcher.UIThread.RunJobs();
AssertFloatingSourceTemplate(manager, documents[0]);
EventHandler<CancelEventArgs> cancelClose = (_, args) => args.Cancel = true;
fromSource.Closing += cancelClose;
sourceHost.Close();
if (!sourceHost.IsVisible || !ReferenceEquals(fromSource.Root, manager.Layout))
    throw new Exception("The native host ignored content closing cancellation.");
fromSource.Closing -= cancelClose;
sourceItem.DockAsDocumentCommand!.Execute(null);
if (fromSource.IsFloating || manager.Layout.FloatingWindows.Count != 0)
    throw new Exception("The source document did not return to its original pane.");
fromSource.Float();
XmlLayoutSerializer sourceSerializer = new(manager);
using StringWriter sourceWriter = new();
sourceSerializer.Serialize(sourceWriter);
sourceSerializer.Deserialize(new StringReader(sourceWriter.ToString()));
Dispatcher.UIThread.RunJobs();
if (sourceItem.LayoutElement is not null || sourceItem.Model is not null)
    throw new Exception("XML restoration did not detach the old item container.");
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
LayoutAnchorablePane batchPane = new(new LayoutAnchorable { ContentId = "first-tool", Content = "First tool" });
batchPane.Children.Add(new LayoutAnchorable { ContentId = "second-tool", Content = "Second tool" });
DockingManager batchManager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(batchPane) } };
RoundTrip(batchManager);
LayoutAnchorable[] batchTools = batchManager.Layout.Descendents().OfType<LayoutAnchorable>().ToArray();
batchManager.DockFactory.FloatAllDockables(batchTools[0].DockModel);
RoundTrip(batchManager);
foreach (LayoutAnchorable item in batchManager.Layout.Descendents().OfType<LayoutAnchorable>().ToArray()) item.Dock();
RoundTrip(batchManager);
LayoutFloatingWindow batchWindow = batchManager.Layout.FloatingWindows.Single();
batchTools = batchManager.Layout.Descendents().OfType<LayoutAnchorable>().ToArray();
if (batchTools.Any(item => item.IsFloating)) throw new Exception("Docked tools retained a floating state after XML reload.");
foreach (LayoutAnchorable item in batchTools.Reverse()) item.Float();
if (!ReferenceEquals(batchWindow, batchManager.Layout.FloatingWindows.Single()) ||
    !batchWindow.Descendents().OfType<LayoutAnchorable>().Select(item => item.ContentId).SequenceEqual(new[] { "first-tool", "second-tool" }))
    throw new Exception("Batch floating did not preserve tab order and return locations across XML saves.");
LayoutDocument metadata = new() { ContentId = "metadata", Title = "XML contract", CanFloat = false };
using StringWriter metadataWriter = new();
using (XmlWriter xml = XmlWriter.Create(metadataWriter))
{
    xml.WriteStartElement("LayoutDocument");
    metadata.WriteXml(xml);
    xml.WriteEndElement();
}
LayoutDocument restoredMetadata = new() { Content = "Existing content" };
using (XmlReader xml = XmlReader.Create(new StringReader(metadataWriter.ToString()))) restoredMetadata.ReadXml(xml);
if (restoredMetadata.ContentId != "metadata" || restoredMetadata.Title != "XML contract" ||
    restoredMetadata.CanFloat || !Equals(restoredMetadata.Content, "Existing content") || restoredMetadata.GetSchema() is not null)
    throw new Exception("The model XML contract did not preserve metadata and existing content.");
Console.WriteLine("NuGet consumer: default XML namespace, compiled XAML, Dock rendering, item containers and commands, native tab command overrides, custom manager context menus, container styles/selectors, manager templates/selectors, tab/auto-hide headers and selector precedence, docked/floating tool titles and selectors, auto-hide and floating XML, model XML contracts, observable sources, floating templates, pane return locations, docking, and close cancellation passed.");

static void AssertToolTitle(Control scope, LayoutAnchorable tool)
{
    if (!scope.GetVisualDescendants().OfType<TextBlock>().Any(text => text.IsEffectivelyVisible &&
        text.Classes.Contains("package-tool-title") && ReferenceEquals(text.DataContext, tool) && text.Text == "Title: " + tool.Title))
        throw new Exception("The packaged tool title template did not render the selected public model and its current title.");
}

static void ClickNative(Control control, MouseButton button = MouseButton.Left)
{
    TopLevel topLevel = TopLevel.GetTopLevel(control)!;
    topLevel.UpdateLayout();
    Point point = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), topLevel)!.Value;
    topLevel.MouseMove(point);
    IInputElement? hit = topLevel.InputHitTest(point);
    if (!ReferenceEquals(hit, control) && (hit is not Visual visual || !visual.GetVisualAncestors().Contains(control)))
        throw new Exception($"The {control.GetType().Name} menu target at {point} is not hit-testable.");
    topLevel.MouseDown(point, button);
    topLevel.MouseUp(point, button);
    Dispatcher.UIThread.RunJobs();
    topLevel.UpdateLayout();
}

static void RoundTrip(DockingManager manager)
{
    XmlLayoutSerializer serializer = new(manager);
    using StringWriter writer = new();
    serializer.Serialize(writer);
    serializer.Deserialize(new StringReader(writer.ToString()));
}

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
    public sealed class SourceStyleSelector : StyleSelector
    {
        public ControlTheme? SourceStyle { get; set; }
        public override ControlTheme? SelectStyle(object? item, AvaloniaObject container)
            => item is string && container is LayoutDocumentItem ? SourceStyle : null;
    }

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
