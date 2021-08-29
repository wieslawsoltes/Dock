using System.Text.Json.Serialization;
using Dock.Model.Controls;

namespace Dock.Model.Core
{
    /// <summary>
    /// Dock window contract.
    /// </summary>
    public interface IDockWindow
    {
        /// <summary>
        /// Gets or sets id.
        /// </summary>
        [JsonInclude]
        string Id { get; set; }

        /// <summary>0
        /// Gets or sets window X coordinate.
        /// </summary>
        [JsonInclude]
        double X { get; set; }

        /// <summary>
        /// Gets or sets window X coordinate.
        /// </summary>
        [JsonInclude]
        double Y { get; set; }

        /// <summary>
        /// Gets or sets window width.
        /// </summary>
        [JsonInclude]
        double Width { get; set; }

        /// <summary>
        /// Gets or sets window height.
        /// </summary>
        [JsonInclude]
        double Height { get; set; }

        /// <summary>
        /// Gets or sets whether this window appears on top of all other windows.
        /// </summary>
        [JsonInclude]
        bool Topmost { get; set; }

        /// <summary>
        /// Gets or sets window title.
        /// </summary>
        [JsonInclude]
        string Title { get; set; }

        /// <summary>
        /// Gets or sets window owner dockable.
        /// </summary>
        [JsonIgnore]
        IDockable? Owner { get; set; }

        /// <summary>
        /// Gets or sets dock factory.
        /// </summary>
        [JsonIgnore]
        IFactory? Factory { get; set; }

        /// <summary>
        /// Gets or sets layout.
        /// </summary>
        [JsonInclude]
        IRootDock? Layout { get; set; }

        /// <summary>
        /// Gets or sets dock window.
        /// </summary>
        [JsonIgnore]
        IHostWindow? Host { get; set; }

        /// <summary>
        /// Called when the window is closed.
        /// </summary>
        /// <returns>True to accept the close, and false to cancel the close.</returns>
        bool OnClose();

        /// <summary>
        /// Called before the window dragging start.
        /// </summary>
        /// <returns>True to accept the dragging, and false to cancel the dragging.</returns>
        bool OnMoveDragBegin();

        /// <summary>
        /// Called when the window is dragged.
        /// </summary>
        void OnMoveDrag();

        /// <summary>
        /// Called after the window dragging ended.
        /// </summary>
        void OnMoveDragEnd();

        /// <summary>
        /// Saves window properties.
        /// </summary>
        void Save();

        /// <summary>
        /// Presents window.
        /// </summary>
        /// <param name="isDialog">The value that indicates whether window is dialog.</param>
        void Present(bool isDialog);

        /// <summary>
        /// Exits window.
        /// </summary>
        void Exit();
    }
}
