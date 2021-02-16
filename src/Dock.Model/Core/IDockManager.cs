using Dock.Model.Controls;

namespace Dock.Model.Core
{
    /// <summary>
    /// Docking manager contract.
    /// </summary>
    public interface IDockManager
    {
        /// <summary>
        /// Gets or sets pointer position relative to event source.
        /// </summary>
        DockPoint Position { get; set; }

        /// <summary>
        /// Gets or sets pointer position relative to event source and translated to screen coordinates.
        /// </summary>
        DockPoint ScreenPosition { get; set; }

        /// <summary>
        /// Validates tool docking operation.
        /// </summary>
        /// <param name="sourceTool">The source tool.</param>
        /// <param name="targetDockable">The target dockable.</param>
        /// <param name="action">The drag action.</param>
        /// <param name="operation">The dock operation.</param>
        /// <param name="bExecute">The flag indicating whether to execute.</param>
        /// <returns>True if docking operation can be executed otherwise false.</returns>
        bool ValidateTool(ITool sourceTool, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute);

        /// <summary>
        /// Validates document docking operation.
        /// </summary>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="targetDockable">The target dockable.</param>
        /// <param name="action">The drag action.</param>
        /// <param name="operation">The dock operation.</param>
        /// <param name="bExecute">The flag indicating whether to execute.</param>
        /// <returns>True if docking operation can be executed otherwise false.</returns>
        bool ValidateDocument(IDocument sourceDocument, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute);

        /// <summary>
        /// Validates dock docking operation.
        /// </summary>
        /// <param name="sourceDock">The source dock.</param>
        /// <param name="targetDockable">The target dockable.</param>
        /// <param name="action">The drag action.</param>
        /// <param name="operation">The dock operation.</param>
        /// <param name="bExecute">The flag indicating whether to execute.</param>
        /// <returns>True if docking operation can be executed otherwise false.</returns>
        bool ValidateDock(IDock sourceDock, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute);

        /// <summary>
        /// Validates dockable docking operation.
        /// </summary>
        /// <param name="sourceDockable">The source dockable.</param>
        /// <param name="targetDockable">The target dockable.</param>
        /// <param name="action">The drag action.</param>
        /// <param name="operation">The dock operation.</param>
        /// <param name="bExecute">The flag indicating whether to execute.</param>
        /// <returns>True if docking operation can be executed otherwise false.</returns>
        bool ValidateDockable(IDockable sourceDockable, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute);
    }
}
