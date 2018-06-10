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
    /// Set FocusedView property on PointerPressedEvent event behavior.
    /// </summary>
    public class SetRootFocusedViewOnPointerPressedBehavior : Behavior<Control>
    {
        private IDisposable _disposable;

        /// <inheritdoc/>
        protected override void OnAttached()
        {
            base.OnAttached();

            _disposable = AssociatedObject.AddHandler(Control.PointerPressedEvent, (sender, e) =>
            {
                if (AssociatedObject.DataContext is IDock dock && dock.Factory is IDockFactory factory)
                {
                    if (dock.CurrentView != null)
                    {
                        if (factory.FindRoot(dock.CurrentView) is IDock root)
                        {
                            factory.SetFocusedView(root, dock.CurrentView);
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
