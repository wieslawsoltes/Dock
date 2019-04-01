// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Dock.Avalonia
{
    /// <summary>
    /// Default drop handler.
    /// </summary>
    public abstract class DefaultDropHandler : IDropHandler
    {
        /// <inheritdoc/>
        public virtual void Enter(object sender, DragEventArgs e, object sourceContext, object targetContext)
        {
            if (Validate(sender, e, sourceContext, targetContext, null) == false)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
            }
            else
            {
                e.DragEffects |= DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
                e.Handled = true;
            }
        }

        /// <inheritdoc/>
        public virtual void Over(object sender, DragEventArgs e, object sourceContext, object targetContext)
        {
            if (Validate(sender, e, sourceContext, targetContext, null) == false)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
            }
            else
            {
                e.DragEffects |= DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
                e.Handled = true;
            }
        }

        /// <inheritdoc/>
        public virtual void Drop(object sender, DragEventArgs e, object sourceContext, object targetContext)
        {
            if (Execute(sender, e, sourceContext, targetContext, null) == false)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
            }
            else
            {
                e.DragEffects |= DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
                e.Handled = true;
            }
        }

        /// <inheritdoc/>
        public virtual void Leave(object sender, RoutedEventArgs e)
        {
            Cancel(sender, e);
        }

        /// <inheritdoc/>
        public virtual bool Validate(object sender, DragEventArgs e, object sourceContext, object targetContext, object state)
        {
            return false;
        }
        
        /// <inheritdoc/>
        public virtual bool Execute(object sender, DragEventArgs e, object sourceContext, object targetContext, object state)
        {
            return false;
        }

        /// <inheritdoc/>
        public virtual void Cancel(object sender, RoutedEventArgs e)
        {
        }
    }
}
