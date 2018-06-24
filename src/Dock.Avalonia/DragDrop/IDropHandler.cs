// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Model;

namespace Dock.Avalonia
{
    /// <summary>
    /// Drop handler contract.
    /// </summary>
    public interface IDropHandler
    {
        /// <summary>
        /// Validate drag operation.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The drag event arguments.</param>
        /// <param name="sourceContext">The source context.</param>
        /// <param name="targetContext">The target context.</param>
        /// <param name="operation">The dock operation.</param>
        /// <returns>True if dock operation can be executed.</returns>
        bool Validate(object sender, DragEventArgs e, object sourceContext, object targetContext, DockOperation operation);

        /// <summary>
        /// Execute drag operation.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The drag event arguments.</param>
        /// <param name="targetContext">The target context.</param>
        /// <param name="sourceContext">The source context.</param>
        /// <param name="operation">The dock operation.</param>
        /// <returns>True if dock operation was successfuly executed.</returns>
        bool Execute(object sender, DragEventArgs e, object targetContext, object sourceContext, DockOperation operation);

        /// <summary>
        /// Cancel drag operation.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        void Cancel(object sender, RoutedEventArgs e);
    }
}
