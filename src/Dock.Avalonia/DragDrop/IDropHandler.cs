// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Dock.Avalonia
{
    /// <summary>
    /// Drop handler contract.
    /// </summary>
    public interface IDropHandler
    {
        /// <summary>
        /// Perform enter operation.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The drag event arguments.</param>
        /// <param name="sourceContext">The source context.</param>
        /// <param name="targetContext">The target context.</param>
        void Enter(object sender, DragEventArgs e, object sourceContext, object targetContext);

        /// <summary>
        /// Perform over operation.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The drag event arguments.</param>
        /// <param name="sourceContext">The source context.</param>
        /// <param name="targetContext">The target context.</param>
        void Over(object sender, DragEventArgs e, object sourceContext, object targetContext);

        /// <summary>
        /// Perform drop operation.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The drag event arguments.</param>
        /// <param name="sourceContext">The source context.</param>
        /// <param name="targetContext">The target context.</param>
        void Drop(object sender, DragEventArgs e, object sourceContext, object targetContext);

        /// <summary>
        /// Perform leave operation.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        void Leave(object sender, RoutedEventArgs e);

        /// <summary>
        /// Validate drag operation.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The drag event arguments.</param>
        /// <param name="sourceContext">The source context.</param>
        /// <param name="targetContext">The target context.</param>
        /// <param name="state">The state object.</param>
        /// <returns>True if drag operation can be executed.</returns>
        bool Validate(object sender, DragEventArgs e, object sourceContext, object targetContext, object state);

        /// <summary>
        /// Execute drag operation.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The drag event arguments.</param>
        /// <param name="sourceContext">The source context.</param>
        /// <param name="targetContext">The target context.</param>
        /// <param name="state">The state object.</param>
        /// <returns>True if drag operation was successfuly executed.</returns>
        bool Execute(object sender, DragEventArgs e, object sourceContext, object targetContext, object state);

        /// <summary>
        /// Cancel drag operation.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        void Cancel(object sender, RoutedEventArgs e);
    }
}
