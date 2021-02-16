
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

        /// <summary>
        /// Gets dockable visible bounds information used for tracking.
        /// </summary>
        /// <param name="x">The dockable x axis position.</param>
        /// <param name="y">The dockable y axis position.</param>
        /// <param name="width">The dockable width.</param>
        /// <param name="height">The dockable height.</param>
        void GetVisibleBounds(out double x, out double y, out double width, out double height);

        /// <summary>
        /// Sets dockable visible bounds information used for tracking.
        /// </summary>
        /// <param name="x">The dock x axis position.</param>
        /// <param name="y">The dock y axis position.</param>
        /// <param name="width">The dockable width.</param>
        /// <param name="height">The dockable height.</param>
        void SetVisibleBounds(double x, double y, double width, double height);

        /// <summary>
        /// Called when dockable visible bounds changed.
        /// </summary>
        /// <param name="x">The dock x axis position.</param>
        /// <param name="y">The dock y axis position.</param>
        /// <param name="width">The dockable width.</param>
        /// <param name="height">The dockable height.</param>
        void OnVisibleBoundsChanged(double x, double y, double width, double height);

        /// <summary>
        /// Gets dockable pinned bounds information used for tracking.
        /// </summary>
        /// <param name="x">The dockable x axis position.</param>
        /// <param name="y">The dockable y axis position.</param>
        /// <param name="width">The dockable width.</param>
        /// <param name="height">The dockable height.</param>
        void GetPinnedBounds(out double x, out double y, out double width, out double height);

        /// <summary>
        /// Sets dockable pinned bounds information used for tracking.
        /// </summary>
        /// <param name="x">The dock x axis position.</param>
        /// <param name="y">The dock y axis position.</param>
        /// <param name="width">The dockable width.</param>
        /// <param name="height">The dockable height.</param>
        void SetPinnedBounds(double x, double y, double width, double height);

        /// <summary>
        /// Called when dockable pinned bounds changed.
        /// </summary>
        /// <param name="x">The dock x axis position.</param>
        /// <param name="y">The dock y axis position.</param>
        /// <param name="width">The dockable width.</param>
        /// <param name="height">The dockable height.</param>
        void OnPinnedBoundsChanged(double x, double y, double width, double height);

        /// <summary>
        /// Gets dockable tab bounds information used for tracking.
        /// </summary>
        /// <param name="x">The dockable x axis position.</param>
        /// <param name="y">The dockable y axis position.</param>
        /// <param name="width">The dockable width.</param>
        /// <param name="height">The dockable height.</param>
        void GetTabBounds(out double x, out double y, out double width, out double height);

        /// <summary>
        /// Sets dockable tab bounds information used for tracking.
        /// </summary>
        /// <param name="x">The dock x axis position.</param>
        /// <param name="y">The dock y axis position.</param>
        /// <param name="width">The dockable width.</param>
        /// <param name="height">The dockable height.</param>
        void SetTabBounds(double x, double y, double width, double height);

        /// <summary>
        /// Called when dockable tab bounds changed.
        /// </summary>
        /// <param name="x">The dock x axis position.</param>
        /// <param name="y">The dock y axis position.</param>
        /// <param name="width">The dockable width.</param>
        /// <param name="height">The dockable height.</param>
        void OnTabBoundsChanged(double x, double y, double width, double height);

        /// <summary>
        /// Gets dockable pointer position used for tracking.
        /// </summary>
        /// <param name="x">The pointer x axis position.</param>
        /// <param name="y">The pointer y axis position.</param>
        void GetPointerPosition(out double x, out double y);

        /// <summary>
        /// Sets dockable pointer position used for tracking.
        /// </summary>
        /// <param name="x">The pointer x axis position.</param>
        /// <param name="y">The pointer y axis position.</param>
        void SetPointerPosition(double x, double y);

        /// <summary>
        /// Called when dockable pointer position changed.
        /// </summary>
        /// <param name="x">The pointer x axis position.</param>
        /// <param name="y">The pointer y axis position.</param>
        void OnPointerPositionChanged(double x, double y);

        /// <summary>
        /// Gets dockable pointer screen position used for tracking.
        /// </summary>
        /// <param name="x">The pointer x axis position.</param>
        /// <param name="y">The pointer y axis position.</param>
        void GetPointerScreenPosition(out double x, out double y);

        /// <summary>
        /// Sets dockable pointer screen position used for tracking.
        /// </summary>
        /// <param name="x">The pointer x axis position.</param>
        /// <param name="y">The pointer y axis position.</param>
        void SetPointerScreenPosition(double x, double y);

        /// <summary>
        /// Called when dockable pointer screen position changed.
        /// </summary>
        /// <param name="x">The pointer x axis position.</param>
        /// <param name="y">The pointer y axis position.</param>
        void OnPointerScreenPositionChanged(double x, double y);
    }
}
