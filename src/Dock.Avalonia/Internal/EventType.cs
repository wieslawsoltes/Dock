
namespace Dock.Avalonia.Internal
{
    /// <summary>
    /// Pointer event type.
    /// </summary>
    internal enum EventType
    {
        /// <summary>
        /// Pointer pressed.
        /// </summary>
        Pressed,
        /// <summary>
        /// Pointer released.
        /// </summary>
        Released,
        /// <summary>
        /// Pointer moved.
        /// </summary>
        Moved,
        /// <summary>
        /// Pointer enter.
        /// </summary>
        Enter,
        /// <summary>
        /// Pointer leave.
        /// </summary>
        Leave,
        /// <summary>
        /// Lost capture.
        /// </summary>
        CaptureLost,
        /// <summary>
        /// Wheel changed.
        /// </summary>
        WheelChanged
    }
}
