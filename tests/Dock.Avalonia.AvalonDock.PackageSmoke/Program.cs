using System;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Xml;
using System.Reactive.Concurrency;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Controls.ProportionalStackPanel;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Converters;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using AvalonDock.Themes;
using Dock.Controls.DeferredContentControl;
using ReactiveUI.Reactive;

ClassicDesktopStyleApplicationLifetime lifetime = new() { ShutdownMode = ShutdownMode.OnExplicitShutdown };
AppBuilder.Configure<PackageConsumer.ConsumerApp>().UseHeadless(new AvaloniaHeadlessPlatformOptions()).SetupWithLifetime(lifetime);
AssertLayoutLifecycle();
AssertLayoutParentPolicies();
AssertPaneSelectionPolicies();
AssertContentHooks();
PackageConsumer.ContentControlProbe.Verify();
PackageConsumer.TitleControlProbe.Verify();
PackageConsumer.TabControlProbe.Verify();
PackageConsumer.TabPanelProbe.Verify();
PackageConsumer.PaneControlProbe.Verify();
PackageConsumer.GridControlProbe.Verify();
PackageConsumer.AnchorControlProbe.Verify();
PackageConsumer.MenuControlProbe.Verify();
PackageConsumer.MetadataAndUtilityProbe.Verify();
PackageConsumer.OverlayProbe.Verify();
PackageConsumer.LifecycleProbe.Verify();
PackageConsumer.DestinationProbe.Verify();
PackageConsumer.SerializationProbe.Verify();
PackageConsumer.OperationProbe.Verify();
PackageConsumer.AutoHideReturnProbe.Verify();
PackageConsumer.NativeWindowProbe.Verify();
PackageConsumer.SourceCollectionProbe.Verify();
PackageConsumer.UpdateStrategyProbe.Verify();
PackageConsumer.HttpImageProbe.Verify();
PackageConsumer.CloseOriginProbe closeOrigin = new(new LayoutDocumentFloatingWindow());
if (!closeOrigin.UserOrigin) throw new Exception("A new floating window did not expose user close origin.");
((Dock.Model.Core.IHostWindow)closeOrigin).Exit();
if (closeOrigin.UserOrigin) throw new Exception("Native host exit did not expose internal close origin.");
PackageConsumer.ConsumerWindow window = new();
if (window.Resources["SystemMenuOption"] is not DockingManager { ShowSystemMenu: false } ||
    window.Resources["LegacySystemMenuOption"] is not DockingManager { ShowSystemMenu: false })
    throw new Exception("The packaged system-menu property did not compile in both XML namespaces.");
if (window.Resources["Container"] is not LayoutDocumentItem { Description: "Unprefixed packaged container" })
    throw new Exception("The packaged item container did not compile in the default XML namespace.");
window.Show();
Dispatcher.UIThread.RunJobs();
DockingManager manager = window.FindControl<DockingManager>("Manager")!;
if (manager.Theme is not GenericTheme ||
    !manager.Resources.TryGetResource(typeof(NavigatorWindow), null, out object? navigatorTheme) || navigatorTheme is not ControlTheme)
    throw new Exception("The unprefixed GenericTheme did not expose the packaged compiled control resources.");
if (window.Resources["GenericByUri"] is not ResourceDictionary genericResources ||
    !genericResources.TryGetResource(typeof(NavigatorWindow), null, out _))
    throw new Exception("A consumer ResourceInclude could not resolve the packaged generic theme URI.");
if (!manager.AllowMovingFloatingWindowWithKeyboard)
    throw new Exception("The unprefixed keyboard-movement property did not compile from XAML.");
XmlLayoutSerializer serializer = new(manager);
using StringWriter writer = new();
serializer.Serialize(writer);
serializer.Deserialize(new StringReader(writer.ToString()));
LayoutDocument document = manager.Layout.Descendents().OfType<LayoutDocument>().Single();
if (((TextBox)document.Content!).Text != "Package consumer") throw new Exception("Content was not preserved.");
LayoutAnchorable tool = manager.Layout.LeftSide.Children.Single().Children.Single();
if (!tool.IsAutoHidden || tool.ContentId != "agenda") throw new Exception("Auto-hide content was not preserved.");
Dispatcher.UIThread.RunJobs();
AssertContentControls(window, manager, document, tool);
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
AssertNativeContentControl(manager, tool);
AssertDocumentMenu(manager);
AssertConverters(window, manager, tool);
AssertNavigator(window, manager, document);
AssertSplitters(manager);
manager.GridSplitterWidth = 28;
manager.GridSplitterHeight = 18;
Dispatcher.UIThread.RunJobs();
window.UpdateLayout();
AssertSplitters(manager);
tool.Float();
Dispatcher.UIThread.RunJobs();
HostWindow toolHost = (HostWindow)tool.FindParent<LayoutFloatingWindow>()!.DockWindow.Host!;
AssertNativeContentControl(toolHost, tool);
AssertActiveContent(manager, document, tool);
AssertThemes(window, manager, toolHost);
AssertFloatingResize(window, (LayoutFloatingWindowControl)toolHost);
AssertFloatingKeyboard(toolHost, (Control)tool.Content!);
AssertFloatingWindowCommands(window, manager, tool, (LayoutAnchorableFloatingWindowControl)toolHost);
tool.Hide();
Dispatcher.UIThread.RunJobs();
if (toolHost.IsVisible || !manager.FloatingWindows.Contains((LayoutFloatingWindowControl)toolHost))
    throw new Exception("Hiding a floating tool did not retain its native control registration.");
