// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Styling;

namespace Dock.Avalonia.Controls
{
    /// <summary>
    /// Interaction logic for <see cref="HostWindow"/> xaml.
    /// </summary>
    public class HostWindow : HostWindowBase
    {
        private Control _titleBar;
        private bool _mouseDown;
        private Point _mouseDownPosition;        

        /// <summary>
        /// Attaches grip to chrome.
        /// </summary>
        /// <param name="chrome">The chrome control.</param>
        public void AttachGrip(DockToolChrome chrome)
        {
            Observable.FromEventPattern(chrome.CloseButton, nameof(Button.Click)).Subscribe(o =>
            {
                Close();
            });

            //Observable.FromEventPattern(chrome.MinimiseButton, nameof(Button.Click)).Subscribe(o =>
            //{
            //    WindowState = WindowState.Minimized;
            //});

            _titleBar = chrome.Grip;

            ((IPseudoClasses)chrome.Classes).Add(":floating");
            this.PseudoClasses.Set(":toolwindow", true);
        }

        /// <inheritdoc/>
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (_titleBar != null)
            {
                if (_titleBar.IsPointerOver)
                {
                    _mouseDown = true;
                    _mouseDownPosition = e.GetPosition(this);
                }
                else
                {
                    _mouseDown = false;
                }
            }

            base.OnPointerPressed(e);
        }

        /// <inheritdoc/>
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            _mouseDown = false;
            base.OnPointerReleased(e);
        }

        /// <inheritdoc/>
        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (_titleBar != null)
            {
                if (_titleBar.IsPointerOver && _mouseDown)
                {
                    //if (mouseDownPosition.DistanceTo(e.GetPosition(this)) > 12)
                    {
                        WindowState = WindowState.Normal;
                        BeginMoveDrag();
                        _mouseDown = false;
                    }
                }
            }

            base.OnPointerMoved(e);
        }

        /// <inheritdoc/>
        protected override void OnTemplateApplied(TemplateAppliedEventArgs e)
        {
            base.OnTemplateApplied(e);
        }
    }
}
