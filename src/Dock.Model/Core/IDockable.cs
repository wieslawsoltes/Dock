
namespace Dock.Model.Core
{
    /// <summary>
    /// Dockable contract.
    /// </summary>
    public interface IDockable
    {
        /// <summary>
        /// Gets or sets dockable id.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets dockable title.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets dockable context.
        /// </summary>
        object? Context { get; set; }

        /// <summary>
        /// Gets or sets dockable owner.
        /// </summary>
        IDockable? Owner { get; set; }

        /// <summary>
        /// Gets or sets dockable factory.
        /// </summary>
        IFactory? Factory { get; set; }

        /// <summary>
        /// Gets or sets if the dockable can be closed.
        /// </summary>
        bool CanClose { get; set; }

        /// <summary>
        /// Gets or sets if the dockable can be pinned.
        /// </summary>
        bool CanPin { get; set; }

        /// <summary>
        /// Gets or sets if the dockable can be floated.
        /// </summary>
        bool CanFloat { get; set; }

        /// <summary>
        /// Called when the dockable is closed.
        /// </summary>
        /// <returns>true to accept the close, and false to cancel the close.</returns>
        bool OnClose();

        /// <summary>
        /// Called when the dockable becomes the selected dockable.
        /// </summary>
        void OnSelected();
    }
}