tool.Show();
Dispatcher.UIThread.RunJobs();
if (!toolHost.IsVisible || !ReferenceEquals(tool.FindParent<LayoutFloatingWindow>()!.DockWindow.Host, toolHost) ||
    manager.DockFactory.HostWindows.Count(value => ReferenceEquals(value, toolHost)) != 1)
    throw new Exception("Restoring a hidden tool did not reuse its host with exactly one native registration.");
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
AssertNativeContentControl(sourceHost, fromSource);
// The compiled title is wider than the native 300-pixel default host. Keep the
// close target inside the viewport before exercising actual pointer input.
sourceHost.Width = 640;
sourceHost.UpdateLayout();
TextBlock documentTitle = sourceHost.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Classes.Contains("package-document-title"));
if (!ReferenceEquals(documentTitle.DataContext, fromSource) || documentTitle.Text != "Window: " + fromSource.Title)
    throw new Exception("The packaged floating title template did not receive the public root document.");
Button maximize = sourceHost.GetVisualDescendants().OfType<Button>().Single(button => button.Classes.Contains("avalondock-document-maximize"));
ClickNative(maximize);
if (!fromSource.IsMaximized || sourceHost.WindowState != WindowState.Maximized)
    throw new Exception("The packaged caption did not maximize the native document window.");
ClickNative(maximize);
if (fromSource.IsMaximized || sourceHost.WindowState != WindowState.Normal)
    throw new Exception("The packaged caption did not restore the native document window.");
IDataTemplate documentTitleTemplate = manager.DocumentTitleTemplate!;
manager.DocumentTitleTemplateSelector = new DataTemplateSelector();
Dispatcher.UIThread.RunJobs();
if (manager.DocumentTitleTemplate is not null || sourceHost.GetVisualDescendants().Contains(documentTitle))
    throw new Exception("The packaged document title selector did not take precedence.");
manager.DocumentTitleTemplateSelector = null;
manager.DocumentTitleTemplate = documentTitleTemplate;
Dispatcher.UIThread.RunJobs();
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
Button captionClose = sourceHost.GetVisualDescendants().OfType<Button>().Single(button => button.Classes.Contains("avalondock-document-window-close"));
ClickNative(captionClose);
if (closeRequests != 3 || !sourceHost.IsVisible)
    throw new Exception("The packaged floating caption did not use the current close command.");
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
AssertDockingNotifications(manager, window);
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
AssertNavigatorLifetime(lifetime);
Console.WriteLine("NuGet consumer: default XML namespace, compiled XAML, Dock rendering, item containers and commands, native tab command overrides, custom manager context menus, document list header/icon templates and activation, container styles/selectors, manager templates/selectors, tab/auto-hide headers and selector precedence, tool and floating document titles/selectors, document caption maximization and close commands, configurable native splitters, auto-hide and floating XML, model XML contracts, observable sources, floating templates, pane return locations, bubbling native docking notifications, typed floating controls and collection notifications, model-to-container and command/value converters, navigator input and desktop lifetime cleanup, and close cancellation passed.");

static void AssertContentControls(Window window, DockingManager manager, LayoutDocument document, LayoutAnchorable tool)
{
    LayoutDocumentControl documentControl = (LayoutDocumentControl)((IDataTemplate)window.Resources["DocumentContentControl"]!).Build(document)!;
    documentControl.DataContext = document;
    LayoutAnchorableControl toolControl = (LayoutAnchorableControl)((IDataTemplate)window.Resources["ToolContentControl"]!).Build(tool)!;
    toolControl.DataContext = tool;
    if (!ReferenceEquals(documentControl.Model, document) || !ReferenceEquals(documentControl.LayoutItem, manager.GetLayoutItemFromModel(document)) ||
        !ReferenceEquals(toolControl.Model, tool) || !ReferenceEquals(toolControl.LayoutItem, manager.GetLayoutItemFromModel(tool)))
        throw new Exception("Compiled packaged content controls did not resolve models and containers through both XML namespaces.");
    documentControl.Model = null;
    toolControl.Model = null;
    AssertNativeContentControl(manager, document);
}

static void AssertNativeContentControl(Control scope, LayoutContent model)
{
    Control control;
    LayoutItem? item;
    if (model is LayoutDocument || model.Parent is LayoutDocumentPane)
    {
        LayoutDocumentControl document = scope.GetVisualDescendants().OfType<LayoutDocumentControl>().Single(value => ReferenceEquals(value.Model, model));
        control = document;
        item = document.LayoutItem;
    }
    else
    {
        LayoutAnchorableControl tool = scope.GetVisualDescendants().OfType<LayoutAnchorableControl>().Single(value => ReferenceEquals(value.Model, model));
        control = tool;
        item = tool.LayoutItem;
    }
    if (item is null || !ReferenceEquals(item, model.Root!.Manager!.GetLayoutItemFromModel(model)) ||
        !control.GetVisualDescendants().Contains(item.View))
        throw new Exception("A packaged native content control did not present its existing LayoutItem.View.");
}

