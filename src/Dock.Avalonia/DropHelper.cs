// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Dock.Avalonia
{
    /// <summary>
    /// Drop helper.
    /// </summary>
    public static class DropHelper
    {
        private static Point TransformPoint(Matrix matrix, Point point)
        {
            return new Point(
                (point.X * matrix.M11) + (point.Y * matrix.M21) + matrix.M31,
                (point.X * matrix.M12) + (point.Y * matrix.M22) + matrix.M32);
        }

        private static Point FixInvalidPosition(IControl control, Point point)
        {
            var matrix = control?.RenderTransform?.Value;
            return matrix != null ? TransformPoint(matrix.Value.Invert(), point) : point;
        }

        /// <summary>
        /// Calculates fixed drag position relative to event source.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        /// <returns>The fixed drag position relative to event source.</returns>
        public static Point GetPosition(object sender, DragEventArgs e)
        {
            var relativeTo = e.Source as IControl;
            var point = e.GetPosition(relativeTo);
            return FixInvalidPosition(relativeTo, point);
        }

        /// <summary>
        /// Calculates fixed drag position relative to event source and translated to screen coordinates.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        /// <returns>The fixed drag position relative to event source and translated to screen coordinates.</returns>
        public static Point GetPositionScreen(object sender, DragEventArgs e)
        {
            var relativeTo = e.Source as IControl;
            var point = e.GetPosition(relativeTo);
            var visual = relativeTo as IVisual;
            var screenPoint = visual.PointToScreen(point);
            return FixInvalidPosition(relativeTo, screenPoint);
        }
    }
}
