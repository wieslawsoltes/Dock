using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.CommandBars;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Controls.Overlays;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model.CommandBars;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using Dock.Model.Services;
using Dock.Settings;
using ModelDirection = Dock.Model.Core.GridResizeDirection;
using ControlDirection = Avalonia.Controls.GridResizeDirection;

namespace DockSystemTextJsonSourceGenAotSample;

// Shared with the xUnit headless suite so the same behavioral checks run after trimming and Native AOT.
internal static class BindingSmokeChecks
{
    public static void ValidateProportionalSplitter(IProportionalDockSplitter first, IProportionalDockSplitter replacement)
    {
        first.CanResize = false;
        first.ResizePreview = true;
        var control = BuildSplitter<ProportionalStackPanelSplitter>(first);
        control.DataContext = first;
        Ensure(!control.IsResizingEnabled && control.PreviewResize, "Initial splitter values were not bound.");

        first.CanResize = true;
        first.ResizePreview = false;
        Ensure(control.IsResizingEnabled && !control.PreviewResize, "Splitter property changes were not observed.");

        replacement.CanResize = false;
        replacement.ResizePreview = true;
        control.DataContext = replacement;
        Ensure(!control.IsResizingEnabled && control.PreviewResize, "Replacement splitter context was not bound.");
        first.CanResize = false;
        first.ResizePreview = true;
        first.CanResize = true;
        first.ResizePreview = false;
        Ensure(!control.IsResizingEnabled && control.PreviewResize, "Old splitter context still updates the control.");
        replacement.CanResize = true;
        replacement.ResizePreview = false;
        Ensure(control.IsResizingEnabled && !control.PreviewResize, "Replacement splitter changes were not observed.");
        control.DataContext = null;
    }

    public static void ValidateGridSplitter(IGridDockSplitter first, IGridDockSplitter replacement)
    {
        first.ResizeDirection = ModelDirection.Rows;
        var control = BuildSplitter<GridSplitter>(first);
        control.DataContext = first;
        Ensure(control.ResizeDirection == ControlDirection.Rows, "Rows was not mapped to Avalonia's enum.");
        first.ResizeDirection = ModelDirection.Columns;
        Ensure(control.ResizeDirection == ControlDirection.Columns, "Columns changes were not mapped.");
        first.ResizeDirection = (ModelDirection)99;
        Ensure(control.ResizeDirection == ControlDirection.Auto, "Unknown direction did not fall back to Auto.");

        replacement.ResizeDirection = ModelDirection.Rows;
        control.DataContext = replacement;
        Ensure(control.ResizeDirection == ControlDirection.Rows, "Replacement grid context was not bound.");
        first.ResizeDirection = ModelDirection.Columns;
        Ensure(control.ResizeDirection == ControlDirection.Rows, "Old grid context still updates the control.");
        replacement.ResizeDirection = ModelDirection.Columns;
        Ensure(control.ResizeDirection == ControlDirection.Columns, "Replacement grid changes were not observed.");
        control.DataContext = null;
    }

    public static void ValidateMdiCommands()
    {
        Ensure(Application.Current!.TryFindResource("MdiDocumentWindowContextMenu", out var resource), "MDI menu was not found.");
        var menu = (ContextMenu)resource!;
        var first = new DocumentDock { LayoutMode = DocumentLayoutMode.Mdi };
        var document = new Document { Owner = first, MdiState = MdiWindowState.Minimized };
        first.VisibleDockables = new List<IDockable> { document };
        menu.DataContext = document;
        VerifyCommands(menu, first);
        FindMenuItem(menu, "Restore").Command!.Execute(null);
        Ensure(document.MdiState == MdiWindowState.Normal, "Restore menu command did not restore the document.");

        var replacement = new DocumentDock { LayoutMode = DocumentLayoutMode.Mdi };
        document.Owner = replacement;
        VerifyCommands(menu, replacement);
        menu.DataContext = new Document { Owner = first };
        VerifyCommands(menu, first);
        menu.DataContext = null;
        Ensure(FindMenuItem(menu, "Restore").Command is null, "Clearing the MDI context retained a command.");
    }