static void AssertNavigatorLifetime(ClassicDesktopStyleApplicationLifetime lifetime)
{
    if (lifetime.Windows.Count != 0) throw new Exception("The preceding package checks left desktop windows open.");
    LayoutDocument first = new() { Title = "Lifetime first", Content = new TextBox() };
    LayoutDocument second = new() { Title = "Lifetime second", Content = new TextBox() };
    PackageConsumer.ConsumerWindow owner = new();
    DockingManager manager = owner.FindControl<DockingManager>("Manager")!;
    manager.Layout = new() { RootPanel = new(new LayoutDocumentPane { Children = { first, second } }) };
    owner.Show();
    Dispatcher.UIThread.RunJobs();
    int closed = 0;
    for (int index = 0; index < 2; index++)
    {
        first.IsActive = true;
        first.LastActivationTimeStamp = DateTime.MaxValue;
        second.LastActivationTimeStamp = DateTime.MinValue;
        owner.MouseMove(default);
        if (!((Control)first.Content!).Focus()) throw new Exception("The lifetime document did not receive focus before Ctrl+Tab.");
        owner.KeyPressQwerty(PhysicalKey.Tab, RawInputModifiers.Control);
        NavigatorWindow navigator = owner.OwnedWindows.OfType<NavigatorWindow>().Single();
        navigator.Closed += (_, _) => closed++;
        navigator.Closing += (_, args) => args.Cancel = true;
        navigator.SelectedDocument = navigator.Documents![0];
        if (navigator.IsVisible || navigator.Owner is not null || !lifetime.Windows.Contains(navigator))
            throw new Exception("Document selection did not keep the hidden navigator in the desktop lifetime until owner cleanup.");
    }
    EventHandler<WindowClosingEventArgs> cancel = (_, args) => args.Cancel = true;
    owner.Closing += cancel;
    owner.Close();
    if (!owner.IsVisible || closed != 0 || lifetime.Windows.Count != 3)
        throw new Exception("Canceling owner closure did not preserve its hidden navigators.");
    owner.Closing -= cancel;
    int exits = 0;
    lifetime.Exit += (_, _) => exits++;
    lifetime.ShutdownMode = ShutdownMode.OnLastWindowClose;
    owner.Close();
    if (closed != 2 || lifetime.Windows.Count != 0 || exits != 1)
        throw new Exception("Hidden navigators prevented the desktop lifetime from shutting down after the last owner closed.");
}

static void AssertThemes(Window owner, DockingManager manager, HostWindow host)
{
    AvalonDock.Themes.Theme? previous = manager.Theme;
    ResourceDictionary dictionary = new() { ["PackagedThemeBrush"] = Brushes.Red };
    NavigatorWindow? navigator = null;
    using IDisposable hostBinding = host.Bind(Window.BackgroundProperty, new DynamicResourceExtension("PackagedThemeBrush"));
    try
    {
        manager.Theme = new PackageConsumer.ConsumerDictionaryTheme(dictionary);
        navigator = manager.CreateNavigatorWindow();
        using IDisposable navigatorBinding = navigator.Bind(Window.BackgroundProperty, new DynamicResourceExtension("PackagedThemeBrush"));
        _ = navigator.ShowDialog(owner);
        navigator.MouseMove(default);
        if (!ReferenceEquals(host.Background, Brushes.Red) || !ReferenceEquals(navigator.Background, Brushes.Red))
            throw new Exception("A shared dictionary theme did not update native floating and navigator resources.");
        dictionary["PackagedThemeBrush"] = Brushes.Blue;
        if (!ReferenceEquals(host.Background, Brushes.Blue) || !ReferenceEquals(navigator.Background, Brushes.Blue))
            throw new Exception("Live dictionary changes did not reach both packaged windows.");
        manager.Theme = new PackageConsumer.ConsumerDictionaryTheme(new() { ["PackagedThemeBrush"] = Brushes.Green });
        if (!ReferenceEquals(host.Background, Brushes.Green) || !ReferenceEquals(navigator.Background, Brushes.Green) || dictionary.Owner is not null)
            throw new Exception("Replacing the theme did not refresh both windows and release the previous dictionary owner.");
    }
    finally
    {
        navigator?.Close();
        manager.Theme = previous;
    }
}

static void AssertLayoutLifecycle()
{
    PackageConsumer.LifecycleDocument document = new();
    LayoutRoot first = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(document)) };
    LayoutDocumentPane destination = new();
    LayoutRoot second = new() { RootPanel = new LayoutPanel(destination) };
    int removed = 0;
    int added = 0;
    first.ElementRemoved += (_, args) =>
    {
        if (!ReferenceEquals(args.Element, document)) throw new Exception("The removed event did not identify the public document.");
        removed++;
    };
    second.ElementAdded += (_, args) =>
    {
        if (!ReferenceEquals(args.Element, document)) throw new Exception("The added event did not identify the public document.");
        added++;
    };
    document.ResetNotifications();
    destination.Children.Add(document);
    if (removed != 1 || added != 1 || document.BeforeChanges != 2 || document.AfterChanges != 2 ||
        document.RootChanges != 2 || !ReferenceEquals(document.Parent, destination) || !ReferenceEquals(document.Root, second))
        throw new Exception("The packaged parent/root hooks and layout membership events did not follow the move.");
}

static void AssertLayoutParentPolicies()
{
    LayoutDocument document = new();
    LayoutDocumentPane destination = new(document);
    LayoutRoot root = new() { RootPanel = new LayoutPanel(destination) };
    LayoutDocument selected = new() { IsSelected = true };
    destination.Children.Add(selected);
    if (!ReferenceEquals(destination.SelectedContent, selected) || !selected.IsSelected || document.IsSelected)
        throw new Exception("Attaching preselected content did not update the native pane's selection.");

    PackageConsumer.VisibilityPane pane = new();
    LayoutAnchorable tool = new();
    pane.Children.Add(tool);
    LayoutPanel panel = new(pane);
    int visibilityChanges = pane.VisibilityChanges;
    pane.SetVisibility(false);
    if (panel.IsVisible) throw new Exception("The protected visibility setter did not update its parent panel.");
    pane.SetVisibility(true);
    if (!panel.IsVisible || pane.VisibilityChanges != visibilityChanges + 2)
        throw new Exception("The external visibility override did not report both transitions.");
    GC.KeepAlive(root);
}

