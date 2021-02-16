using Dock.Model.Core;

namespace Dock.Model.Adapters
{
    /// <summary>
    /// Tracking adapter for the <see cref="IDockable"/>.
    /// </summary>
    public class TrackingAdapter
    {
        private double _xVisible;
        private double _yVisible;
        private double _widthVisible;
        private double _heightVisible;
        private double _xPinned;
        private double _yPinned;
        private double _widthPinned;
        private double _heightPinned;
        private double _xTab;
        private double _yTab;
        private double _widthTab;
        private double _heightTab;
        private double _pointerX;
        private double _pointerY;
        private double _pointerScreenX;
        private double _pointerScreenY;

        /// <summary>
        /// Initializes the new instance of <see cref="TrackingAdapter"/> class.
        /// </summary>
        public TrackingAdapter()
        {
            _xVisible = double.NaN;
            _yVisible = double.NaN;
            _widthVisible = double.NaN;
            _heightVisible = double.NaN;
            _xPinned = double.NaN;
            _yPinned = double.NaN;
            _widthPinned = double.NaN;
            _heightPinned = double.NaN;
            _xTab = double.NaN;
            _yTab = double.NaN;
            _widthTab = double.NaN;
            _heightTab = double.NaN;
            _pointerX = double.NaN;
            _pointerY = double.NaN;
            _pointerScreenX = double.NaN;
            _pointerScreenY = double.NaN;
        }
        
        /// <summary>
        /// Gets dockable visible bounds information used for tracking.
        /// </summary>
        /// <param name="x">The dockable x axis position.</param>
        /// <param name="y">The dockable y axis position.</param>
        /// <param name="width">The dockable width.</param>
        /// <param name="height">The dockable height.</param>
        public void GetVisibleBounds(out double x, out double y, out double width, out double height)
        {
            x = _xVisible;
            y = _yVisible;
            width = _widthVisible;
            height = _heightVisible;
        }

        /// <summary>
        /// Sets dockable visible bounds information used for tracking.
        /// </summary>
        /// <param name="x">The dock x axis position.</param>
        /// <param name="y">The dock y axis position.</param>
        /// <param name="width">The dockable width.</param>
        /// <param name="height">The dockable height.</param>
        public void SetVisibleBounds(double x, double y, double width, double height)
        {
            _xVisible = x;
            _yVisible = y;
            _widthVisible = width;
            _heightVisible = height;
        }

        /// <summary>
        /// Gets dockable pinned bounds information used for tracking.
        /// </summary>
        /// <param name="x">The dockable x axis position.</param>
        /// <param name="y">The dockable y axis position.</param>
        /// <param name="width">The dockable width.</param>
        /// <param name="height">The dockable height.</param>
        public void GetPinnedBounds(out double x, out double y, out double width, out double height)
        {
            x = _xPinned;
            y = _yPinned;
            width = _widthPinned;
            height = _heightPinned;
        }

        /// <summary>
        /// Sets dockable pinned bounds information used for tracking.
        /// </summary>
        /// <param name="x">The dock x axis position.</param>
        /// <param name="y">The dock y axis position.</param>
        /// <param name="width">The dockable width.</param>
        /// <param name="height">The dockable height.</param>
        public void SetPinnedBounds(double x, double y, double width, double height)
        {
            _xPinned = x;
            _yPinned = y;
            _widthPinned = width;
            _heightPinned = height;
        }

        /// <summary>
        /// Gets dockable tab bounds information used for tracking.
        /// </summary>
        /// <param name="x">The dockable x axis position.</param>
        /// <param name="y">The dockable y axis position.</param>
        /// <param name="width">The dockable width.</param>
        /// <param name="height">The dockable height.</param>
        public void GetTabBounds(out double x, out double y, out double width, out double height)
        {
            x = _xTab;
            y = _yTab;
            width = _widthTab;
            height = _heightTab;
        }

        /// <summary>
        /// Sets dockable tab bounds information used for tracking.
        /// </summary>
        /// <param name="x">The dock x axis position.</param>
        /// <param name="y">The dock y axis position.</param>
        /// <param name="width">The dockable width.</param>
        /// <param name="height">The dockable height.</param>
        public void SetTabBounds(double x, double y, double width, double height)
        {
            _xTab = x;
            _yTab = y;
            _widthTab = width;
            _heightTab = height;
        }

        /// <summary>
        /// Gets dockable pointer position used for tracking.
        /// </summary>
        /// <param name="x">The pointer x axis position.</param>
        /// <param name="y">The pointer y axis position.</param>
        public void GetPointerPosition(out double x, out double y)
        {
            x = _pointerX;
            y = _pointerY;
        }

        /// <summary>
        /// Sets dockable pointer position used for tracking.
        /// </summary>
        /// <param name="x">The pointer x axis position.</param>
        /// <param name="y">The pointer y axis position.</param>
        public void SetPointerPosition(double x, double y)
        {
            _pointerX = x;
            _pointerY = y;
        }

        /// <summary>
        /// Gets dockable pointer screen position used for tracking.
        /// </summary>
        /// <param name="x">The pointer x axis position.</param>
        /// <param name="y">The pointer y axis position.</param>
        public void GetPointerScreenPosition(out double x, out double y)
        {
            x = _pointerScreenX;
            y = _pointerScreenY;
        }

        /// <summary>
        /// Sets dockable pointer screen position used for tracking.
        /// </summary>
        /// <param name="x">The pointer x axis position.</param>
        /// <param name="y">The pointer y axis position.</param>
        public void SetPointerScreenPosition(double x, double y)
        {
            _pointerScreenX = x;
            _pointerScreenY = y;
        }
    }
}