    public static void ValidateOverlayServices()
    {
        var first = new OverlayServices();
        var replacement = new OverlayServices();
        var host = new OverlayHost { DataContext = first, Content = new TextBlock { Text = "Content" }, UseServiceLayers = false };
        var window = new Window { Content = host, Width = 400, Height = 300 };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            VerifyOverlays(host, first);
            host.DataContext = replacement;
            VerifyOverlays(host, replacement);
            host.DataContext = new RootDock();
            VerifyOverlays(host, null);
            host.DataContext = null;
            VerifyOverlays(host, null);
            host.DataContext = first;
            VerifyOverlays(host, first);
        }
        finally
        {
            window.Close();
        }
    }

    public static void ValidateCommandBarKinds()
    {
        bool previous = DockSettings.CommandBarMergingEnabled;
        DockSettings.CommandBarMergingEnabled = true;
        var menu = new Border();
        var toolBar = new Border();
        var ribbon = new Border();
        var host = new DockCommandBarHost
        {
            BaseCommandBars = new[]
            {
                new DockCommandBarDefinition("menu", DockCommandBarKind.Menu) { Content = menu },
                new DockCommandBarDefinition("tools", DockCommandBarKind.ToolBar) { Content = toolBar },
                new DockCommandBarDefinition("ribbon", DockCommandBarKind.Ribbon) { Content = ribbon }
            }
        };
        var manager = new DockCommandBarManager(host);
        try
        {
            manager.Attach(new RootDock { Factory = new Factory() });
            Ensure(host.MenuBars?.Single() == menu, "Menu kind was lost during merging.");
            Ensure(host.ToolBars?.Single() == toolBar, "Toolbar kind was lost during merging.");
            Ensure(host.RibbonBars?.Single() == ribbon, "Ribbon kind was lost during merging.");
        }
        finally
        {
            manager.Detach();
            DockSettings.CommandBarMergingEnabled = previous;
        }
    }

    private static T BuildSplitter<T>(object model) where T : Control
    {
        var dock = new DockControl();
        var window = new Window { Content = dock, Width = 400, Height = 300 };
        window.Show();
        try
        {
            dock.ApplyTemplate();
            var content = dock.GetVisualDescendants().OfType<Control>()
                .First(control => control.Name == "PART_ContentControl");
            return (T)content.DataTemplates.First(template => template.Match(model)).Build(model)!;
        }
        finally
        {
            window.Close();
        }
    }

    private static MenuItem FindMenuItem(ContextMenu menu, string header) =>
        menu.Items.OfType<MenuItem>().Single(item => Equals(item.Header, header));

    private static void VerifyCommands(ContextMenu menu, IDocumentDock owner)
    {
        Ensure(ReferenceEquals(FindMenuItem(menu, "Cascade").Command, owner.CascadeDocuments), "Cascade command binding failed.");
        Ensure(ReferenceEquals(FindMenuItem(menu, "Tile Horizontal").Command, owner.TileDocumentsHorizontal), "Horizontal tile command binding failed.");
        Ensure(ReferenceEquals(FindMenuItem(menu, "Tile Vertical").Command, owner.TileDocumentsVertical), "Vertical tile command binding failed.");
        Ensure(ReferenceEquals(FindMenuItem(menu, "Restore").Command, owner.RestoreDocuments), "Restore command binding failed.");
    }

    private static void VerifyOverlays(OverlayHost host, IHostOverlayServices? expected)
    {
        Ensure(host.OverlayLayers?.Count == 3, "Default overlay layers were not materialized.");
        var dialog = host.GetVisualDescendants().OfType<DialogOverlayControl>().Single();
        var confirmation = host.GetVisualDescendants().OfType<ConfirmationOverlayControl>().Single();
        var busy = host.GetVisualDescendants().OfType<BusyOverlayControl>().Single();
        Ensure(ReferenceEquals(dialog.DialogService, expected?.Dialogs), $"Host dialog service binding failed. Context: {dialog.DataContext?.GetType().Name}, parent: {dialog.Parent?.GetType().Name}, expected: {expected?.GetType().Name}.");
        Ensure(ReferenceEquals(dialog.GlobalDialogService, expected?.GlobalDialogService), "Global dialog service binding failed.");
        Ensure(ReferenceEquals(confirmation.ConfirmationService, expected?.Confirmations), "Host confirmation service binding failed.");
        Ensure(ReferenceEquals(confirmation.GlobalConfirmationService, expected?.GlobalConfirmationService), "Global confirmation service binding failed.");
        Ensure(ReferenceEquals(busy.BusyService, expected?.Busy), "Host busy service binding failed.");
        Ensure(ReferenceEquals(busy.GlobalBusyService, expected?.GlobalBusyService), "Global busy service binding failed.");
    }

    private static void Ensure(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    // Explicit interface properties must work; runtime property-name lookup cannot resolve them.
    private sealed class OverlayServices : IHostOverlayServices
    {
        private readonly OverlayServiceStub _services = new();
        IDockBusyService IHostOverlayServices.Busy => _services;
        IDockDialogService IHostOverlayServices.Dialogs => _services;
        IDockConfirmationService IHostOverlayServices.Confirmations => _services;
        IDockGlobalBusyService IHostOverlayServices.GlobalBusyService => _services;
        IDockGlobalDialogService IHostOverlayServices.GlobalDialogService => _services;
        IDockGlobalConfirmationService IHostOverlayServices.GlobalConfirmationService => _services;
    }

    private sealed class OverlayServiceStub : IDockBusyService, IDockDialogService, IDockConfirmationService,
        IDockGlobalBusyService, IDockGlobalDialogService, IDockGlobalConfirmationService
    {
        public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }
        public bool IsBusy => false;
        public string? Message => null;
        public bool IsReloadVisible { get; set; }
        public bool CanReload => false;
        public ICommand ReloadCommand => null!;
        public bool IsDialogOpen => false;
        public bool IsConfirmationOpen => false;
        public ReadOnlyObservableCollection<DialogRequest> Dialogs { get; } = new(new());
        public DialogRequest? ActiveDialog => null;
        public bool HasDialogs => false;
        public ReadOnlyObservableCollection<ConfirmationRequest> Confirmations { get; } = new(new());
        public ConfirmationRequest? ActiveConfirmation => null;
        public bool HasConfirmations => false;
        public IDisposable Begin(string? message = null) => throw new NotSupportedException();
        public Task RunAsync(string message, Func<Task> action) => throw new NotSupportedException();
        public void UpdateMessage(string? message) => throw new NotSupportedException();
        public void SetReloadHandler(Func<Task>? handler) => throw new NotSupportedException();
        public Task<T?> ShowAsync<T>(object content, string? title = null) => throw new NotSupportedException();
        public void Close(DialogRequest request, object? result = null) => throw new NotSupportedException();
        public Task<bool> ConfirmAsync(string title, string message, string confirmText = "Confirm", string cancelText = "Cancel") => throw new NotSupportedException();
        public void Close(ConfirmationRequest request, bool result) => throw new NotSupportedException();
        public void CancelAll() => throw new NotSupportedException();
    }
}
