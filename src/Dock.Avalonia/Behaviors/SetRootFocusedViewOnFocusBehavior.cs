// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using Dock.Model;
using System;

namespace Dock.Avalonia
{
    /// <summary>
    /// Set FocusedView property on PointerPressedEvent event behavior.
    /// </summary>
    public class SetRootFocusedViewOnPointerPressedBehavior : Behavior<Control>
    {
        private IDisposable _disposable;

        private IView FindRoot(IView view)
        {
            if (view.Parent == null)
            {
                return view;
            }
            return FindRoot(view.Parent);
        }

        /// <inheritdoc/>
        protected override void OnAttached()
        {
            base.OnAttached();

            _disposable = AssociatedObject.AddHandler(Control.PointerPressedEvent, (sender, e) =>
            {
                if (AssociatedObject.DataContext is IDock host && host.Factory is IDockFactory factory)
                {
                    if (factory.FindRoot(host.CurrentView) is IViewsHost rootHost)
                    {
                        factory.SetFocusedView(rootHost, host.CurrentView);
                    }
                }
            }, global::Avalonia.Interactivity.RoutingStrategies.Tunnel);
        }

        /// <inheritdoc/>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            _disposable.Dispose();
        }
    }
}
