using System.Globalization;

namespace Dock.Model.Core
{
    /// <summary>
    /// Point structure.
    /// </summary>
    public readonly struct DockPoint
    {
        /// <summary>
        /// Gets X coordinate.
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Gets Y coordinate.
        /// </summary>
        public double Y { get; }

        /// <summary>
        /// Initialize the new instance of the <see cref="DockPoint"/>.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public DockPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Returns the string representation of the point.
        /// </summary>
        /// <returns>The string representation of the point.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}, {1}", X, Y);
        }
    }
}
