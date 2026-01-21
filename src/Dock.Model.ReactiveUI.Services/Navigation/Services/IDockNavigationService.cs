using Dock.Model.Core;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Navigation.Services;

/// <summary>
/// Provides navigation helpers for opening dockables in the current document context.
/// </summary>
public interface IDockNavigationService
{
    /// <summary>
    /// Attaches the factory and host screen used as a fallback navigation context.
    /// </summary>
    /// <param name="factory">The dock factory.</param>
    /// <param name="host">The host screen.</param>
    void AttachFactory(IFactory factory, IScreen host);

    /// <summary>
    /// Opens the specified document dockable in the appropriate document dock.
    /// </summary>
    /// <param name="hostScreen">The navigation host screen.</param>
    /// <param name="document">The document to open.</param>
    /// <param name="floatWindow">True to float the document into a new window.</param>
    void OpenDocument(IScreen hostScreen, IDockable document, bool floatWindow);
}
