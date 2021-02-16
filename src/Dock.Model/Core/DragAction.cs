using System;

namespace Dock.Model.Core
{
    /// <summary>
    /// Defines the available drag actions.
    /// </summary>
    [Flags]
    public enum DragAction
    {
        /// <summary>
        /// No action.
        /// </summary>
        None = 0,

        /// <summary>
        /// Copy action.
        /// </summary>
        Copy = 1,

        /// <summary>
        /// Move action.
        /// </summary>
        Move = 2,

        /// <summary>
        /// Link action.
        /// </summary>
        Link = 4
    }
}
