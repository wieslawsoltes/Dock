// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using Dock.Model;

namespace Dock.Avalonia
{
    /// <summary>
    /// Set focused dockable property on pointer pressed behavior.
    /// </summary>
    public class SeFocusedOnPointerPressedBehavior : Behavior<Control>
    {
        private IDisposable _disposable;

        /// <inheritdoc/>
        protected override void OnAttached()
        {
            base.OnAttached();

            _disposable = AssociatedObject.AddHandler(Control.PointerPressedEvent, (sender, e) =>
            {
                if (AssociatedObject.DataContext is IDock dock && dock.Factory is IFactory factory)
                {
                    if (dock.CurrentDockable != null)
                    {
                        if (factory.FindRoot(dock.CurrentDockable) is IDock root)
                        {
                            factory.SetFocusedDockable(root, dock.CurrentDockable);
                        }
                    }
                }
            }, RoutingStrategies.Tunnel);
        }

        /// <inheritdoc/>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            _disposable.Dispose();
        }
    }
}