static void AssertPaneSelectionPolicies()
{
    LayoutDocument first = new();
    LayoutDocument selected = new();
    LayoutDocument recent = new();
    PackageConsumer.SelectionPane documents = new();
    documents.Children.Add(first);
    documents.Children.Add(selected);
    documents.Children.Add(recent);
    LayoutAnchorable disabled = new() { IsEnabled = false };
    LayoutAnchorable removed = new();
    LayoutAnchorable enabled = new();
    LayoutAnchorablePane tools = new(disabled);
    if (tools.SelectedContent is not null) throw new Exception("A detached pane selected its disabled content.");
    tools.Children.Add(removed);
    tools.Children.Add(enabled);
    LayoutPanel panel = new(documents);
    panel.Children.Add(tools);
    LayoutRoot root = new() { RootPanel = panel };
    first.IsActive = true;

    documents.SelectedContentIndex = 1;
    if (!ReferenceEquals(documents.SelectedContent, selected) || !selected.IsSelected || first.IsSelected ||
        !ReferenceEquals(root.ActiveContent, first))
        throw new Exception("Public pane selection did not preserve the active content independently of selection.");
    selected.IsSelected = false;
    if (documents.SelectedContentIndex != -1 || !ReferenceEquals(root.ActiveContent, first))
        throw new Exception("Clearing content selection changed activation.");
    selected.IsSelected = true;
    documents.MoveChild(1, 2);
    documents.MoveChild(2, 2);
    if (documents.MoveCount != 1 || documents.LastMove != (1, 2, 2) ||
        !ReferenceEquals(documents.SelectedContent, selected))
        throw new Exception("The external ChildMoved override did not receive the updated selected index.");

    selected.IsActive = true;
    first.LastActivationTimeStamp = new DateTime(2000, 1, 1);
    recent.LastActivationTimeStamp = new DateTime(2001, 1, 1);
    root.DockFactory.RemoveDockable(selected.DockModel, false);
    if (!ReferenceEquals(documents.SelectedContent, recent) || !ReferenceEquals(root.ActiveContent, recent))
        throw new Exception("Native removal did not select and activate the most recent remaining document.");
    removed.IsActive = true;
    documents.Children.Clear();
    if (root.ActiveContent is not null || removed.IsActive)
        throw new Exception("Emptying a rooted document pane did not clear root activation.");
    tools.SelectedContentIndex = 1;
    root.DockFactory.RemoveDockable(removed.DockModel, false);
    if (!ReferenceEquals(tools.SelectedContent, enabled) || !enabled.IsSelected || disabled.IsSelected)
        throw new Exception("Native removal did not choose the first enabled remaining tool.");
}

static void AssertContentHooks()
{
    LayoutDocument first = new();
    PackageConsumer.ContentHookDocument document = new();
    LayoutDocumentPane pane = new(first);
    pane.Children.Add(document);
    LayoutRoot root = new() { RootPanel = new LayoutPanel(pane) };
    DockingManager manager = new() { Layout = root };
    first.IsActive = true;
    int selections = document.SelectionChanges;
    int activations = document.Activations;
    document.IsSelected = true;
    if (document.SelectionChanges != selections + 1 || !ReferenceEquals(root.ActiveContent, first))
        throw new Exception("The external selection override did not run independently of activation.");
    root.DockFactory.SetActiveDockable(document.DockModel);
    if (document.Activations != activations + 1 || !ReferenceEquals(root.ActiveContent, document) || document.LastActivationTimeStamp is null)
        throw new Exception("The native factory did not activate the selected document through its external override.");
    int managerCloses = 0;
    manager.DocumentClosed += (_, _) => managerCloses++;
    document.CancelClose = true;
    document.Close();
    if (document.ClosingRequests != 1 || document.CompletedCloses != 0 || !ReferenceEquals(document.Parent, pane))
        throw new Exception("The external closing override did not cancel removal.");
    document.CancelClose = false;
    manager.GetLayoutItemFromModel(document)!.CloseCommand!.Execute(null);
    if (document.ClosingRequests != 2 || document.CompletedCloses != 1 || document.Parent is not null || managerCloses != 1)
        throw new Exception("The packaged item command did not close through the external hooks and manager event.");
}

static void AssertActiveContent(DockingManager manager, LayoutDocument document, LayoutAnchorable tool)
{
    try
    {
        document.Float();
        Dispatcher.UIThread.RunJobs();
        LayoutFloatingWindow window = document.FindParent<LayoutFloatingWindow>()!;
        tool.IsActive = true;
        manager.ActiveContent = document.Content;
        if (!ReferenceEquals(manager.Layout.ActiveContent, document) ||
            !ReferenceEquals(manager.ActiveContent, document.Content) ||
            !ReferenceEquals(manager.Layout.LastFocusedDocument, document) ||
            !document.IsLastFocusedDocument || !document.IsActive || tool.IsActive ||
            !ReferenceEquals(((Dock.Model.Controls.IRootDock)window.DockModel).FocusedDockable, document.DockModel) ||
            ReferenceEquals(manager.DockLayout.FocusedDockable, document.DockModel))
            throw new Exception("Floating activation did not synchronize public content with the owning native root.");
        manager.ActiveContent = null;
        if (manager.Layout.ActiveContent is not null || manager.Layout.LastFocusedDocument is not null ||
            document.IsLastFocusedDocument || document.IsActive ||
            ((Dock.Model.Controls.IRootDock)window.DockModel).FocusedDockable is not null)
            throw new Exception("Clearing floating activation did not clear its native focus and document history.");
    }
    finally
    {
        document.Dock();
        Dispatcher.UIThread.RunJobs();
        tool.IsActive = true;
    }
}

