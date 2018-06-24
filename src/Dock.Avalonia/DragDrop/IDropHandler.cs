// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Model;

namespace Dock.Avalonia
{
    /// <summary>
    /// Dock drop handler contract.
    /// </summary>
    public interface IDropHandler
    {
        /// <summary>
        /// Validate drag operation.
        /// </summary>
        /// <param name="sourceContext">The source context.</param>
        /// <param name="targetContext">The target context.</param>
        /// <param name="sender">The sender object.</param>
        /// <param name="operation">The dock operation.</param>
        /// <param name="e">The drag event arguments.</param>
        /// <returns>True if dock operation can be executed.</returns>
        bool Validate(object sourceContext, object targetContext, object sender, DockOperation operation, DragEventArgs e);

        /// <summary>
        /// Execute drag operation.
        /// </summary>
        /// <param name="sourceContext">The source context.</param>
        /// <param name="targetContext">The target context.</param>
        /// <param name="sender">The sender object.</param>
        /// <param name="operation">The dock operation.</param>
        /// <param name="e">The drag event arguments.</param>
        /// <returns>True if dock operation was successfuly executed.</returns>
        bool Execute(object sourceContext, object targetContext, object sender, DockOperation operation, DragEventArgs e);

        /// <summary>
        /// Cancel drag operation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Cancel(object sender, RoutedEventArgs e);
    }
}
