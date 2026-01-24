using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using Dock.Avalonia.Converters;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Avalonia.Mdi;
using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;
using AvaloniaPointer = Avalonia.Input.Pointer;

namespace Dock.Avalonia.HeadlessTests;

public class ManagedWindowParityTests
{
    private static (ManagedHostWindow Host, DockWindow Window, IRootDock Root) CreateManagedWindow(Factory factory)
    {
        return CreateManagedWindow(factory, new DockWindow());
    }

    private static (ManagedHostWindow Host, DockWindow Window, IRootDock Root) CreateManagedWindow(
        Factory factory,
        DockWindow window)
    {
        var root = factory.CreateRootDock();
        root.Factory = factory;

        window.Factory = factory;
        window.Layout = root;
        root.Window = window;

        var host = new ManagedHostWindow
        {
            Window = window
        };
        window.Host = host;
        host.Present(false);

        return (host, window, root);
    }

    [AvaloniaFact]
    public void ActiveDockable_Switch_Raises_Window_Activated_Deactivated()
    {
        var factory = new Factory();
        var managed1 = CreateManagedWindow(factory);
        var managed2 = CreateManagedWindow(factory);
        var root1 = managed1.Root;
        var root2 = managed2.Root;

        var dockable1 = factory.CreateDocument();
        dockable1.Title = "Doc1";
        root1.VisibleDockables = factory.CreateList<IDockable>(dockable1);
        root1.ActiveDockable = dockable1;

        var dockable2 = factory.CreateDocument();
        dockable2.Title = "Doc2";
        root2.VisibleDockables = factory.CreateList<IDockable>(dockable2);
        root2.ActiveDockable = dockable2;

        var dock = ManagedWindowRegistry.GetOrCreateDock(factory);
        var documents = dock.VisibleDockables!.OfType<ManagedDockWindowDocument>().ToList();

        var doc1 = documents.Single(d => ReferenceEquals(d.Window, managed1.Window));
        var doc2 = documents.Single(d => ReferenceEquals(d.Window, managed2.Window));

        var activated = new List<IDockWindow>();
        var deactivated = new List<IDockWindow>();
        var dockableActivated = new List<IDockable>();
        var dockableDeactivated = new List<IDockable>();

        factory.WindowActivated += (_, e) => activated.Add(e.Window!);
        factory.WindowDeactivated += (_, e) => deactivated.Add(e.Window!);
        factory.DockableActivated += (_, e) =>
        {
            if (e.Dockable is { } dockable)
            {
                dockableActivated.Add(dockable);
            }
        };
        factory.DockableDeactivated += (_, e) =>
        {
            if (e.Dockable is { } dockable)
            {
                dockableDeactivated.Add(dockable);
            }
        };

        dock.ActiveDockable = doc1;

        Assert.Contains(managed1.Window, activated);
        Assert.Contains(managed2.Window, deactivated);
        Assert.Contains(dockable1, dockableActivated);
        Assert.Contains(dockable2, dockableDeactivated);
        Assert.Equal(doc1, dock.ActiveDockable);
        Assert.NotSame(doc2, dock.ActiveDockable);
    }

    [AvaloniaFact]
    public void Topmost_Window_Receives_Higher_ZIndex()
    {
        var factory = new Factory();
        var managed1 = CreateManagedWindow(factory);
        var managed2 = CreateManagedWindow(factory);

        var dock = ManagedWindowRegistry.GetOrCreateDock(factory);
        _ = new ManagedWindowLayer { Dock = dock };

        var documents = dock.VisibleDockables!.OfType<ManagedDockWindowDocument>().ToList();
        var doc1 = documents.Single(d => ReferenceEquals(d.Window, managed1.Window));
        var doc2 = documents.Single(d => ReferenceEquals(d.Window, managed2.Window));

        managed1.Window.Topmost = true;

        Assert.True(doc1.MdiZIndex > doc2.MdiZIndex);
    }

    [AvaloniaFact]
    public void ManagedDockWindowDocument_Tracks_FocusedDockable_Title()
    {
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.Factory = factory;

        var doc1 = factory.CreateDocument();
        doc1.Title = "Doc1";
        root.VisibleDockables = factory.CreateList<IDockable>(doc1);
        root.ActiveDockable = doc1;
        root.FocusedDockable = doc1;

        var window = new DockWindow
        {
            Factory = factory,
            Layout = root
        };
        root.Window = window;

        var managedDoc = new ManagedDockWindowDocument(window);
        Assert.Equal("Doc1", managedDoc.Title);

        var doc2 = factory.CreateDocument();
        doc2.Title = "Doc2";
        root.VisibleDockables!.Add(doc2);
        root.FocusedDockable = doc2;

        Assert.Equal("Doc2", managedDoc.Title);
    }

    [AvaloniaFact]
    public void ApplyWindowMagnetism_Snaps_To_Adjacent_Window()
    {
        var original = DockSettings.WindowMagnetDistance;
        try
        {
            DockSettings.WindowMagnetDistance = 10;

            var current = new Document { MdiBounds = new DockRect(95, 0, 50, 50) };
            var other = new Document { MdiBounds = new DockRect(150, 0, 50, 50) };

            var entries = new List<MdiLayoutEntry>
            {
                new(new Control(), current),
                new(new Control(), other)
            };

            var method = typeof(MdiDocumentWindow).GetMethod("ApplyWindowMagnetism", BindingFlags.NonPublic | BindingFlags.Static);
            var bounds = new Rect(95, 0, 50, 50);
            var result = (Rect)method!.Invoke(null, new object[] { bounds, current, entries })!;

            Assert.Equal(100, result.X);
        }
        finally
        {
            DockSettings.WindowMagnetDistance = original;
        }
    }