static void AssertNavigator(Window view, DockingManager manager, LayoutDocument document)
{
    TextBox editor = new() { Text = "Navigator package editor" };
    LayoutDocument next = new() { Title = "Navigator package document", Content = editor };
    ((LayoutDocumentPane)document.Parent!).Children.Add(next);
    DateTime? activation = document.LastActivationTimeStamp;
    NavigatorWindow? navigator = null;
    try
    {
        document.IsActive = true;
        document.LastActivationTimeStamp = DateTime.MaxValue;
        next.LastActivationTimeStamp = DateTime.MinValue;
        DeferredContentScheduling.SetTimeline(manager, new DeferredContentPresentationTimeline
        {
            InitialDelay = TimeSpan.FromHours(1),
            RevealDuration = TimeSpan.Zero
        });
        if (!((Control)document.Content!).Focus()) throw new Exception("The document did not receive focus before Ctrl+Tab.");
        view.KeyPressQwerty(PhysicalKey.Tab, RawInputModifiers.Control);
        navigator = view.OwnedWindows.OfType<NavigatorWindow>().Single();
        navigator.OnApplyTemplate();
        navigator.Styles.Add((Style)view.Resources["NavigatorLabels"]!);
        navigator.MouseMove(default);
        if (!ReferenceEquals(navigator.SelectedDocument?.LayoutElement, next) ||
            !ReferenceEquals(navigator.SelectedDocument, manager.GetLayoutItemFromModel(next)))
            throw new Exception("The packaged navigator did not select the second native-backed document container.");
        if (navigator.LayoutDocumentsLabel != "Packaged documents" || navigator.LayoutAnchorablesLabel != "Packaged tools" ||
            !navigator.GetVisualDescendants().OfType<TextBlock>().Any(text => text.Text == "Packaged documents") ||
            !navigator.GetVisualDescendants().OfType<TextBlock>().Any(text => text.Text == next.Title))
            throw new Exception("The unprefixed navigator style and compiled item template did not render.");
        navigator.KeyPressQwerty(PhysicalKey.ArrowRight, RawInputModifiers.Control);
        navigator.KeyReleaseQwerty(PhysicalKey.ArrowRight, RawInputModifiers.Control);
        if (navigator.SelectedDocument is not null || navigator.SelectedAnchorable is null)
            throw new Exception("The packaged navigator did not cross to its tool list.");
        navigator.KeyPressQwerty(PhysicalKey.ArrowLeft, RawInputModifiers.Control);
        navigator.KeyReleaseQwerty(PhysicalKey.ArrowLeft, RawInputModifiers.Control);
        navigator.KeyPressQwerty(PhysicalKey.Tab, RawInputModifiers.Control);
        navigator.KeyReleaseQwerty(PhysicalKey.Tab, RawInputModifiers.Control);
        if (!ReferenceEquals(navigator.SelectedDocument?.LayoutElement, next))
            throw new Exception("The packaged navigator did not cycle back to the second document.");
        navigator.KeyReleaseQwerty(PhysicalKey.ControlLeft, RawInputModifiers.None);
        if (navigator.IsVisible || !ReferenceEquals(manager.Layout.ActiveContent, next) || TopLevel.GetTopLevel(editor) is not null)
            throw new Exception("The packaged navigator did not activate the document while its scoped timeline deferred presentation.");
        DeferredContentScheduling.SetTimeline(manager, new DeferredContentPresentationTimeline
        {
            FollowUpDelay = TimeSpan.Zero,
            MaxPresentationsPerPass = 1,
            RevealDuration = TimeSpan.Zero
        });
        view.MouseMove(default);
        if (navigator.IsVisible || !ReferenceEquals(manager.Layout.ActiveContent, next) ||
            !ReferenceEquals(view.FocusManager!.GetFocusedElement(), editor))
            throw new Exception("The packaged navigator did not activate and focus the selected native document.");
    }
    finally
    {
        manager.ClearValue(DeferredContentScheduling.TimelineProperty);
        navigator?.Close();
        next.Close();
        document.IsActive = true;
        document.LastActivationTimeStamp = activation;
    }
}

static void AssertConverters(Window resources, DockingManager manager, LayoutAnchorable tool)
{
    Control controls = ((IDataTemplate)resources.Resources["ConvertedCommands"]!).Build(tool)!;
    controls.DataContext = tool;
    Window view = new() { Content = controls, Width = 300, Height = 260 };
    bool canClose = tool.CanClose;
    object? tooltip = tool.ToolTip;
    try
    {
        view.Show();
        Dispatcher.UIThread.RunJobs();
        LayoutAnchorableItem item = (LayoutAnchorableItem)manager.GetLayoutItemFromModel(tool)!;
        Button activate = controls.GetVisualDescendants().OfType<Button>().Single(value => value.Classes.Contains("converted-activate"));
        Button hide = controls.GetVisualDescendants().OfType<Button>().Single(value => value.Classes.Contains("converted-hide"));
        Button autoHide = controls.GetVisualDescendants().OfType<Button>().Single(value => value.Classes.Contains("converted-auto-hide"));
        if (!ReferenceEquals(activate.Command, item.ActivateCommand) || !ReferenceEquals(hide.Command, item.HideCommand) ||
            !ReferenceEquals(autoHide.Command, item.AutoHideCommand))
            throw new Exception("Compiled converter bindings did not resolve the current packaged item commands.");
        ClickNative(activate);
        if (!ReferenceEquals(manager.Layout.ActiveContent, tool)) throw new Exception("Converted activation did not select the native tool.");
        ClickNative(autoHide);
        if (!tool.IsAutoHidden) throw new Exception("Converted auto-hide did not pin the native tool.");
        ClickNative(autoHide);
        if (tool.IsAutoHidden) throw new Exception("Converted auto-hide did not restore the native tool.");
        ClickNative(hide);
        if (!tool.IsHidden) throw new Exception("Converted hiding did not hide the native tool.");
        tool.Show();
        TextBlock visible = controls.GetVisualDescendants().OfType<TextBlock>().Single(value => value.Classes.Contains("converted-visible"));
        TextBlock inverse = controls.GetVisualDescendants().OfType<TextBlock>().Single(value => value.Classes.Contains("converted-inverse"));
        TextBlock hideEntry = controls.GetVisualDescendants().OfType<TextBlock>().Single(value => value.Classes.Contains("converted-hide-visible"));
        tool.CanClose = true;
        if (!visible.IsVisible || inverse.IsVisible || hideEntry.IsVisible)
            throw new Exception("Packaged converters did not adapt visibility to Boolean values.");
        tool.CanClose = false;
        if (visible.IsVisible || !inverse.IsVisible || !hideEntry.IsVisible)
            throw new Exception("Packaged visibility converters did not react to capability changes.");
        tool.ToolTip = "Retained converter text";
        tool.ToolTip = null;
        if (visible.Text != "Retained converter text") throw new Exception("The packaged null converter did not preserve the target text.");
        AnchorSideToAngleConverter angle = (AnchorSideToAngleConverter)resources.Resources["SideAngle"]!;
        AnchorSideToOrientationConverter orientation = (AnchorSideToOrientationConverter)resources.Resources["SideOrientation"]!;
        if (!Equals(angle.Convert(AnchorSide.Left, typeof(double), null, CultureInfo.InvariantCulture), 90d) ||
            !Equals(orientation.Convert(AnchorSide.Left, typeof(Avalonia.Layout.Orientation), null, CultureInfo.InvariantCulture), Avalonia.Layout.Orientation.Vertical))
            throw new Exception("Unprefixed packaged side converters did not resolve and convert.");
    }
    finally
    {
        tool.CanClose = canClose;
        tool.ToolTip = tooltip;
        view.Close();
    }
}

