namespace Dock.Model.Core;

internal static class DockOperationExtensions
{
    public static Alignment ToAlignment(this DockOperation operation)
    {
        return operation switch
        {
            DockOperation.Left => Alignment.Left,
            DockOperation.Bottom => Alignment.Bottom,
            DockOperation.Right => Alignment.Right,
            DockOperation.Top => Alignment.Top,
            DockOperation.TopLeft => Alignment.Top,
            DockOperation.TopRight => Alignment.Top,
            DockOperation.BottomLeft => Alignment.Bottom,
            DockOperation.BottomRight => Alignment.Bottom,
            _ => Alignment.Unset
        };
    }
}
