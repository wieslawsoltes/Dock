// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Model;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Interaction logic for <see cref="ToolControl"/> xaml.
    /// </summary>
    public class ToolControl : TemplatedControl
    {
        private IDisposable _disposable;

        /// <inheritdoc/>
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            _disposable = AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Tunnel);
        }

        /// <inheritdoc/>
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            _disposable.Dispose();
        }

        private void Pressed(object sender, PointerPressedEventArgs e)
        {
            if (this.DataContext is IDock dock && dock.Factory is IFactory factory)
            {
                if (dock.AvtiveDockable != null)
                {
                    if (factory.FindRoot(dock.AvtiveDockable) is IDock root)
                    {
                        Debug.WriteLine($"{nameof(ToolControl)} SetFocusedDockable {dock.AvtiveDockable}");
                        factory.SetFocusedDockable(root, dock.AvtiveDockable);
                    }
                }
            }
        }
    }
}