static void AssertFloatingResize(Window view, LayoutFloatingWindowControl host)
{
    Style style = (Style)view.Resources["FloatingResizeStyle"]!;
    host.Styles.Add(style);
    try
    {
        Dispatcher.UIThread.RunJobs();
        host.UpdateLayout();
        if (host.ResizeBorderThickness != new Thickness(4, 6, 8, 12))
            throw new Exception("The unprefixed resize-border style did not configure the native host.");
        Point edge = new(1, host.Bounds.Height / 2);
        host.MouseMove(edge);
        if (host.InputHitTest(edge) is not Visual grip ||
            WindowDecorationProperties.GetElementRole(grip) != WindowDecorationsElementRole.ResizeW)
            throw new Exception("The configured floating border did not expose a native resize role.");
        bool handled = false;
        EventHandler<PointerPressedEventArgs> observe = (_, args) => handled = args.Handled;
        host.AddHandler(InputElement.PointerPressedEvent, observe, RoutingStrategies.Bubble, handledEventsToo: true);
        try
        {
            host.MouseDown(edge, MouseButton.Left);
            host.MouseUp(edge, MouseButton.Left);
            if (!handled) throw new Exception("The resize grip did not handle native resize input.");
        }
        finally { host.RemoveHandler(InputElement.PointerPressedEvent, observe); }
        host.CanResize = false;
        host.MouseMove(edge);
        if (host.InputHitTest(edge) is Visual disabled &&
            WindowDecorationProperties.GetElementRole(disabled) == WindowDecorationsElementRole.ResizeW)
            throw new Exception("A fixed-size floating host still exposed resize input.");
    }
    finally
    {
        host.CanResize = true;
        host.Styles.Remove(style);
    }
}

static void AssertFloatingKeyboard(HostWindow host, Control target)
{
    PixelPoint before = host.Position;
    bool wasFocusable = target.Focusable;
    try
    {
        target.Focusable = true;
        if (!target.Focus()) throw new Exception("The floating keyboard target could not receive focus.");
        host.KeyPressQwerty(PhysicalKey.ArrowRight, RawInputModifiers.None);
        host.KeyReleaseQwerty(PhysicalKey.ArrowRight, RawInputModifiers.None);
        int step = (int)Math.Round(25 * host.DesktopScaling);
        if (host.Position != new PixelPoint(before.X + step, before.Y))
            throw new Exception("The packaged floating host did not respond to an unhandled arrow key.");
    }
    finally
    {
        target.Focusable = wasFocusable;
        host.Position = before;
    }
}

static void AssertFloatingWindowCommands(Window resources, DockingManager manager, LayoutAnchorable tool, LayoutAnchorableFloatingWindowControl host)
{
    LayoutAnchorableItem item = (LayoutAnchorableItem)manager.GetLayoutItemFromModel(tool)!;
    System.Windows.Input.ICommand? previousClose = item.CloseCommand;
    System.Windows.Input.ICommand? previousHide = item.HideCommand;
    bool previousCanClose = tool.CanClose;
    Control controls = ((IDataTemplate)resources.Resources["FloatingWindowCommands"]!).Build(host)!;
    controls.DataContext = host;
    Window view = new() { Content = controls, Width = 300, Height = 150 };
    int closed = 0;
    int hidden = 0;
    using var close = ReactiveCommand.Create<object?>(value =>
    {
        if (!ReferenceEquals(value, host.Model)) throw new Exception("The window close command lost its parameter.");
        closed++;
    }, outputScheduler: ImmediateScheduler.Instance);
    using var hide = ReactiveCommand.Create<object?>(value =>
    {
        if (!ReferenceEquals(value, host.Model)) throw new Exception("The window hide command lost its parameter.");
        hidden++;
    }, outputScheduler: ImmediateScheduler.Instance);
    try
    {
        tool.CanClose = true;
        item.CloseCommand = close;
        item.HideCommand = hide;
        view.Show();
        Dispatcher.UIThread.RunJobs();
        Button closeButton = controls.GetVisualDescendants().OfType<Button>().Single(button => button.Classes.Contains("window-close"));
        Button hideButton = controls.GetVisualDescendants().OfType<Button>().Single(button => button.Classes.Contains("window-hide"));
        if (!ReferenceEquals(closeButton.Command, host.CloseWindowCommand) || !ReferenceEquals(hideButton.Command, host.HideWindowCommand))
            throw new Exception("The unprefixed floating-window commands did not compile and bind.");
        ClickNative(closeButton);
        ClickNative(hideButton);
        if (closed != 1 || hidden != 1 || !host.IsVisible)
            throw new Exception("Window commands did not invoke the current item commands.");
        tool.CanClose = false;
        if (closeButton.IsEffectivelyEnabled) throw new Exception("The bound window command ignored tool capability changes.");
    }
    finally
    {
        item.CloseCommand = previousClose;
        item.HideCommand = previousHide;
        tool.CanClose = previousCanClose;
        view.Close();
    }
}