    [AvaloniaFact]
    public void BeginDrag_Respects_IsDragEnabled()
    {
        var doc = new Document { Title = "Doc", CanDrag = true };
        var mdiWindow = new MdiDocumentWindow
        {
            Width = 200,
            Height = 200
        };
        var host = new Window
        {
            Width = 400,
            Height = 300,
            Content = mdiWindow
        };

        host.Show();
        try
        {
            mdiWindow.DataContext = doc;
            mdiWindow.ApplyTemplate();
            host.UpdateLayout();
            mdiWindow.UpdateLayout();

            mdiWindow.SetValue(DockProperties.IsDragEnabledProperty, false);

            var pointer = new AvaloniaPointer(1, PointerType.Mouse, true);
            var props = new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
            var args = new PointerPressedEventArgs(mdiWindow, pointer, host, new Point(5, 5), 0, props, KeyModifiers.None, 1);

            var beginDrag = typeof(MdiDocumentWindow).GetMethod("BeginDrag", BindingFlags.Instance | BindingFlags.NonPublic);
            beginDrag!.Invoke(mdiWindow, new object[] { args });

            var isDraggingField = typeof(MdiDocumentWindow).GetField("_isDragging", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.False((bool)isDraggingField!.GetValue(mdiWindow)!);
        }
        finally
        {
            host.Close();
        }
    }

    [AvaloniaFact]
    public void PointerTracking_Uses_ManagedLayer_Coordinates()
    {
        var original = DockSettings.UseManagedWindows;
        DockSettings.UseManagedWindows = true;

        var document = new Document { Title = "Doc" };
        var dockableControl = new DockableControl
        {
            Width = 120,
            Height = 80,
            DataContext = document
        };

        var layer = new ManagedWindowLayer
        {
            Width = 200,
            Height = 200,
            IsVisible = true
        };

        var header = new Border { Height = 24 };

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("24,*"),
            Children =
            {
                header,
                layer,
                dockableControl
            }
        };

        Grid.SetRow(layer, 1);
        Grid.SetRow(dockableControl, 1);

        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = grid
        };

