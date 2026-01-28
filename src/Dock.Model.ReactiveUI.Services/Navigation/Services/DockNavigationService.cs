using System;
using System.Linq;
using Dock.Model.Controls;
using Dock.Model.Core;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Navigation.Services;

/// <summary>
/// Default navigation service that opens documents in the current document dock context.
/// </summary>
public class DockNavigationService : IDockNavigationService
{
    private IFactory? _factory;
    private IScreen? _host;

    /// <inheritdoc />
    public void AttachFactory(IFactory factory, IScreen host)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    /// <inheritdoc />
    public void OpenDocument(IScreen hostScreen, IDockable document, bool floatWindow)
    {
        if (hostScreen is null)
        {
            throw new ArgumentNullException(nameof(hostScreen));
        }

        if (document is null)
        {
            throw new ArgumentNullException(nameof(document));
        }

        if (!TryGetDocumentContext(hostScreen, out var factory, out var documentDock))
        {
            return;
        }

        if (document.Owner is IDock ownerDock
            && ownerDock.VisibleDockables?.Contains(document) == true)
        {
            factory.SetActiveDockable(document);
            factory.SetFocusedDockable(ownerDock, document);
            factory.ActivateWindow(document);

            if (floatWindow)
            {
                factory.FloatDockable(document);
            }

            return;
        }

        factory.AddDockable(documentDock, document);
        factory.SetActiveDockable(document);
        factory.SetFocusedDockable(documentDock, document);

        if (floatWindow)
        {
            factory.FloatDockable(document);
        }
    }

    /// <summary>
    /// Resolves the document dock and factory for the supplied host screen.
    /// </summary>
    /// <param name="hostScreen">The host screen.</param>
    /// <param name="factory">The resolved factory.</param>
    /// <param name="documentDock">The resolved document dock.</param>
    /// <returns>True when the document dock and factory are resolved.</returns>
    protected bool TryGetDocumentContext(
        IScreen? hostScreen,
        out IFactory factory,
        out IDocumentDock documentDock)
    {
        factory = null!;
        documentDock = null!;

        if (TryResolveDocumentDock(hostScreen, out factory, out documentDock))
        {
            return true;
        }

        if (_host is not null
            && !ReferenceEquals(hostScreen, _host)
            && TryResolveDocumentDock(_host, out factory, out documentDock))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Resolves the document dock and factory by walking the owner chain.
    /// </summary>
    /// <param name="hostScreen">The host screen.</param>
    /// <param name="factory">The resolved factory.</param>
    /// <param name="documentDock">The resolved document dock.</param>
    /// <returns>True when the document dock and factory are resolved.</returns>
    protected bool TryResolveDocumentDock(
        IScreen? hostScreen,
        out IFactory factory,
        out IDocumentDock documentDock)
    {
        factory = null!;
        documentDock = null!;

        if (hostScreen is not IDockable dockable)
        {
            return false;
        }

        var current = dockable;
        IFactory? resolvedFactory = null;
        IDocumentDock? resolvedDock = null;

        while (current is not null)
        {
            if (resolvedFactory is null && current.Factory is IFactory dockFactory)
            {
                resolvedFactory = dockFactory;
            }

            if (resolvedDock is null && current is IDocumentDock dock)
            {
                resolvedDock = dock;
            }

            if (resolvedFactory is not null && resolvedDock is not null)
            {
                break;
            }

            current = current.Owner;
        }

        resolvedFactory ??= _factory;

        if (resolvedDock is null && resolvedFactory is not null)
        {
            var root = resolvedFactory.FindRoot(dockable);
            resolvedDock = root?.DefaultDockable as IDocumentDock
                ?? root?.VisibleDockables?.OfType<IDocumentDock>().FirstOrDefault();
        }

        if (resolvedFactory is null || resolvedDock is null)
        {
            return false;
        }

        factory = resolvedFactory;
        documentDock = resolvedDock;
        return true;
    }
}