static void AssertDockingNotifications(DockingManager manager, Window window)
{
    LayoutDocument document = new() { ContentId = "event-probe", Title = "Docking events" };
    LayoutDocumentPane pane = manager.Layout.RootPanel.Descendents().OfType<LayoutDocumentPane>().First();
    pane.Children.Add(document);
    List<string> events = new();
    List<NotifyCollectionChangedEventArgs> hosts = new();
    void RecordHost(object? sender, LayoutFloatingWindowControlCollectionChangedEventArgs args)
    {
        if (!ReferenceEquals(sender, manager)) throw new Exception("Floating control notifications lost their manager.");
        hosts.Add(args.CollectionChangedEventArgs);
    }
    void Record(object? sender, RoutedEventArgs args)
    {
        if (!ReferenceEquals(sender, window) || !ReferenceEquals(args.Source, document))
            throw new Exception("The packaged docking event lost its public source or bubbling route.");
        events.Add(args.RoutedEvent!.Name);
    }
    window.AddHandler(DockingManager.PreviewFloatEvent, Record);
    window.AddHandler(DockingManager.FloatedEvent, Record);
    window.AddHandler(DockingManager.PreviewDockEvent, Record);
    window.AddHandler(DockingManager.DockedEvent, Record);
    manager.LayoutFloatingWindowControlCollectionChanged += RecordHost;
    try
    {
        document.Float();
        LayoutDocumentFloatingWindowControl host = manager.FloatingWindows.OfType<LayoutDocumentFloatingWindowControl>()
            .Single(value => ReferenceEquals(((LayoutDocumentFloatingWindow)value.Model).RootDocument, document));
        if (!ReferenceEquals(((LayoutFloatingWindow)host.Model).DockWindow.Host, host) ||
            !ReferenceEquals(host.RootDocumentLayoutItem, manager.GetLayoutItemFromModel(document)) ||
            hosts.Count != 1 || hosts[0].Action != NotifyCollectionChangedAction.Add || !ReferenceEquals(hosts[0].NewItems![0], host))
            throw new Exception("Floating controls did not expose the actual native host, model, and container.");
        if (window.Resources["ItemFromModel"] is not global::AvalonDock.Converters.LayoutItemFromLayoutModelConverter converter ||
            !ReferenceEquals(converter.Convert(document, typeof(LayoutItem), null, System.Globalization.CultureInfo.InvariantCulture), host.RootDocumentLayoutItem))
            throw new Exception("The unprefixed model-to-container converter did not resolve the packaged container.");
        Dock.Model.DockManager native = new(new Dock.Model.DockService());
        if (!native.ValidateDockable(document.DockModel, pane.DockModel, Dock.Model.Core.DragAction.Move,
                Dock.Model.Core.DockOperation.Fill, false) || events.Count != 2)
            throw new Exception("Native docking validation raised notifications or rejected the document.");
        if (!native.ValidateDockable(document.DockModel, pane.DockModel, Dock.Model.Core.DragAction.Move,
                Dock.Model.Core.DockOperation.Fill, true) || document.IsFloating ||
            !events.SequenceEqual(new[] { "PreviewFloat", "Floated", "PreviewDock", "Docked" }))
            throw new Exception("The packaged native docking operation did not raise one ordered event pair.");
        if (manager.FloatingWindows.Contains(host) || hosts.Count != 2 ||
            hosts[1].Action != NotifyCollectionChangedAction.Remove || !ReferenceEquals(hosts[1].OldItems![0], host))
            throw new Exception("Native docking did not remove the same floating control from the manager.");
        LayoutFloatingWindowControl created = manager.CreateFloatingWindow(document, true)
            ?? throw new Exception("The packaged manager did not create a floating window.");
        if (created.IsVisible || !created.IsContentImmutable || ReferenceEquals(created, host) ||
            !ReferenceEquals(((LayoutFloatingWindow)created.Model).DockWindow.Host, created))
            throw new Exception("The created window did not preserve deferred presentation and native host identity.");
        created.Show();
        Dispatcher.UIThread.RunJobs();
        object? originalContent = created.Content;
        created.Content = new TextBlock { Text = "Replacement content" };
        if (!ReferenceEquals(originalContent, created.Content) || !manager.DockFactory.HostWindows.Contains(created))
            throw new Exception("The explicitly shown window lost content protection or native registration.");
        EventHandler<Dock.Model.Core.Events.WindowClosingEventArgs> veto = (_, args) => args.Cancel = true;
        manager.DockFactory.WindowClosing += veto;
        LayoutFloatingWindow createdModel = (LayoutFloatingWindow)created.Model;
        try
        {
            document.Dock();
            if (!created.IsVisible || !created.IsTracked || !ReferenceEquals(createdModel.DockWindow.Host, created) ||
                !manager.FloatingWindows.Contains(created) || !manager.Layout.FloatingWindows.Contains(createdModel) ||
                !manager.DockFactory.HostWindows.Contains(created) ||
                ((Dock.Model.Core.IHostWindowCloseState)created).IsClosed)
                throw new Exception("Canceled native cleanup lost the live host or its ownership.");
        }
        finally { manager.DockFactory.WindowClosing -= veto; }
        manager.Layout.CollectGarbage();
        if (manager.FloatingWindows.Contains(created) || createdModel.DockWindow.Host is not null ||
            manager.Layout.FloatingWindows.Contains(createdModel) || manager.DockFactory.HostWindows.Contains(created) ||
            !((Dock.Model.Core.IHostWindowCloseState)created).IsClosed)
            throw new Exception("Retrying cleanup did not release the previously canceled floating window.");
    }
    finally
    {
        window.RemoveHandler(DockingManager.PreviewFloatEvent, Record);
        window.RemoveHandler(DockingManager.FloatedEvent, Record);
        window.RemoveHandler(DockingManager.PreviewDockEvent, Record);
        window.RemoveHandler(DockingManager.DockedEvent, Record);
        manager.LayoutFloatingWindowControlCollectionChanged -= RecordHost;
        document.Close();
    }
}