        try
        {
            window.Show();
            window.UpdateLayout();

            var pointer = new AvaloniaPointer(2, PointerType.Mouse, true);
            var props = new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
            var dockableOrigin = dockableControl.TranslatePoint(new Point(0, 0), window) ?? new Point(0, 0);
            var rootPoint = new Point(dockableOrigin.X + 10, dockableOrigin.Y + 10);
            var args = new PointerPressedEventArgs(dockableControl, pointer, window, rootPoint, 0, props, KeyModifiers.None, 1);

            dockableControl.RaiseEvent(args);

            document.GetPointerScreenPosition(out var pointerX, out var pointerY);
            var expected = dockableControl.TranslatePoint(new Point(10, 10), layer);

            Assert.True(expected.HasValue);
            Assert.Equal(expected.Value.X, pointerX, 3);
            Assert.Equal(expected.Value.Y, pointerY, 3);
        }
        finally
        {
            window.Close();
            DockSettings.UseManagedWindows = original;
        }
    }

    [AvaloniaFact]
    public void ManagedPreview_Uses_VisualBrush_Proxy()
    {
        var window = new DockWindow { Title = "Preview" };
        var managedDoc = new ManagedDockWindowDocument(window);
        var content = new Panel();
        managedDoc.Content = content;

        DragPreviewContext.IsActive = true;
        DragPreviewContext.Dockable = managedDoc;

        try
        {
            var preview = managedDoc.Build(null);
            var border = Assert.IsType<Border>(preview);
            var brush = Assert.IsType<VisualBrush>(border.Background);

            Assert.Same(content, brush.Visual);
        }
        finally
        {
            DragPreviewContext.Clear();
        }
    }

    [AvaloniaFact]
    public void ManagedHostWindowDrag_Does_Not_Show_DragPreview()
    {
        var originalPreview = DockSettings.ShowDockablePreviewOnDrag;
        var originalManaged = DockSettings.UseManagedWindows;
        DockSettings.ShowDockablePreviewOnDrag = true;
        DockSettings.UseManagedWindows = true;

        var factory = new Factory();
        var (host, window, _) = CreateManagedWindow(factory);
        var dock = ManagedWindowRegistry.GetOrCreateDock(factory);
        var managedDocument = dock.VisibleDockables!.OfType<ManagedDockWindowDocument>()
            .Single(document => ReferenceEquals(document.Window, window));
        managedDocument.MdiBounds = new DockRect(0, 0, 200, 150);

        DragPreviewContext.Clear();
        try
        {
            var state = new ManagedHostWindowState(new DockManager(new DockService()), host);
            state.Process(new PixelPoint(10, 10), EventType.Pressed);
            state.Process(new PixelPoint(80, 80), EventType.Moved);

            Assert.False(DragPreviewContext.IsActive);
            Assert.Null(DragPreviewContext.Dockable);

            state.Process(new PixelPoint(80, 80), EventType.Released);
            Assert.False(DragPreviewContext.IsActive);
        }
        finally
        {
            DragPreviewContext.Clear();
            DockSettings.ShowDockablePreviewOnDrag = originalPreview;
            DockSettings.UseManagedWindows = originalManaged;
        }
    }

    [AvaloniaFact]
    public void ManagedHostWindowDrag_CaptureLost_Does_Not_Activate_DragPreview()
    {
        var originalPreview = DockSettings.ShowDockablePreviewOnDrag;
        var originalManaged = DockSettings.UseManagedWindows;
        DockSettings.ShowDockablePreviewOnDrag = true;
        DockSettings.UseManagedWindows = true;

        var factory = new Factory();
        var (host, window, _) = CreateManagedWindow(factory);
        var dock = ManagedWindowRegistry.GetOrCreateDock(factory);
        var managedDocument = dock.VisibleDockables!.OfType<ManagedDockWindowDocument>()
            .Single(document => ReferenceEquals(document.Window, window));
        managedDocument.MdiBounds = new DockRect(0, 0, 200, 150);

        DragPreviewContext.Clear();
        try
        {
            var state = new ManagedHostWindowState(new DockManager(new DockService()), host);
            state.Process(new PixelPoint(10, 10), EventType.Pressed);
            state.Process(new PixelPoint(80, 80), EventType.Moved);

            Assert.False(DragPreviewContext.IsActive);

            state.Process(new PixelPoint(80, 80), EventType.CaptureLost);
            Assert.False(DragPreviewContext.IsActive);
        }
        finally
        {
            DragPreviewContext.Clear();
            DockSettings.ShowDockablePreviewOnDrag = originalPreview;
            DockSettings.UseManagedWindows = originalManaged;
        }
    }

    [AvaloniaFact]
    public void DragPreviewHelper_Uses_Managed_Layer_Overlay()
    {
        var originalPreview = DockSettings.ShowDockablePreviewOnDrag;
        var originalManaged = DockSettings.UseManagedWindows;
        DockSettings.ShowDockablePreviewOnDrag = true;
        DockSettings.UseManagedWindows = true;

        var factory = new Factory();
        var (host, window, _) = CreateManagedWindow(factory);
        var dock = ManagedWindowRegistry.GetOrCreateDock(factory);
        var managedDocument = dock.VisibleDockables!.OfType<ManagedDockWindowDocument>()
            .Single(document => ReferenceEquals(document.Window, window));

        var layer = new ManagedWindowLayer
        {
            Dock = dock,
            Width = 300,
            Height = 200,
            IsVisible = true
        };

        var windowHost = new Window
        {
            Width = 400,
            Height = 300,
            Content = layer
        };

        windowHost.Show();
        DragPreviewContext.Clear();
        DragPreviewHelper? helper = null;
        try
        {
            layer.ApplyTemplate();
            windowHost.UpdateLayout();

            ManagedWindowRegistry.RegisterLayer(factory, layer);

            helper = new DragPreviewHelper();
            helper.Show(managedDocument, new PixelPoint(50, 60), new PixelPoint(0, 0));

            windowHost.UpdateLayout();

            var canvas = layer.GetVisualDescendants()
                .OfType<Canvas>()
                .FirstOrDefault(candidate => candidate.Name == "PART_OverlayCanvas");

            Assert.NotNull(canvas);
            Assert.Contains(canvas!.Children, child => child is DragPreviewControl);
            var preview = canvas.Children.OfType<DragPreviewControl>().FirstOrDefault();
            Assert.NotNull(preview);
            Assert.True(preview!.Bounds.Width > 0);
            Assert.True(preview.Bounds.Height > 0);

            helper.Hide();
            Assert.DoesNotContain(canvas.Children, child => child is DragPreviewControl);
        }
        finally
        {
            helper?.Hide();
            DragPreviewContext.Clear();
            ManagedWindowRegistry.UnregisterLayer(factory, layer);
            windowHost.Close();
            host.Exit();
            DockSettings.ShowDockablePreviewOnDrag = originalPreview;
            DockSettings.UseManagedWindows = originalManaged;
        }
    }

    [AvaloniaFact]
    public void DragPreviewHelper_ManagedToolWindow_Disables_PreviewContent()
    {
        var originalPreview = DockSettings.ShowDockablePreviewOnDrag;
        var originalManaged = DockSettings.UseManagedWindows;
        DockSettings.ShowDockablePreviewOnDrag = true;
        DockSettings.UseManagedWindows = true;

        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.Factory = factory;

        var toolDock = factory.CreateToolDock();
        root.VisibleDockables = factory.CreateList<IDockable>(toolDock);
        root.ActiveDockable = toolDock;
        root.FocusedDockable = toolDock;

        var dockWindow = new DockWindow
        {
            Factory = factory,
            Layout = root
        };
        root.Window = dockWindow;

        var managedDocument = new ManagedDockWindowDocument(dockWindow);
        var dock = ManagedWindowRegistry.GetOrCreateDock(factory);
        dock.AddWindow(managedDocument);

        var layer = new ManagedWindowLayer
        {
            Dock = dock,
            Width = 400,
            Height = 300,
            IsVisible = true
        };

        var windowHost = new Window
        {
            Width = 500,
            Height = 400,
            Content = layer
        };

        windowHost.Show();
        DragPreviewContext.Clear();
        DragPreviewHelper? helper = null;
        try
        {
            layer.ApplyTemplate();
            windowHost.UpdateLayout();

            ManagedWindowRegistry.RegisterLayer(factory, layer);

            helper = new DragPreviewHelper();
            helper.Show(managedDocument, new PixelPoint(50, 60), new PixelPoint(0, 0));

            windowHost.UpdateLayout();

            var canvas = layer.GetVisualDescendants()
                .OfType<Canvas>()
                .FirstOrDefault(candidate => candidate.Name == "PART_OverlayCanvas");

            Assert.NotNull(canvas);
            var preview = canvas!.Children.OfType<DragPreviewControl>().FirstOrDefault();
            Assert.NotNull(preview);
            Assert.False(preview!.ShowContent);

            helper.Hide();
            Assert.DoesNotContain(canvas.Children, child => child is DragPreviewControl);
        }
        finally
        {
            helper?.Hide();
            DragPreviewContext.Clear();
            ManagedWindowRegistry.UnregisterLayer(factory, layer);
            windowHost.Close();
            DockSettings.ShowDockablePreviewOnDrag = originalPreview;
            DockSettings.UseManagedWindows = originalManaged;
        }
    }

    [AvaloniaFact]
    public void DragPreviewHelper_Uses_Managed_Layer_From_Visual_Context()
    {
        var originalPreview = DockSettings.ShowDockablePreviewOnDrag;
        var originalManaged = DockSettings.UseManagedWindows;
        DockSettings.ShowDockablePreviewOnDrag = true;
        DockSettings.UseManagedWindows = true;

        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.Factory = factory;

        var document = factory.CreateDocument();
        document.Title = "Doc";
        root.VisibleDockables = factory.CreateList<IDockable>(document);
        root.ActiveDockable = document;

        var dockControl = new DockControl
        {
            Layout = root
        };

        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = dockControl
        };

        window.Show();
        DragPreviewContext.Clear();
        try
        {
            dockControl.ApplyTemplate();
            window.UpdateLayout();

            var helper = new DragPreviewHelper();
            helper.Show(document, new PixelPoint(50, 60), new PixelPoint(0, 0), dockControl);

            var layer = dockControl.GetVisualDescendants().OfType<ManagedWindowLayer>().FirstOrDefault();
            Assert.NotNull(layer);

            var canvas = layer!.GetVisualDescendants()
                .OfType<Canvas>()
                .FirstOrDefault(candidate => candidate.Name == "PART_OverlayCanvas");

            Assert.NotNull(canvas);
            Assert.Contains(canvas!.Children, child => child is DragPreviewControl);
            window.UpdateLayout();
            var preview = canvas.Children.OfType<DragPreviewControl>().FirstOrDefault();
            Assert.NotNull(preview);
            Assert.True(preview!.Bounds.Width > 0);
            Assert.True(preview.Bounds.Height > 0);

            helper.Hide();
            Assert.DoesNotContain(canvas.Children, child => child is DragPreviewControl);
        }
        finally
        {
            DragPreviewContext.Clear();
            window.Close();
            DockSettings.ShowDockablePreviewOnDrag = originalPreview;
            DockSettings.UseManagedWindows = originalManaged;
        }
    }

    [AvaloniaFact]
    public void DragPreviewControl_Uses_PreviewContent_When_Set()
    {
        var preview = new Border { Name = "Preview" };
        var control = new DragPreviewControl
        {
            PreviewContent = preview,
            ShowContent = true,
            PreviewContentWidth = 120,
            PreviewContentHeight = 80
        };

        var window = new Window
        {
            Width = 300,
            Height = 200,
            Content = control
        };

        window.Show();
        try
        {
            control.ApplyTemplate();
            window.UpdateLayout();
            control.UpdateLayout();

            var previewPresenter = control.GetVisualDescendants()
                .OfType<ContentControl>()
                .FirstOrDefault(candidate => candidate.Name == "PART_PreviewContentPresenter");
            var contentPresenter = control.GetVisualDescendants()
                .OfType<ContentControl>()
                .FirstOrDefault(candidate => candidate.Name == "PART_ContentPresenter");

            Assert.NotNull(previewPresenter);
            Assert.Same(preview, previewPresenter!.Content);
            Assert.True(previewPresenter.IsVisible);
            Assert.NotNull(contentPresenter);
            Assert.False(contentPresenter!.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void FloatingDockable_Uses_Active_Managed_Layer_For_Pointer()
    {
        var originalManaged = DockSettings.UseManagedWindows;
        DockSettings.UseManagedWindows = true;

        Window? windowA = null;
        Window? windowB = null;
        ManagedWindowLayer? layerA = null;
        ManagedWindowLayer? layerB = null;
        Factory? factory = null;

        try
        {
            factory = new Factory();
            var rootA = factory.CreateRootDock();
            rootA.Factory = factory;
            var rootB = factory.CreateRootDock();
            rootB.Factory = factory;

            var dockControlA = new DockControl { Layout = rootA };
            var dockControlB = new DockControl { Layout = rootB };

            windowA = new Window
            {
                Width = 400,
                Height = 300,
                Position = new PixelPoint(0, 0),
                Content = dockControlA
            };

            windowB = new Window
            {
                Width = 400,
                Height = 300,
                Position = new PixelPoint(200, 120),
                Content = dockControlB
            };

            windowA.Show();
            windowB.Show();
            dockControlA.ApplyTemplate();
            dockControlB.ApplyTemplate();
            windowA.UpdateLayout();
            windowB.UpdateLayout();

            layerA = dockControlA.GetVisualDescendants().OfType<ManagedWindowLayer>().FirstOrDefault();
            layerB = dockControlB.GetVisualDescendants().OfType<ManagedWindowLayer>().FirstOrDefault();
            Assert.NotNull(layerA);
            Assert.NotNull(layerB);

            layerA!.IsVisible = true;
            layerB!.IsVisible = true;

            ManagedWindowRegistry.RegisterLayer(factory, layerA);
            layerB.IsVisible = true;

            var dockable = new Document { Title = "Doc" };
            var state = new TestDockManagerState(new DockManager(new DockService()));
            var point = new Point(50, 60);
            var dragOffset = new PixelPoint(-12, -8);

            state.InvokeFloat(point, dockControlB, dockable, factory, dragOffset);

            var translated = dockControlB.TranslatePoint(point, layerB);
            Assert.True(translated.HasValue);
            var scaling = (dockControlB.GetVisualRoot() as TopLevel)
                ?.Screens
                ?.ScreenFromVisual(dockControlB)
                ?.Scaling ?? 1.0;
            var expected = new Point(
                translated!.Value.X + dragOffset.X / scaling,
                translated.Value.Y + dragOffset.Y / scaling);

            dockable.GetPointerScreenPosition(out var pointerX, out var pointerY);

            Assert.Equal(expected.X, pointerX, 3);
            Assert.Equal(expected.Y, pointerY, 3);
        }
        finally
        {
            if (layerA is { } && factory is { })
            {
                ManagedWindowRegistry.UnregisterLayer(factory, layerA);
            }

            windowA?.Close();
            windowB?.Close();
            DockSettings.UseManagedWindows = originalManaged;
        }
    }

    [AvaloniaFact]
    public void ManagedHostWindow_Present_Registers_Window_And_Document()
    {
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.Factory = factory;

        var window = new DockWindow
        {
            Factory = factory,
            Layout = root,
            Title = "Host"
        };
        root.Window = window;

        var host = new ManagedHostWindow
        {
            Window = window
        };

        var opened = new List<IDockWindow>();
        factory.WindowOpened += (_, e) => opened.Add(e.Window!);

        host.Present(false);

        Assert.True(host.IsTracked);
        Assert.Same(host, window.Host);
        Assert.Contains(host, factory.HostWindows);
        Assert.Contains(window, opened);

        var dock = ManagedWindowRegistry.GetOrCreateDock(factory);
        var managedDocument = dock.VisibleDockables!.OfType<ManagedDockWindowDocument>()
            .Single(document => ReferenceEquals(document.Window, window));

        Assert.Same(dock, managedDocument.Owner);
        Assert.Same(factory, managedDocument.Factory);
        Assert.Same(managedDocument, dock.ActiveDockable);
        Assert.IsType<DockControl>(managedDocument.Content);
    }

    [AvaloniaFact]
    public void ManagedHostWindow_Present_Without_Factory_Does_Not_Track()
    {
        var window = new DockWindow();
        var host = new ManagedHostWindow
        {
            Window = window
        };

        host.Present(false);

        Assert.False(host.IsTracked);
        Assert.Null(window.Host);
    }

    [AvaloniaFact]
    public void ManagedHostWindow_SetTitle_Updates_Document_And_Window()
    {
        var factory = new Factory();
        var (host, window, _) = CreateManagedWindow(factory);
        var dock = ManagedWindowRegistry.GetOrCreateDock(factory);
        var managedDocument = dock.VisibleDockables!.OfType<ManagedDockWindowDocument>()
            .Single(document => ReferenceEquals(document.Window, window));

        host.SetTitle("Updated");

        Assert.Equal("Updated", managedDocument.Title);
        Assert.Equal("Updated", window.Title);
    }

    [AvaloniaFact]
    public void ManagedHostWindow_Exit_Removes_Window_And_Document()
    {
        var factory = new Factory();
        var (host, window, _) = CreateManagedWindow(factory);

        var closed = new List<IDockWindow>();
        factory.WindowClosed += (_, e) => closed.Add(e.Window!);

        host.Exit();

        Assert.False(host.IsTracked);
        Assert.Null(window.Host);
        Assert.DoesNotContain(host, factory.HostWindows);
        Assert.Contains(window, closed);

        var dock = ManagedWindowRegistry.GetOrCreateDock(factory);
        Assert.DoesNotContain(
            dock.VisibleDockables!.OfType<ManagedDockWindowDocument>(),
            document => ReferenceEquals(document.Window, window));
    }

    [AvaloniaFact]
    public void ManagedHostWindow_Exit_Can_Be_Canceled()
    {
        var factory = new Factory();
        var (host, window, _) = CreateManagedWindow(factory, new TestDockWindow());
        factory.WindowClosing += (_, e) => e.Cancel = true;

        host.Exit();

        Assert.True(host.IsTracked);
        Assert.Same(host, window.Host);
        Assert.Contains(host, factory.HostWindows);

        var dock = ManagedWindowRegistry.GetOrCreateDock(factory);
        var managedDocument = dock.VisibleDockables!.OfType<ManagedDockWindowDocument>()
            .Single(document => ReferenceEquals(document.Window, window));
        Assert.Same(managedDocument, dock.ActiveDockable);
    }

    [AvaloniaFact]
    public void ManagedHostWindow_SetPosition_And_Size_Ignore_NaN()
    {
        var host = new ManagedHostWindow();
        host.SetPosition(10, 20);
        host.SetSize(200, 150);

        host.SetPosition(double.NaN, 30);
        host.SetSize(double.NaN, 300);

        host.GetPosition(out var x, out var y);
        host.GetSize(out var width, out var height);

        Assert.Equal(10, x);
        Assert.Equal(20, y);
        Assert.Equal(200, width);
        Assert.Equal(150, height);
    }

    [AvaloniaFact]
    public void ManagedHostWindow_SetLayout_Updates_Content()
    {
        var factory = new Factory();
        var (host, window, _) = CreateManagedWindow(factory);
        var dock = ManagedWindowRegistry.GetOrCreateDock(factory);
        var managedDocument = dock.VisibleDockables!.OfType<ManagedDockWindowDocument>()
            .Single(document => ReferenceEquals(document.Window, window));
        var previousContent = managedDocument.Content;

        var newRoot = factory.CreateRootDock();
        newRoot.Factory = factory;
        host.SetLayout(newRoot);

        var newContent = Assert.IsType<DockControl>(managedDocument.Content);
        Assert.NotSame(previousContent, newContent);
        Assert.Same(newRoot, newContent.Layout);
    }

    [AvaloniaFact]
    public void ManagedHostWindow_SetActive_Activates_Document()
    {
        var factory = new Factory();
        var (host, window, _) = CreateManagedWindow(factory);

        var dock = ManagedWindowRegistry.GetOrCreateDock(factory);
        var managedDocument = dock.VisibleDockables!.OfType<ManagedDockWindowDocument>()
            .Single(document => ReferenceEquals(document.Window, window));

        dock.ActiveDockable = null;
        host.SetActive();

        Assert.Same(managedDocument, dock.ActiveDockable);
    }

    [AvaloniaFact]
    public void ManagedDockWindowDocument_Syncs_Title_And_Id_To_Window()
    {
        var window = new DockWindow { Title = "Original", Id = "win-1" };
        var managedDocument = new ManagedDockWindowDocument(window);

        managedDocument.Title = "Updated";
        managedDocument.Id = "win-2";

        Assert.Equal("Updated", window.Title);
        Assert.Equal("win-2", window.Id);
    }

    [AvaloniaFact]
    public void ManagedDockWindowDocument_Window_Title_Change_Does_Not_Override_Focused_Dockable()
    {
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.Factory = factory;

        var document = factory.CreateDocument();
        document.Title = "DocTitle";
        root.VisibleDockables = factory.CreateList<IDockable>(document);
        root.ActiveDockable = document;
        root.FocusedDockable = document;

        var window = new DockWindow
        {
            Factory = factory,
            Layout = root,
            Title = "WindowTitle"
        };
        root.Window = window;

        var managedDocument = new ManagedDockWindowDocument(window);
        window.Title = "UpdatedWindow";

        Assert.Equal("DocTitle", managedDocument.Title);
    }

    [AvaloniaFact]
    public void ManagedDockWindowDocument_Updates_Title_When_Window_Title_Changes()
    {
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.Factory = factory;

        var window = new DockWindow
        {
            Factory = factory,
            Layout = root,
            Title = "Initial"
        };
        root.Window = window;

        var managedDocument = new ManagedDockWindowDocument(window);
        window.Title = "Updated";

        Assert.Equal("Updated", managedDocument.Title);
    }

    [AvaloniaFact]
    public void ManagedDockWindowDocument_Updates_Title_When_Layout_Swapped()
    {
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.Factory = factory;

        var window = new DockWindow
        {
            Factory = factory,
            Layout = root
        };
        root.Window = window;

        var initialDocument = factory.CreateDocument();
        initialDocument.Title = "Doc1";
        root.VisibleDockables = factory.CreateList<IDockable>(initialDocument);
        root.FocusedDockable = initialDocument;

        var managedDocument = new ManagedDockWindowDocument(window);
        Assert.Equal("Doc1", managedDocument.Title);

        var newRoot = factory.CreateRootDock();
        newRoot.Factory = factory;
        var newDocument = factory.CreateDocument();
        newDocument.Title = "Doc2";
        newRoot.VisibleDockables = factory.CreateList<IDockable>(newDocument);
        newRoot.FocusedDockable = newDocument;
        newRoot.Window = window;

        window.Layout = newRoot;

        Assert.Equal("Doc2", managedDocument.Title);
    }

    [AvaloniaFact]
    public void ManagedDockWindowDocument_Build_Uses_Direct_Content()
    {
        var window = new DockWindow { Title = "Content" };
        var managedDocument = new ManagedDockWindowDocument(window);
        var content = new Border { Name = "DirectContent" };
        managedDocument.Content = content;

        var result = managedDocument.Build(null);

        Assert.Same(content, result);
    }

    [AvaloniaFact]
    public void ManagedDockWindowDocument_Build_Uses_Func_Content()
    {
        var window = new DockWindow { Title = "Content" };
        var managedDocument = new ManagedDockWindowDocument(window);
        managedDocument.Content = new Func<IServiceProvider, object>(_ => new Border { Name = "FactoryContent" });

        var result = managedDocument.Build(null);

        var border = Assert.IsType<Border>(result);
        Assert.Equal("FactoryContent", border.Name);
    }

    [AvaloniaFact]
    public void ManagedDockWindowDocument_Match_Respects_DataType()
    {
        var window = new DockWindow { Title = "Match" };
        var managedDocument = new ManagedDockWindowDocument(window)
        {
            DataType = typeof(Border)
        };

        Assert.True(managedDocument.Match(new Border()));
        Assert.False(managedDocument.Match(new Panel()));

        managedDocument.DataType = null;
        Assert.True(managedDocument.Match(new Panel()));
    }

    [AvaloniaFact]
    public void ManagedDockWindowDocument_Dispose_Detaches_Subscriptions()
    {
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.Factory = factory;

        var document = factory.CreateDocument();
        document.Title = "DocTitle";
        root.VisibleDockables = factory.CreateList<IDockable>(document);
        root.FocusedDockable = document;

        var window = new DockWindow
        {
            Factory = factory,
            Layout = root,
            Title = "WindowTitle"
        };
        root.Window = window;

        var managedDocument = new ManagedDockWindowDocument(window);
        managedDocument.Content = new Border();
        var initialTitle = managedDocument.Title;

        managedDocument.Dispose();

        window.Title = "UpdatedWindow";
        document.Title = "UpdatedDoc";

        Assert.Equal(initialTitle, managedDocument.Title);
        Assert.Null(managedDocument.Window);
        Assert.Null(managedDocument.Content);
    }

    [AvaloniaFact]
    public void ManagedDockWindowDocument_OnClose_Respects_ManagedHost_Cancellation()
    {
        var factory = new Factory();
        factory.WindowClosing += (_, e) => e.Cancel = true;

        var (host, window, _) = CreateManagedWindow(factory);
        var dock = ManagedWindowRegistry.GetOrCreateDock(factory);
        var managedDocument = dock.VisibleDockables!.OfType<ManagedDockWindowDocument>()
            .Single(document => ReferenceEquals(document.Window, window));

        var closed = managedDocument.OnClose();

        Assert.False(closed);
        Assert.True(host.IsTracked);
        Assert.Same(host, window.Host);
    }

    [AvaloniaFact]
    public void ManagedDockWindowDocument_OnClose_Uses_NonManaged_Host()
    {
        var host = new TestHostWindow();
        var window = new DockWindow
        {
            Host = host
        };
        host.Window = window;

        var managedDocument = new ManagedDockWindowDocument(window);
        var closed = managedDocument.OnClose();

        Assert.True(closed);
        Assert.True(host.ExitCalled);
        Assert.Null(window.Host);
    }

    [AvaloniaFact]
    public void ManagedDockWindowDocument_MdiBounds_Syncs_To_Host()
    {
        var factory = new Factory();
        var (host, window, _) = CreateManagedWindow(factory);

        var dock = ManagedWindowRegistry.GetOrCreateDock(factory);
        var managedDocument = dock.VisibleDockables!.OfType<ManagedDockWindowDocument>()
            .Single(document => ReferenceEquals(document.Window, window));

        managedDocument.MdiBounds = new DockRect(10, 20, 300, 200);

        host.GetPosition(out var x, out var y);
        host.GetSize(out var width, out var height);

        Assert.Equal(10, x);
        Assert.Equal(20, y);
        Assert.Equal(300, width);
        Assert.Equal(200, height);
        Assert.Equal(10, window.X);
        Assert.Equal(20, window.Y);
        Assert.Equal(300, window.Width);
        Assert.Equal(200, window.Height);
    }

    [AvaloniaFact]
    public void ManagedDockWindowDocument_MdiBounds_Syncs_To_NonManaged_Host()
    {
        var host = new TestHostWindow();
        var window = new DockWindow
        {
            Host = host
        };
        host.Window = window;

        var managedDocument = new ManagedDockWindowDocument(window);
        managedDocument.MdiBounds = new DockRect(12, 24, 320, 240);

        host.GetPosition(out var x, out var y);
        host.GetSize(out var width, out var height);

        Assert.Equal(12, x);
        Assert.Equal(24, y);
        Assert.Equal(320, width);
        Assert.Equal(240, height);
        Assert.Equal(12, window.X);
        Assert.Equal(24, window.Y);
        Assert.Equal(320, window.Width);
        Assert.Equal(240, window.Height);
    }

    [AvaloniaFact]
    public void ManagedWindowDock_AddRemove_Updates_Count_And_Active()
    {
        var factory = new Factory();
        var dock = new ManagedWindowDock { Factory = factory };
        var window1 = new DockWindow { Factory = factory };
        var window2 = new DockWindow { Factory = factory };
        var doc1 = new ManagedDockWindowDocument(window1);
        var doc2 = new ManagedDockWindowDocument(window2);

        dock.AddWindow(doc1);
        dock.AddWindow(doc2);

        Assert.Equal(2, dock.OpenedDockablesCount);
        Assert.Same(doc2, dock.ActiveDockable);
        Assert.Same(dock, doc1.Owner);
        Assert.Same(factory, doc1.Factory);
        Assert.Same(dock, doc2.Owner);
        Assert.Same(factory, doc2.Factory);

        dock.RemoveWindow(doc2);

        Assert.Equal(1, dock.OpenedDockablesCount);
        Assert.Same(doc1, dock.ActiveDockable);
    }

    [AvaloniaFact]
    public void ManagedWindowDock_FocusedDockable_Raises_Factory_Event()
    {
        var factory = new Factory();
        var dock = new ManagedWindowDock { Factory = factory };
        var dockable = new ManagedDockWindowDocument(new DockWindow { Factory = factory });

        var focused = new List<IDockable>();
        factory.FocusedDockableChanged += (_, e) =>
        {
            if (e.Dockable is { } value)
            {
                focused.Add(value);
            }
        };

        dock.FocusedDockable = dockable;

        Assert.Contains(dockable, focused);
    }

    [AvaloniaFact]
    public void ManagedWindowRegistry_Register_Unregister_Layer()
    {
        var factory = new Factory();
        var layer1 = new ManagedWindowLayer { IsVisible = true };
        var layer2 = new ManagedWindowLayer { IsVisible = true };

        ManagedWindowRegistry.RegisterLayer(factory, layer1);
        Assert.NotNull(layer1.Dock);
        Assert.Same(ManagedWindowRegistry.GetOrCreateDock(factory), layer1.Dock);

        ManagedWindowRegistry.RegisterLayer(factory, layer2);
        Assert.Null(layer1.Dock);
        Assert.False(layer1.IsVisible);
        Assert.Same(ManagedWindowRegistry.GetOrCreateDock(factory), layer2.Dock);

        ManagedWindowRegistry.UnregisterLayer(factory, layer2);
        Assert.Null(layer2.Dock);
        Assert.False(layer2.IsVisible);
    }

    [AvaloniaFact]
    public void ManagedWindowRegistry_TryGetLayer_Returns_Registered_Layer()
    {
        var factory = new Factory();
        Assert.Null(ManagedWindowRegistry.TryGetLayer(factory));

        var layer = new ManagedWindowLayer { IsVisible = true };
        ManagedWindowRegistry.RegisterLayer(factory, layer);

        Assert.Same(layer, ManagedWindowRegistry.TryGetLayer(factory));

        ManagedWindowRegistry.UnregisterLayer(factory, layer);
        Assert.Null(ManagedWindowRegistry.TryGetLayer(factory));
    }

    [AvaloniaFact]
    public void ManagedWindowLayer_Updates_ZOrder_For_Topmost_Active()
    {
        var dock = new ManagedWindowDock();
        var window1 = new DockWindow();
        var window2 = new DockWindow { Topmost = true };
        var window3 = new DockWindow { Topmost = true };
        var doc1 = new ManagedDockWindowDocument(window1);
        var doc2 = new ManagedDockWindowDocument(window2);
        var doc3 = new ManagedDockWindowDocument(window3);

        dock.VisibleDockables!.Add(doc1);
        dock.VisibleDockables!.Add(doc2);
        dock.VisibleDockables!.Add(doc3);
        dock.ActiveDockable = doc2;

        _ = new ManagedWindowLayer { Dock = dock };

        Assert.True(doc2.MdiZIndex > doc3.MdiZIndex);
        Assert.True(doc3.MdiZIndex > doc1.MdiZIndex);
    }

    [AvaloniaFact]
    public void ManagedWindowLayer_DockableCollection_Changes_Update_Subscriptions()
    {
        var dockables = new ObservableCollection<IDockable>();
        var dock = new ManagedWindowDock
        {
            VisibleDockables = dockables
        };

        var layer = new ManagedWindowLayer
        {
            Dock = dock
        };

        var doc1 = new ManagedDockWindowDocument(new DockWindow());
        var doc2 = new ManagedDockWindowDocument(new DockWindow());

        dockables.Add(doc1);
        var subscriptions = GetWindowSubscriptions(layer);

        Assert.Single(subscriptions);
        Assert.Contains(doc1, subscriptions.Keys);

        dockables.Add(doc2);

        Assert.Equal(2, subscriptions.Count);
        Assert.Contains(doc2, subscriptions.Keys);

        dockables.Remove(doc1);

        Assert.Single(subscriptions);
        Assert.DoesNotContain(doc1, subscriptions.Keys);

        var doc3 = new ManagedDockWindowDocument(new DockWindow());
        dockables[0] = doc3;

        Assert.Single(subscriptions);
        Assert.Contains(doc3, subscriptions.Keys);
        Assert.DoesNotContain(doc2, subscriptions.Keys);

        dockables.Clear();
        Assert.Empty(subscriptions);
    }

    [AvaloniaFact]
    public void ManagedWindowLayer_Show_Hide_Overlay()
    {
        var dock = new ManagedWindowDock();
        var layer = new ManagedWindowLayer
        {
            Dock = dock,
            Width = 300,
            Height = 200
        };

        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = layer
        };

        window.Show();
        try
        {
            layer.ApplyTemplate();
            window.UpdateLayout();

            var overlay = new Border();
            layer.ShowOverlay("TestOverlay", overlay, new Rect(10, 20, 120, 80), true);

            var canvas = layer.GetVisualDescendants()
                .OfType<Canvas>()
                .FirstOrDefault(candidate => candidate.Name == "PART_OverlayCanvas");
            Assert.NotNull(canvas);
            Assert.Contains(overlay, canvas!.Children);
            Assert.Equal(10, Canvas.GetLeft(overlay));
            Assert.Equal(20, Canvas.GetTop(overlay));
            Assert.Equal(120, overlay.Width);
            Assert.Equal(80, overlay.Height);

            layer.HideOverlay("TestOverlay");
            Assert.DoesNotContain(overlay, canvas.Children);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ManagedWindowLayer_TryGetLayer_Returns_From_DockControl()
    {
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.Factory = factory;

        var dockControl = new DockControl
        {
            Layout = root
        };

        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = dockControl
        };

        window.Show();
        try
        {
            dockControl.ApplyTemplate();
            window.UpdateLayout();

            var contentControl = dockControl.GetVisualDescendants()
                .OfType<Control>()
                .FirstOrDefault(control => control.Name == "PART_ContentControl");
            var layer = dockControl.GetVisualDescendants().OfType<ManagedWindowLayer>().FirstOrDefault();

            Assert.NotNull(contentControl);
            Assert.NotNull(layer);

            var resolved = ManagedWindowLayer.TryGetLayer(contentControl!);

            Assert.Same(layer, resolved);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ManagedWindowLayer_TryGetLayer_Returns_From_VisualRoot()
    {
        var layer = new ManagedWindowLayer
        {
            Width = 200,
            Height = 100
        };

        var window = new Window
        {
            Width = 300,
            Height = 200,
            Content = layer
        };

        window.Show();
        try
        {
            window.UpdateLayout();
            var resolved = ManagedWindowLayer.TryGetLayer(layer);

            Assert.Same(layer, resolved);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ManagedDockableConverters_Resolve_Tool_Window()
    {
        var factory = new Factory();
        var toolDock = factory.CreateToolDock();
        var documentDock = factory.CreateDocumentDock();

        var root = factory.CreateRootDock();
        root.Factory = factory;
        root.VisibleDockables = factory.CreateList<IDockable>(toolDock, documentDock);
        root.ActiveDockable = toolDock;

        var window = new DockWindow
        {
            Factory = factory,
            Layout = root
        };
        root.Window = window;

        var managedDocument = new ManagedDockWindowDocument(window);

        var isToolWindow = ManagedDockableIsToolWindowConverter.Instance.Convert(
            managedDocument,
            typeof(bool),
            null,
            CultureInfo.InvariantCulture);
        Assert.True((bool)isToolWindow);

        var invertedToolWindow = ManagedDockableIsToolWindowConverter.Instance.Convert(
            managedDocument,
            typeof(bool),
            true,
            CultureInfo.InvariantCulture);
        Assert.False((bool)invertedToolWindow);

        var invertedToolWindowText = ManagedDockableIsToolWindowConverter.Instance.Convert(
            managedDocument,
            typeof(bool),
            "Not",
            CultureInfo.InvariantCulture);
        Assert.False((bool)invertedToolWindowText);

        var resolvedToolDock = ManagedDockableToolDockConverter.Instance.Convert(
            managedDocument,
            typeof(IToolDock),
            null,
            CultureInfo.InvariantCulture);
        Assert.Same(toolDock, resolvedToolDock);

        root.ActiveDockable = documentDock;
        isToolWindow = ManagedDockableIsToolWindowConverter.Instance.Convert(
            managedDocument,
            typeof(bool),
            null,
            CultureInfo.InvariantCulture);
        Assert.False((bool)isToolWindow);

        resolvedToolDock = ManagedDockableToolDockConverter.Instance.Convert(
            managedDocument,
            typeof(IToolDock),
            null,
            CultureInfo.InvariantCulture);
        Assert.Null(resolvedToolDock);
    }

    private static Dictionary<ManagedDockWindowDocument, PropertyChangedEventHandler> GetWindowSubscriptions(
        ManagedWindowLayer layer)
    {
        var field = typeof(ManagedWindowLayer).GetField("_windowSubscriptions", BindingFlags.Instance | BindingFlags.NonPublic);
        return (Dictionary<ManagedDockWindowDocument, PropertyChangedEventHandler>)field!.GetValue(layer)!;
    }

    private sealed class TestHostWindow : IHostWindow
    {
        private double _x;
        private double _y;
        private double _width;
        private double _height;

        public bool ExitCalled { get; private set; }

        public IHostWindowState? HostWindowState => null;

        public bool IsTracked { get; set; }

        public IDockWindow? Window { get; set; }

        public void Present(bool isDialog)
        {
        }

        public void Exit()
        {
            ExitCalled = true;
        }

        public void SetPosition(double x, double y)
        {
            _x = x;
            _y = y;
        }

        public void GetPosition(out double x, out double y)
        {
            x = _x;
            y = _y;
        }

        public void SetSize(double width, double height)
        {
            _width = width;
            _height = height;
        }

        public void GetSize(out double width, out double height)
        {
            width = _width;
            height = _height;
        }

        public void SetTitle(string? title)
        {
        }

        public void SetLayout(IDock layout)
        {
        }

        public void SetActive()
        {
        }
    }

    private sealed class TestDockWindow : DockWindow
    {
        public override bool OnClose() => true;
    }

    private sealed class TestDockManagerState : DockManagerState
    {
        public TestDockManagerState(IDockManager dockManager)
            : base(dockManager)
        {
        }

        public void InvokeFloat(Point point, DockControl dockControl, IDockable dockable, IFactory factory, PixelPoint dragOffset)
        {
            Float(point, dockControl, dockable, factory, dragOffset);
        }
    }
}
