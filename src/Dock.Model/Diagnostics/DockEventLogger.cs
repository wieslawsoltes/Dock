using System;
using Dock.Model.Core.Events;
using Microsoft.Extensions.Logging;

namespace Dock.Model.Diagnostics;

/// <summary>
/// Logs <see cref="FactoryBase"/> events using <see cref="ILogger"/>.
/// </summary>
public sealed class DockEventLogger : IDisposable
{
    private readonly FactoryBase _factory;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockEventLogger"/> class.
    /// </summary>
    /// <param name="factory">Factory emitting events.</param>
    /// <param name="logger">Logger implementation.</param>
    public DockEventLogger(FactoryBase factory, ILogger<DockEventLogger> logger)
    {
        _factory = factory;
        _logger = logger;
        Subscribe();
    }

    private void Subscribe()
    {
        _factory.ActiveDockableChanged += OnActiveDockableChanged;
        _factory.FocusedDockableChanged += OnFocusedDockableChanged;
        _factory.DockableInit += OnDockableInit;
        _factory.DockableAdded += OnDockableAdded;
        _factory.DockableRemoved += OnDockableRemoved;
        _factory.DockableClosed += OnDockableClosed;
        _factory.DockableMoved += OnDockableMoved;
        _factory.DockableDocked += OnDockableDocked;
        _factory.DockableUndocked += OnDockableUndocked;
        _factory.DockableSwapped += OnDockableSwapped;
        _factory.DockablePinned += OnDockablePinned;
        _factory.DockableUnpinned += OnDockableUnpinned;
        _factory.DockableHidden += OnDockableHidden;
        _factory.DockableRestored += OnDockableRestored;
        _factory.WindowOpened += OnWindowOpened;
        _factory.WindowClosing += OnWindowClosing;
        _factory.WindowClosed += OnWindowClosed;
        _factory.WindowAdded += OnWindowAdded;
        _factory.WindowRemoved += OnWindowRemoved;
        _factory.WindowMoveDragBegin += OnWindowMoveDragBegin;
        _factory.WindowMoveDrag += OnWindowMoveDrag;
        _factory.WindowMoveDragEnd += OnWindowMoveDragEnd;
    }

    private void Unsubscribe()
    {
        _factory.ActiveDockableChanged -= OnActiveDockableChanged;
        _factory.FocusedDockableChanged -= OnFocusedDockableChanged;
        _factory.DockableInit -= OnDockableInit;
        _factory.DockableAdded -= OnDockableAdded;
        _factory.DockableRemoved -= OnDockableRemoved;
        _factory.DockableClosed -= OnDockableClosed;
        _factory.DockableMoved -= OnDockableMoved;
        _factory.DockableDocked -= OnDockableDocked;
        _factory.DockableUndocked -= OnDockableUndocked;
        _factory.DockableSwapped -= OnDockableSwapped;
        _factory.DockablePinned -= OnDockablePinned;
        _factory.DockableUnpinned -= OnDockableUnpinned;
        _factory.DockableHidden -= OnDockableHidden;
        _factory.DockableRestored -= OnDockableRestored;
        _factory.WindowOpened -= OnWindowOpened;
        _factory.WindowClosing -= OnWindowClosing;
        _factory.WindowClosed -= OnWindowClosed;
        _factory.WindowAdded -= OnWindowAdded;
        _factory.WindowRemoved -= OnWindowRemoved;
        _factory.WindowMoveDragBegin -= OnWindowMoveDragBegin;
        _factory.WindowMoveDrag -= OnWindowMoveDrag;
        _factory.WindowMoveDragEnd -= OnWindowMoveDragEnd;
    }

    private void OnActiveDockableChanged(object? sender, ActiveDockableChangedEventArgs e)
        => _logger.LogInformation("ActiveDockableChanged {Id}", e.Dockable?.Id);

    private void OnFocusedDockableChanged(object? sender, FocusedDockableChangedEventArgs e)
        => _logger.LogInformation("FocusedDockableChanged {Id}", e.Dockable?.Id);

    private void OnDockableInit(object? sender, DockableInitEventArgs e)
        => _logger.LogInformation("DockableInit {Id}", e.Dockable?.Id);

    private void OnDockableAdded(object? sender, DockableAddedEventArgs e)
        => _logger.LogInformation("DockableAdded {Id}", e.Dockable?.Id);

    private void OnDockableRemoved(object? sender, DockableRemovedEventArgs e)
        => _logger.LogInformation("DockableRemoved {Id}", e.Dockable?.Id);

    private void OnDockableClosed(object? sender, DockableClosedEventArgs e)
        => _logger.LogInformation("DockableClosed {Id}", e.Dockable?.Id);

    private void OnDockableMoved(object? sender, DockableMovedEventArgs e)
        => _logger.LogInformation("DockableMoved {Id}", e.Dockable?.Id);

    private void OnDockableDocked(object? sender, DockableDockedEventArgs e)
        => _logger.LogInformation("DockableDocked {Id} {Operation}", e.Dockable?.Id, e.Operation);

    private void OnDockableUndocked(object? sender, DockableUndockedEventArgs e)
        => _logger.LogInformation("DockableUndocked {Id} {Operation}", e.Dockable?.Id, e.Operation);

    private void OnDockableSwapped(object? sender, DockableSwappedEventArgs e)
        => _logger.LogInformation("DockableSwapped {Id}", e.Dockable?.Id);

    private void OnDockablePinned(object? sender, DockablePinnedEventArgs e)
        => _logger.LogInformation("DockablePinned {Id}", e.Dockable?.Id);

    private void OnDockableUnpinned(object? sender, DockableUnpinnedEventArgs e)
        => _logger.LogInformation("DockableUnpinned {Id}", e.Dockable?.Id);

    private void OnDockableHidden(object? sender, DockableHiddenEventArgs e)
        => _logger.LogInformation("DockableHidden {Id}", e.Dockable?.Id);

    private void OnDockableRestored(object? sender, DockableRestoredEventArgs e)
        => _logger.LogInformation("DockableRestored {Id}", e.Dockable?.Id);

    private void OnWindowOpened(object? sender, WindowOpenedEventArgs e)
        => _logger.LogInformation("WindowOpened {Id}", e.Window?.Id);

    private void OnWindowClosing(object? sender, WindowClosingEventArgs e)
        => _logger.LogInformation("WindowClosing {Id} Cancel={Cancel}", e.Window?.Id, e.Cancel);

    private void OnWindowClosed(object? sender, WindowClosedEventArgs e)
        => _logger.LogInformation("WindowClosed {Id}", e.Window?.Id);

    private void OnWindowAdded(object? sender, WindowAddedEventArgs e)
        => _logger.LogInformation("WindowAdded {Id}", e.Window?.Id);

    private void OnWindowRemoved(object? sender, WindowRemovedEventArgs e)
        => _logger.LogInformation("WindowRemoved {Id}", e.Window?.Id);

    private void OnWindowMoveDragBegin(object? sender, WindowMoveDragBeginEventArgs e)
        => _logger.LogInformation("WindowMoveDragBegin {Id} Cancel={Cancel}", e.Window?.Id, e.Cancel);

    private void OnWindowMoveDrag(object? sender, WindowMoveDragEventArgs e)
        => _logger.LogInformation("WindowMoveDrag {Id}", e.Window?.Id);

    private void OnWindowMoveDragEnd(object? sender, WindowMoveDragEndEventArgs e)
        => _logger.LogInformation("WindowMoveDragEnd {Id}", e.Window?.Id);

    /// <inheritdoc />
    public void Dispose()
    {
        Unsubscribe();
    }
}

