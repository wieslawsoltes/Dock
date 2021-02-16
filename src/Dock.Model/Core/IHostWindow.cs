namespace Dock.Model.Core
{
    /// <summary>
    /// Host window contract.
    /// </summary>
    public interface IHostWindow
    {
        /// <summary>
        /// Gets dock manager.
        /// </summary>
        IDockManager? DockManager { get; }

        /// <summary>
        /// Gets host window state.
        /// </summary>
        IHostWindowState? HostWindowState { get; }

        /// <summary>
        /// Gets or sets value that indicates whether host size and position is tracked.
        /// </summary>
        bool IsTracked { get; set; }

        /// <summary>
        /// Gets or sets dock window.
        /// </summary>
        IDockWindow? Window { get; set; }

        /// <summary>
        /// Presents host.
        /// </summary>
        /// <param name="isDialog">The value that indicates whether window is dialog.</param>
        void Present(bool isDialog);

        /// <summary>
        /// Exits host.
        /// </summary>
        void Exit();

        /// <summary>
        /// Sets host position.
        /// </summary>
        /// <param name="x">The X coordinate of host.</param>
        /// <param name="y">The Y coordinate of host.</param>
        void SetPosition(double x, double y);

        /// <summary>
        /// Gets host position.
        /// </summary>
        /// <param name="x">The X coordinate of host.</param>
        /// <param name="y">The Y coordinate of host.</param>
        void GetPosition(out double x, out double y);

        /// <summary>
        /// Sets host size.
        /// </summary>
        /// <param name="width">The host width.</param>
        /// <param name="height">The host height.</param>
        void SetSize(double width, double height);

        /// <summary>
        /// Gets host size.
        /// </summary>
        /// <param name="width">The host width.</param>
        /// <param name="height">The host height.</param>
        void GetSize(out double width, out double height);

        /// <summary>
        /// Sets host topmost.
        /// </summary>
        /// <param name="topmost">The host topmost.</param>
        void SetTopmost(bool topmost);

        /// <summary>
        /// Sets host title.
        /// </summary>
        /// <param name="title">The host title.</param>
        void SetTitle(string title);

        /// <summary>
        /// Sets host layout.
        /// </summary>
        /// <param name="layout">The host layout.</param>
        void SetLayout(IDock layout);
    }
}
