using Dock.Model.Core;
using Dock.Model.Core.Events;
using Microsoft.Extensions.Logging;

namespace Dock.Model.Extensions.DependencyInjection;

/// <summary>
/// Logs events raised by <see cref="FactoryBase"/>.
/// </summary>
public sealed class DockEventLogger : IDisposable
{
    private readonly FactoryBase _factory;
    private readonly ILogger<DockEventLogger> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockEventLogger"/> class.
    /// </summary>
    /// <param name="factory">The dock factory.</param>
    /// <param name="logger">The logger instance.</param>
    public DockEventLogger(FactoryBase factory, ILogger<DockEventLogger> logger)
    {
        _factory = factory;
        _logger = logger;

        Subscribe();
    }

    private void Subscribe()
    {
        _factory.ActiveDockableChanged += OnActiveDockableChanged;
        _factory.DockableAdded += OnDockableAdded;
        _factory.DockableRemoved += OnDockableRemoved;
        _factory.WindowOpened += OnWindowOpened;
        _factory.WindowClosed += OnWindowClosed;
    }

    private void Unsubscribe()
    {
        _factory.ActiveDockableChanged -= OnActiveDockableChanged;
        _factory.DockableAdded -= OnDockableAdded;
        _factory.DockableRemoved -= OnDockableRemoved;
        _factory.WindowOpened -= OnWindowOpened;
        _factory.WindowClosed -= OnWindowClosed;
    }

    private void OnActiveDockableChanged(object? sender, ActiveDockableChangedEventArgs e)
    {
        _logger.LogInformation("Active dockable changed: {Title}", e.Dockable?.Title);
    }

    private void OnDockableAdded(object? sender, DockableAddedEventArgs e)
    {
        _logger.LogInformation("Dockable added: {Title}", e.Dockable?.Title);
    }

    private void OnDockableRemoved(object? sender, DockableRemovedEventArgs e)
    {
        _logger.LogInformation("Dockable removed: {Title}", e.Dockable?.Title);
    }

    private void OnWindowOpened(object? sender, WindowOpenedEventArgs e)
    {
        _logger.LogInformation("Window opened: {Title}", e.Window?.Title);
    }

    private void OnWindowClosed(object? sender, WindowClosedEventArgs e)
    {
        _logger.LogInformation("Window closed: {Title}", e.Window?.Title);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Unsubscribe();
    }
}

