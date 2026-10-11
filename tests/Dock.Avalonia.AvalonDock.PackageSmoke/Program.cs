using System;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Xml;
using System.Reactive.Concurrency;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Controls.ProportionalStackPanel;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using ReactiveUI.Reactive;

AppBuilder.Configure<PackageConsumer.ConsumerApp>().UseHeadless(new AvaloniaHeadlessPlatformOptions()).SetupWithoutStarting();
PackageConsumer.CloseOriginProbe closeOrigin = new(new LayoutDocumentFloatingWindow());
if (!closeOrigin.UserOrigin) throw new Exception("A new floating window did not expose user close origin.");
((Dock.Model.Core.IHostWindow)closeOrigin).Exit();
if (closeOrigin.UserOrigin) throw new Exception("Native host exit did not expose internal close origin.");
PackageConsumer.ConsumerWindow window = new();
if (window.Resources["Container"] is not LayoutDocumentItem { Description: "Unprefixed packaged container" })
    throw new Exception("The packaged item container did not compile in the default XML namespace.");
window.Show();
Dispatcher.UIThread.RunJobs();
DockingManager manager = window.FindControl<DockingManager>("Manager")!;
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
AssertDocumentMenu(manager);
AssertSplitters(manager);
manager.GridSplitterWidth = 28;
manager.GridSplitterHeight = 18;
Dispatcher.UIThread.RunJobs();
window.UpdateLayout();
AssertSplitters(manager);
tool.Float();
Dispatcher.UIThread.RunJobs();
HostWindow toolHost = (HostWindow)tool.FindParent<LayoutFloatingWindow>()!.DockWindow.Host!;
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
Console.WriteLine("NuGet consumer: default XML namespace, compiled XAML, Dock rendering, item containers and commands, native tab command overrides, custom manager context menus, document list header/icon templates and activation, container styles/selectors, manager templates/selectors, tab/auto-hide headers and selector precedence, tool and floating document titles/selectors, document caption maximization and close commands, configurable native splitters, auto-hide and floating XML, model XML contracts, observable sources, floating templates, pane return locations, bubbling native docking notifications, typed floating controls and collection notifications, model-to-container conversion, and close cancellation passed.");

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
    ClickNative(entry);
    if (!model.IsActive || menu.IsOpen) throw new Exception("The document list did not activate and dismiss through the native menu.");
}

static void AssertSplitters(DockingManager manager)
{
    ProportionalStackPanelSplitter[] splitters = manager.GetVisualDescendants().OfType<ProportionalStackPanelSplitter>().ToArray();
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