static void AssertDocumentMenu(DockingManager manager)
{
    Button button = manager.GetVisualDescendants().OfType<Button>().Single(value => value.Classes.Contains("avalondock-document-menu"));
    ClickNative(button);
    MenuFlyout menu = (MenuFlyout)button.Flyout!;
    if (!menu.IsOpen) throw new Exception("The document list menu did not open.");
    ItemsControl presenter = (ItemsControl)menu.Popup.Child!;
    MenuItem entry = (MenuItem)presenter.ContainerFromIndex(0)!;
    LayoutContent model = (LayoutContent)entry.DataContext!;
    TextBlock header = entry.GetVisualDescendants().OfType<TextBlock>().Single(value => value.Classes.Contains("package-menu-header"));
    TextBlock icon = entry.GetVisualDescendants().OfType<TextBlock>().Single(value => value.Classes.Contains("package-menu-icon"));
    if (!ReferenceEquals(header.DataContext, model) || !ReferenceEquals(icon.DataContext, model) || icon.Text != model.ContentId)
        throw new Exception("Packaged document menu templates did not receive the public layout content.");
    if (entry is not MenuItemEx iconEntry)
        throw new Exception("The packaged document list did not create a public MenuItemEx container.");
    object? previousIcon = iconEntry.Icon;
    IDataTemplate? previousTemplate = manager.IconContentTemplate;
    manager.IconContentTemplateSelector = new DataTemplateSelector();
    manager.IconContentTemplate = manager.DocumentPaneMenuItemHeaderTemplate;
    Dispatcher.UIThread.RunJobs();
    if (!ReferenceEquals(previousIcon, iconEntry.Icon))
        throw new Exception("A null packaged icon selector result replaced the existing icon.");
    manager.IconContentTemplateSelector = null;
    manager.IconContentTemplate = previousTemplate;
    Dispatcher.UIThread.RunJobs();
    ClickNative(entry);
    if (!model.IsActive || menu.IsOpen) throw new Exception("The document list did not activate and dismiss through the native menu.");
}

static void AssertSplitters(DockingManager manager)
{
    ProportionalStackPanelSplitter[] splitters = manager.GetVisualDescendants().OfType<ProportionalStackPanelSplitter>()
        .Where(splitter => splitter.DataContext is Dock.Model.Controls.IProportionalDockSplitter).ToArray();
    if (splitters.Length == 0) throw new Exception("The package consumer did not render a native splitter.");
    foreach (ProportionalStackPanelSplitter splitter in splitters)
    {
        ProportionalStackPanel panel = splitter.GetVisualAncestors().OfType<ProportionalStackPanel>().First();
        bool horizontal = panel.Orientation == Avalonia.Layout.Orientation.Horizontal;
        double expected = horizontal ? manager.GridSplitterWidth : manager.GridSplitterHeight;
        if (splitter.Thickness != expected || (horizontal ? splitter.Bounds.Width : splitter.Bounds.Height) != expected)
            throw new Exception("The packaged splitter did not apply the manager's size to its rendered bounds.");
    }
}

static void AssertToolTitle(Control scope, LayoutAnchorable tool)
{
    AnchorablePaneTitle title = scope.GetVisualDescendants().OfType<AnchorablePaneTitle>()
        .Single(control => control.IsEffectivelyVisible && ReferenceEquals(control.Model, tool));
    if (!ReferenceEquals(title.LayoutItem, tool.Root!.Manager!.GetLayoutItemFromModel(tool)))
        throw new Exception("The native tool title did not expose the manager's existing item container.");
    if (!scope.GetVisualDescendants().OfType<TextBlock>().Any(text => text.IsEffectivelyVisible &&
        text.Classes.Contains("package-tool-title") && ReferenceEquals(text.DataContext, tool) && text.Text == "Title: " + tool.Title))
        throw new Exception("The packaged tool title template did not render the selected public model and its current title.");
}

static void ClickNative(Control control, MouseButton button = MouseButton.Left)
{
    TopLevel topLevel = TopLevel.GetTopLevel(control)!;
    // Apply queued host size/state changes before computing pointer coordinates.
    Dispatcher.UIThread.RunJobs();
    topLevel.UpdateLayout();
    Point point = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), topLevel)!.Value;
    topLevel.MouseMove(point);
    IInputElement? hit = topLevel.InputHitTest(point);
    if (!ReferenceEquals(hit, control) && (hit is not Visual visual || !visual.GetVisualAncestors().Contains(control)))
        throw new Exception($"The {control.GetType().Name} target at {point} is not hit-testable: hit {hit?.GetType().Name}, control {control.Bounds}, host {topLevel.Bounds}.");
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
    public sealed class ConsumerDictionaryTheme(ResourceDictionary resources) : DictionaryTheme(resources) { }
    public sealed class CloseOriginProbe(ILayoutElement model) : LayoutFloatingWindowControl(model)
    {
        public override ILayoutElement Model { get; } = model;
        public bool UserOrigin => CloseInitiatedByUser;
    }

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
