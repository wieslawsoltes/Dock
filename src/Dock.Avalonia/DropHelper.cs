// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Dock.Avalonia
{
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

        public static Point GetPosition(object sender, DragEventArgs e)
        {
            var relativeTo = e.Source as IControl;
            var point = e.GetPosition(relativeTo);
            return FixInvalidPosition(relativeTo, point);
        }

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
