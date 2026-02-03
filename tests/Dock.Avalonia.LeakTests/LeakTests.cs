using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Controls.Overlays;
using Dock.Avalonia.Selectors;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Model.Services;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class LeakTests
{
    private sealed class BindingViewModel : INotifyPropertyChanged
    {
        private bool _isGlobalDockActive;

        public bool IsGlobalDockActive
        {
            get => _isGlobalDockActive;
            set
            {
                if (_isGlobalDockActive == value)
                {
                    return;
                }

                _isGlobalDockActive = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsGlobalDockActive)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private sealed class NoOpDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }

    private sealed class NoOpCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    private sealed class StubBusyService : IDockBusyService
    {
        private readonly NoOpDisposable _scope = new();
        private Func<Task>? _reloadHandler;

        public bool IsBusy { get; private set; }

        public string? Message { get; private set; }

        public bool IsReloadVisible { get; set; }

        public bool CanReload { get; private set; }

        public ICommand ReloadCommand { get; } = new NoOpCommand();

        public event PropertyChangedEventHandler? PropertyChanged;

        public IDisposable Begin(string message)
        {
            IsBusy = true;
            Message = message;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
            return _scope;
        }

        public Task RunAsync(string message, Func<Task> action)
        {
            IsBusy = true;
            Message = message;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
            return action();
        }

        public void UpdateMessage(string? message)
        {
            Message = message;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
        }

        public void SetReloadHandler(Func<Task>? handler)
        {
            _reloadHandler = handler;
            CanReload = handler is not null;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanReload)));
        }
    }

    private sealed class StubGlobalBusyService : IDockGlobalBusyService
    {
        public bool IsBusy { get; private set; }

        public string? Message { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public IDisposable Begin(string? message = null)
        {
            IsBusy = true;
            Message = message;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
            return new NoOpDisposable();
        }
    }

    private sealed class StubDialogService : IDockDialogService
    {
        private readonly ObservableCollection<DialogRequest> _dialogs = new();
        private readonly ReadOnlyObservableCollection<DialogRequest> _readonlyDialogs;

        public StubDialogService()
        {
            _readonlyDialogs = new ReadOnlyObservableCollection<DialogRequest>(_dialogs);
        }

        public ReadOnlyObservableCollection<DialogRequest> Dialogs => _readonlyDialogs;

        public DialogRequest? ActiveDialog => null;

        public bool HasDialogs => false;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Task<T?> ShowAsync<T>(object content, string? title = null) => Task.FromResult<T?>(default);

        public void Close(DialogRequest request, object? result = null)
        {
        }

        public void CancelAll()
        {
        }
    }

    private sealed class StubGlobalDialogService : IDockGlobalDialogService
    {
        public bool IsDialogOpen { get; private set; }

        public string? Message { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public IDisposable Begin(string? message = null)
        {
            IsDialogOpen = true;
            Message = message;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDialogOpen)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
            return new NoOpDisposable();
        }
    }

    private sealed class StubConfirmationService : IDockConfirmationService
    {
        private readonly ObservableCollection<ConfirmationRequest> _confirmations = new();
        private readonly ReadOnlyObservableCollection<ConfirmationRequest> _readonlyConfirmations;

        public StubConfirmationService()
        {
            _readonlyConfirmations = new ReadOnlyObservableCollection<ConfirmationRequest>(_confirmations);
        }

        public ReadOnlyObservableCollection<ConfirmationRequest> Confirmations => _readonlyConfirmations;

        public ConfirmationRequest? ActiveConfirmation => null;

        public bool HasConfirmations => false;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Task<bool> ConfirmAsync(string title, string message, string confirmText = "Confirm", string cancelText = "Cancel")
            => Task.FromResult(false);

        public void Close(ConfirmationRequest request, bool result)
        {
        }

        public void CancelAll()
        {
        }
    }

    private sealed class StubGlobalConfirmationService : IDockGlobalConfirmationService
    {
        public bool IsConfirmationOpen { get; private set; }

        public string? Message { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public IDisposable Begin(string? message = null)
        {
            IsConfirmationOpen = true;
            Message = message;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConfirmationOpen)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
            return new NoOpDisposable();
        }
    }

    private sealed class LeakContext
    {
        private LeakContext(
            Factory factory,
            RootDock root,
            ToolDock toolDock,
            DocumentDock documentDock,
            Tool tool,
            Document document,
            StackDock stackDock,
            ProportionalDock proportionalDock,
            GridDock gridDock,
            UniformGridDock uniformGridDock,
            WrapDock wrapDock,
            DockDock dockDock,
            SplitViewDock splitViewDock,
            ManagedWindowDock managedWindowDock)
        {
            Factory = factory;
            Root = root;
            ToolDock = toolDock;
            DocumentDock = documentDock;
            Tool = tool;
            Document = document;
            StackDock = stackDock;
            ProportionalDock = proportionalDock;
            GridDock = gridDock;
            UniformGridDock = uniformGridDock;
            WrapDock = wrapDock;
            DockDock = dockDock;
            SplitViewDock = splitViewDock;
            ManagedWindowDock = managedWindowDock;
        }

        public Factory Factory { get; }

        public RootDock Root { get; }

        public ToolDock ToolDock { get; }

        public DocumentDock DocumentDock { get; }

        public Tool Tool { get; }

        public Document Document { get; }

        public StackDock StackDock { get; }

        public ProportionalDock ProportionalDock { get; }

        public GridDock GridDock { get; }

        public UniformGridDock UniformGridDock { get; }

        public WrapDock WrapDock { get; }

        public DockDock DockDock { get; }

        public SplitViewDock SplitViewDock { get; }

        public ManagedWindowDock ManagedWindowDock { get; }

        public static LeakContext Create()
        {
            var factory = new Factory();

            var root = (RootDock)factory.CreateRootDock();
            root.Factory = factory;
            root.VisibleDockables = factory.CreateList<IDockable>();

            var tool = (Tool)factory.CreateTool();
            tool.Factory = factory;

            var document = (Document)factory.CreateDocument();
            document.Factory = factory;

            var toolDock = (ToolDock)factory.CreateToolDock();
            toolDock.Factory = factory;
            toolDock.VisibleDockables = factory.CreateList<IDockable>(tool);
            toolDock.ActiveDockable = tool;
            tool.Owner = toolDock;

            var documentDock = (DocumentDock)factory.CreateDocumentDock();
            documentDock.Factory = factory;
            documentDock.VisibleDockables = factory.CreateList<IDockable>(document);
            documentDock.ActiveDockable = document;
            document.Owner = documentDock;

            var stackDock = (StackDock)factory.CreateStackDock();
            stackDock.Factory = factory;
            stackDock.VisibleDockables = factory.CreateList<IDockable>();

            var proportionalDock = (ProportionalDock)factory.CreateProportionalDock();
            proportionalDock.Factory = factory;
            proportionalDock.VisibleDockables = factory.CreateList<IDockable>();

            var gridDock = (GridDock)factory.CreateGridDock();
            gridDock.Factory = factory;
            gridDock.VisibleDockables = factory.CreateList<IDockable>();

            var uniformGridDock = (UniformGridDock)factory.CreateUniformGridDock();
            uniformGridDock.Factory = factory;
            uniformGridDock.VisibleDockables = factory.CreateList<IDockable>();

            var wrapDock = (WrapDock)factory.CreateWrapDock();
            wrapDock.Factory = factory;
            wrapDock.VisibleDockables = factory.CreateList<IDockable>();

            var dockDock = (DockDock)factory.CreateDockDock();
            dockDock.Factory = factory;
            dockDock.VisibleDockables = factory.CreateList<IDockable>();

            var splitViewDock = (SplitViewDock)factory.CreateSplitViewDock();
            splitViewDock.Factory = factory;
            splitViewDock.VisibleDockables = factory.CreateList<IDockable>();

            root.VisibleDockables.Add(toolDock);
            root.VisibleDockables.Add(documentDock);
            root.VisibleDockables.Add(stackDock);
            root.VisibleDockables.Add(proportionalDock);
            root.VisibleDockables.Add(gridDock);
            root.VisibleDockables.Add(uniformGridDock);
            root.VisibleDockables.Add(wrapDock);
            root.VisibleDockables.Add(dockDock);
            root.VisibleDockables.Add(splitViewDock);

            root.PinnedDock = toolDock;
            root.LeftPinnedDockables?.Add(tool);

            var managedWindowDock = new ManagedWindowDock
            {
                Factory = factory
            };

            return new LeakContext(
                factory,
                root,
                toolDock,
                documentDock,
                tool,
                document,
                stackDock,
                proportionalDock,
                gridDock,
                uniformGridDock,
                wrapDock,
                dockDock,
                splitViewDock,
                managedWindowDock);
        }
    }

    private sealed record BindingLeakResult(WeakReference ViewModelRef, DockTarget Target);

    private sealed record ControlSetup(Control Control, object?[] KeepAlive, Action<Control>? BeforeCleanup = null);

    private sealed record WindowSetup(Window Window, object?[] KeepAlive);

    private sealed record ControlLeakCase(string Name, Func<LeakContext, ControlSetup> Create);

    private sealed record WindowLeakCase(string Name, Func<LeakContext, WindowSetup> Create);

    private sealed record LeakResult(string Name, WeakReference[] References, object?[] KeepAlive);

    private static readonly ControlLeakCase[] ControlCases =
    [
        new ControlLeakCase("DockControl", context =>
        {
            var control = new DockControl { Factory = context.Factory, Layout = context.Root };
            return new ControlSetup(control, new object?[] { context.Factory });
        }),
        new ControlLeakCase("DockCommandBarHost", _ =>
            new ControlSetup(new DockCommandBarHost(), Array.Empty<object?>())),
        new ControlLeakCase("DockDockControl", context =>
            new ControlSetup(new DockDockControl { DataContext = context.DockDock }, Array.Empty<object?>())),
        new ControlLeakCase("DockSelectorOverlay", context =>
        {
            var item = new DockSelectorItem(context.Document, 1, true, false, false, false, false);
            return new ControlSetup(new DockSelectorOverlay { Items = new[] { item } }, Array.Empty<object?>());
        }),
        new ControlLeakCase("DockTarget", _ =>
            new ControlSetup(new DockTarget(), Array.Empty<object?>())),
        new ControlLeakCase("GlobalDockTarget", _ =>
            new ControlSetup(new GlobalDockTarget(), Array.Empty<object?>())),
        new ControlLeakCase("DockableControl", context =>
        {
            var control = new DockableControl
            {
                TrackingMode = TrackingMode.Visible,
                DataContext = context.Document
            };
            return new ControlSetup(control, new object?[] { context.Factory });
        }),
        new ControlLeakCase("DocumentContentControl", context =>
            new ControlSetup(new DocumentContentControl { DataContext = context.Document }, new object?[] { context.Factory })),
        new ControlLeakCase("ToolContentControl", context =>
            new ControlSetup(new ToolContentControl { DataContext = context.Tool }, new object?[] { context.Factory })),
        new ControlLeakCase("DocumentControl", context =>
            new ControlSetup(new DocumentControl { DataContext = context.DocumentDock }, Array.Empty<object?>())),
        new ControlLeakCase("ToolControl", context =>
            new ControlSetup(new ToolControl { DataContext = context.ToolDock }, Array.Empty<object?>())),
        new ControlLeakCase("ToolChromeControl", context =>
            new ControlSetup(new ToolChromeControl { DataContext = context.ToolDock }, Array.Empty<object?>())),
        new ControlLeakCase("DocumentDockControl", context =>
            new ControlSetup(new DocumentDockControl { DataContext = context.DocumentDock }, Array.Empty<object?>())),
        new ControlLeakCase("ToolDockControl", context =>
            new ControlSetup(new ToolDockControl { DataContext = context.ToolDock }, Array.Empty<object?>())),
        new ControlLeakCase("GridDockControl", context =>
            new ControlSetup(new GridDockControl { DataContext = context.GridDock }, Array.Empty<object?>())),
        new ControlLeakCase("ProportionalDockControl", context =>
            new ControlSetup(new ProportionalDockControl { DataContext = context.ProportionalDock }, Array.Empty<object?>())),
        new ControlLeakCase("StackDockControl", context =>
            new ControlSetup(new StackDockControl { DataContext = context.StackDock }, Array.Empty<object?>())),
        new ControlLeakCase("WrapDockControl", context =>
            new ControlSetup(new WrapDockControl { DataContext = context.WrapDock }, Array.Empty<object?>())),
        new ControlLeakCase("UniformGridDockControl", context =>
            new ControlSetup(new UniformGridDockControl { DataContext = context.UniformGridDock }, Array.Empty<object?>())),
        new ControlLeakCase("SplitViewDockControl", context =>
            new ControlSetup(new SplitViewDockControl { DataContext = context.SplitViewDock }, Array.Empty<object?>())),
        new ControlLeakCase("RootDockControl", context =>
            new ControlSetup(new RootDockControl { DataContext = context.Root }, Array.Empty<object?>())),
        new ControlLeakCase("PinnedDockControl", context =>
            new ControlSetup(new PinnedDockControl { DataContext = context.Root }, new object?[] { context.Root })),
        new ControlLeakCase("PinnedDockHostPanel", context =>
            new ControlSetup(new PinnedDockHostPanel { DataContext = context.Root }, new object?[] { context.Root })),
        new ControlLeakCase("DocumentTabStrip", context =>
            new ControlSetup(new DocumentTabStrip { ItemsSource = new[] { context.Document } }, Array.Empty<object?>())),
        new ControlLeakCase("ToolTabStrip", context =>
            new ControlSetup(new ToolTabStrip { ItemsSource = new[] { context.Tool } }, Array.Empty<object?>())),
        new ControlLeakCase("DocumentTabStripItem", context =>
            new ControlSetup(new DocumentTabStripItem { DataContext = context.Document }, Array.Empty<object?>())),
        new ControlLeakCase("ToolTabStripItem", context =>
            new ControlSetup(new ToolTabStripItem { DataContext = context.Tool }, Array.Empty<object?>())),
        new ControlLeakCase("ToolPinnedControl", context =>
            new ControlSetup(new ToolPinnedControl { ItemsSource = new[] { context.Tool } }, Array.Empty<object?>())),
        new ControlLeakCase("ToolPinItemControl", _ =>
            new ControlSetup(new ToolPinItemControl(), Array.Empty<object?>())),
        new ControlLeakCase("DragPreviewControl", _ =>
            new ControlSetup(new DragPreviewControl { PreviewContent = new Border() }, Array.Empty<object?>())),
        new ControlLeakCase("HostWindowTitleBar", _ =>
            new ControlSetup(new HostWindowTitleBar(), Array.Empty<object?>())),
        new ControlLeakCase("MdiLayoutPanel", _ =>
            new ControlSetup(new MdiLayoutPanel(), Array.Empty<object?>())),
        new ControlLeakCase("MdiDocumentControl", context =>
            new ControlSetup(new MdiDocumentControl { DataContext = context.DocumentDock }, new object?[] { context.DocumentDock })),
        new ControlLeakCase("MdiDocumentWindow", context =>
            new ControlSetup(new MdiDocumentWindow { DataContext = context.Document }, new object?[] { context.Document, context.DocumentDock })),
        new ControlLeakCase("ManagedWindowLayer", context =>
            new ControlSetup(
                new ManagedWindowLayer { Dock = context.ManagedWindowDock },
                new object?[] { context.ManagedWindowDock },
                control => ((ManagedWindowLayer)control).Dock = null)),
        new ControlLeakCase("BusyOverlayControl", _ =>
        {
            var busy = new StubBusyService();
            var global = new StubGlobalBusyService();
            var control = new BusyOverlayControl { BusyService = busy, GlobalBusyService = global };
            return new ControlSetup(control, new object?[] { busy, global });
        }),
        new ControlLeakCase("DialogOverlayControl", _ =>
        {
            var dialog = new StubDialogService();
            var global = new StubGlobalDialogService();
            var control = new DialogOverlayControl { DialogService = dialog, GlobalDialogService = global };
            return new ControlSetup(control, new object?[] { dialog, global });
        }),
        new ControlLeakCase("ConfirmationOverlayControl", _ =>
        {
            var confirmation = new StubConfirmationService();
            var global = new StubGlobalConfirmationService();
            var control = new ConfirmationOverlayControl { ConfirmationService = confirmation, GlobalConfirmationService = global };
            return new ControlSetup(control, new object?[] { confirmation, global });
        }),
        new ControlLeakCase("ConfirmationDialogControl", _ =>
            new ControlSetup(new ConfirmationDialogControl(), Array.Empty<object?>())),
        new ControlLeakCase("DialogShellControl", _ =>
            new ControlSetup(new DialogShellControl(), Array.Empty<object?>())),
        new ControlLeakCase("OverlayHost", _ =>
            new ControlSetup(new OverlayHost { Content = new Border() }, Array.Empty<object?>()))
    ];

    private static readonly WindowLeakCase[] WindowCases =
    [
        new WindowLeakCase("DockAdornerWindow", _ =>
            new WindowSetup(new DockAdornerWindow { Content = new Border() }, Array.Empty<object?>())),
        new WindowLeakCase("DragPreviewWindow", _ =>
            new WindowSetup(new DragPreviewWindow { Content = new Border() }, Array.Empty<object?>())),
        new WindowLeakCase("PinnedDockWindow", _ =>
            new WindowSetup(new PinnedDockWindow { Content = new Border() }, Array.Empty<object?>())),
        new WindowLeakCase("HostWindow", _ =>
            new WindowSetup(new HostWindow { Content = new Border() }, Array.Empty<object?>()))
    ];

    [ReleaseFact]
    public void DockControl_AttachDetach_DoesNotLeak()
    {
        var factory = new Factory();

        var (controlRef, layoutRef) = RunInSession(() =>
        {
            var layout = factory.CreateLayout();
            layout.Factory = factory;

            var dockControl = new DockControl
            {
                Factory = factory,
                Layout = layout
            };

            var window = new Window { Content = dockControl };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);

            var result = (new WeakReference(dockControl), new WeakReference(layout));
            CleanupWindow(window);
            return result;
        });

        AssertCollected(controlRef, layoutRef);
        GC.KeepAlive(factory);
    }

    [ReleaseFact]
    public void DockableControl_Detach_DoesNotLeak_Dockable()
    {
        var factory = new Factory();

        var (controlRef, dockableRef) = RunInSession(() =>
        {
            var dockable = factory.CreateDocument();
            dockable.Factory = factory;

            var control = new DockableControl
            {
                TrackingMode = TrackingMode.Visible,
                DataContext = dockable
            };

            var window = new Window { Content = control };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);

            var result = (new WeakReference(control), new WeakReference(dockable));
            CleanupWindow(window);
            return result;
        });

        AssertCollected(controlRef, dockableRef);
        GC.KeepAlive(factory);
    }

    [ReleaseFact]
    public void OverlayHost_Detach_DoesNotLeak_StaticRegistry()
    {
        var hostRef = RunInSession(() =>
        {
            var host = new OverlayHost
            {
                Content = new Border()
            };

            var window = new Window { Content = host };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);

            var reference = new WeakReference(host);
            CleanupWindow(window);
            return reference;
        });

        AssertCollected(hostRef);
    }

    [ReleaseFact]
    public void Binding_Clears_DataContext_DoesNotLeak_ViewModel()
    {
        var result = RunInSession(() =>
        {
            var viewModel = new BindingViewModel { IsGlobalDockActive = true };

            var target = new DockTarget();
            target.Bind(DockTargetBase.IsGlobalDockActiveProperty,
                new Binding(nameof(BindingViewModel.IsGlobalDockActive)) { Mode = BindingMode.TwoWay });
            target.DataContext = viewModel;

            var window = new Window { Content = target };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);

            target.DataContext = null;
            DrainDispatcher();

            var result = new BindingLeakResult(new WeakReference(viewModel), target);
            CleanupWindow(window);
            return result;
        });

        AssertCollected(result.ViewModelRef);
        GC.KeepAlive(result.Target);
    }

    [ReleaseFact]
    public void AllControls_AttachDetach_DoesNotLeak()
    {
        foreach (var testCase in ControlCases)
        {
            var result = RunControlCase(testCase);
            AssertCollectedForCase(result);
        }
    }

    [ReleaseFact]
    public void AllWindows_AttachDetach_DoesNotLeak()
    {
        foreach (var testCase in WindowCases)
        {
            var result = RunWindowCase(testCase);
            AssertCollectedForCase(result);
        }
    }

    private static LeakResult RunControlCase(ControlLeakCase testCase)
    {
        return RunInSession(() =>
        {
            var context = LeakContext.Create();
            var setup = testCase.Create(context);

            var window = new Window { Content = setup.Control };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);

            setup.BeforeCleanup?.Invoke(setup.Control);

            var result = new LeakResult(
                testCase.Name,
                new[] { new WeakReference(setup.Control) },
                setup.KeepAlive);

            CleanupWindow(window);
            return result;
        });
    }

    private static LeakResult RunWindowCase(WindowLeakCase testCase)
    {
        return RunInSession(() =>
        {
            var context = LeakContext.Create();
            var setup = testCase.Create(context);
            setup.Window.Styles.Add(new FluentTheme());

            ShowWindow(setup.Window);

            var result = new LeakResult(
                testCase.Name,
                new[] { new WeakReference(setup.Window) },
                setup.KeepAlive);

            CleanupWindow(setup.Window);
            return result;
        });
    }

    private static void AssertCollectedForCase(LeakResult result)
    {
        var references = result.References;

        for (var attempt = 0; attempt < 20 && AnyAlive(references); attempt++)
        {
            CollectGarbage();
            Thread.Sleep(10);
        }

        foreach (var reference in references)
        {
            Assert.False(reference.IsAlive, $"{result.Name} leaked");
        }

        foreach (var keepAlive in result.KeepAlive)
        {
            GC.KeepAlive(keepAlive);
        }
    }

    private static bool AnyAlive(IReadOnlyList<WeakReference> references)
    {
        for (var i = 0; i < references.Count; i++)
        {
            if (references[i].IsAlive)
            {
                return true;
            }
        }

        return false;
    }
}
